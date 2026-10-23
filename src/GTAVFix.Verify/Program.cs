using GTAVFix.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Globalization;
using System.Text;

if(args.Length==1&&args[0]=="--localization")
{
    string[] expected=["en","fr","it","de","es","es-419","ja","ko","pl","pt-BR","ru","zh-Hans","zh-Hant"];
    if(!Text.Languages.Order().SequenceEqual(expected.Order()))throw new Exception("Language catalog mismatch");
    var baseline=Text.Entries("en");
    foreach(string language in expected)
    {
        var entries=Text.Entries(language);
        if(!entries.Keys.Order().SequenceEqual(baseline.Keys.Order())||entries.Values.Any(string.IsNullOrWhiteSpace))throw new Exception("Incomplete catalog: "+language);
        foreach(var entry in entries)
        {
            var format=CompositeFormat.Parse(entry.Value);var original=CompositeFormat.Parse(baseline[entry.Key]);
            if(format.MinimumArgumentCount!=original.MinimumArgumentCount)throw new Exception("Placeholder mismatch: "+language+"/"+entry.Key);
            _=Text.ForLanguage(language,entry.Key,Enumerable.Repeat<object>("sample",format.MinimumArgumentCount).ToArray());
        }
    }
    var cases=new Dictionary<string,string>{["zh-TW"]="zh-Hant",["zh-HK"]="zh-Hant",["zh-MO"]="zh-Hant",["zh-Hant"]="zh-Hant",["zh-CN"]="zh-Hans",["zh-SG"]="zh-Hans",["zh-Hans"]="zh-Hans",["en-US"]="en",["en-GB"]="en",["fr-CA"]="fr",["it-IT"]="it",["de-AT"]="de",["es-ES"]="es",["es-MX"]="es-419",["es-AR"]="es-419",["es-US"]="es-419",["es-419"]="es-419",["pt-BR"]="pt-BR",["pt-PT"]="pt-BR",["ja-JP"]="ja",["ko-KR"]="ko",["pl-PL"]="pl",["ru-RU"]="ru",["tr-TR"]="en",["uk-UA"]="en",["ar-SA"]="en",["th-TH"]="en",[""]="en"};
    foreach(var item in cases)if(Text.Resolve(CultureInfo.GetCultureInfo(item.Key))!=item.Value)throw new Exception("Culture mismatch: "+item.Key);
    var saved=CultureInfo.CurrentUICulture;
    try{foreach(var item in cases){CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(item.Key);if(Text.Get("Title")!=Text.ForLanguage(item.Value,"Title"))throw new Exception("UI culture not applied");}}
    finally{CultureInfo.CurrentUICulture=saved;}
    if(!GameLanguages.All.Select(x=>x.Code).Order().SequenceEqual(expected.Order()))throw new Exception("Game language coverage mismatch");
    Console.WriteLine($"PASS: {expected.Length} complete catalogs, {baseline.Count} strings each, format placeholders, {cases.Count} culture/fallback cases, actual UI-culture selection and game-language list.");return;
}

if(args.Length==2&&args[0]=="--install-plan")
{
    var plan=JsonSerializer.Deserialize<PatchPlan>(File.ReadAllText(args[1]))??throw new InvalidDataException("Invalid plan");
    new PatchService(Console.WriteLine).Install(plan);return;
}

if(args.Length==3&&args[0] is "--inspect" or "--inspect-mods")
{
    string game=args[1],output=args[2];Directory.CreateDirectory(output);
    var keys=GameKeys.Load(Path.Combine(game,"GTA5_Enhanced.exe"));
    string archivePath=args[0]=="--inspect-mods"?Path.Combine(game,"mods","update","update.rpf"):Path.Combine(game,"update","update.rpf");
    using var arc=new RpfArchive(File.OpenRead(archivePath),"update.rpf",keys);
    foreach(var e in arc.Files.Where(e=>e.Name.Contains("fontmap",StringComparison.OrdinalIgnoreCase)||e.Name.Contains("scaleformpreallocation",StringComparison.OrdinalIgnoreCase)))
    {File.WriteAllBytes(Path.Combine(output,e.Name),arc.Extract(e));Console.WriteLine(e.Path);}
    var platformEntry=arc.Find("scaleform_platform_pc.rpf");
    Console.WriteLine(JsonSerializer.Serialize(platformEntry));
    using var platform=new RpfArchive(new MemoryStream(arc.Extract(platformEntry)),platformEntry.Name,keys);
    Console.WriteLine("Platform encryption "+platform.Encryption.ToString("X8"));
    foreach(var e in platform.Files.Where(e=>e.Name.EndsWith(".gfx")))Console.WriteLine(JsonSerializer.Serialize(e));
    File.WriteAllText(Path.Combine(output,"outer-entries.json"),JsonSerializer.Serialize(arc.Files,new JsonSerializerOptions{WriteIndented=true}));
    return;
}
if(args.Length==2&&args[0]=="--ng-roundtrip")
{
    string game=args[1];var keys=GameKeys.Load(Path.Combine(game,"GTA5_Enhanced.exe"));
    string path=Path.Combine(game,"update","update.rpf");
    using var file=File.OpenRead(path);using var reader=new BinaryReader(file);
    file.Position=4;int entries=(int)reader.ReadUInt32(),nameLength=(int)reader.ReadUInt32();file.Position=16;
    byte[] toc=reader.ReadBytes(entries*16),strings=reader.ReadBytes(nameLength);
    keys.VerifyNgRoundTrip(toc,"update.rpf",file.Length);keys.VerifyNgRoundTrip(strings,"update.rpf",file.Length);
    foreach(int n in new[]{0,1,15,16,17,31,32,257,4097})
    {byte[] sample=new byte[n];new Random(n).NextBytes(sample);keys.VerifyNgRoundTrip(sample,"update.rpf",file.Length+12345);}
    Console.WriteLine("PASS: original encrypted TOC and names reproduce byte-for-byte; partial-block tails and changed archive-size seeds verified.");return;
}
if(args.Length==3&&args[0]=="--prepare")
{
    new PatchService(Console.WriteLine).Prepare(args[1],args[2]);return;
}
if(args.Length==2&&args[0]=="--transactions")
{
    string root=Path.GetFullPath(args[1]);Directory.CreateDirectory(Path.Combine(root,"update"));
    string stock=Path.Combine(root,"update","update.rpf"),target=Path.Combine(root,"mods","update","update.rpf"),cache=Path.Combine(root,"rpf.cache"),stage=Path.Combine(root,"stage.rpf");
    File.WriteAllText(Path.Combine(root,"GTA5_Enhanced.exe"),"transaction test fixture");
    File.WriteAllText(stock,"stock");File.WriteAllText(cache,"cache");File.WriteAllText(stage,"patched");
    var service=new PatchService(Console.WriteLine);
    bool rejected=false;try{ModEnvironment.Validate(root);}catch(FileNotFoundException){rejected=true;}
    if(!rejected)throw new Exception("Missing loader accepted");
    byte[] pe=new byte[256];pe[0]=77;pe[1]=90;pe[60]=128;pe[128]=80;pe[129]=69;pe[132]=100;pe[133]=134;pe[200]=65;pe[201]=83;pe[202]=73;
    File.WriteAllBytes(Path.Combine(root,"OpenRPF.asi"),pe);File.WriteAllBytes(Path.Combine(root,"xinput1_4.dll"),pe);
    rejected=false;try{ModEnvironment.Validate(root);}catch(InvalidOperationException){rejected=true;}
    if(!rejected)throw new Exception("Missing story-mode arguments accepted");
    File.WriteAllText(Path.Combine(root,"args.txt"),"-nobattleye");ModEnvironment.Validate(root);
    var unsafePlan=new PatchPlan(root,"update/update.rpf",PatchService.HashFile(stock),PatchService.HashFile(stage),stage,[]);
    rejected=false;try{service.Install(unsafePlan);}catch(InvalidOperationException){rejected=true;}
    if(!rejected)throw new Exception("Direct stock write accepted");
    var fresh=new PatchPlan(root,"mods/update/update.rpf",PatchService.HashFile(stock),PatchService.HashFile(stage),stage,[],TargetExisted:false);
    service.Install(fresh);CheckStock();
    if(File.ReadAllText(target)!="patched")throw new Exception("New mods install failed");
    service.Restore(root);CheckStock();if(File.Exists(target))throw new Exception("New mods restore failed");
    File.WriteAllText(target,"original mod");
    var plan=new PatchPlan(root,"mods/update/update.rpf",PatchService.HashFile(target),PatchService.HashFile(stage),stage,[]);
    service.Install(plan);CheckStock();
    File.WriteAllText(target,"game update");
    try{service.Restore(root);throw new Exception("Restore accepted modified file");}catch(IOException){}
    if(File.ReadAllText(target)!="game update")throw new Exception("Restore damaged modified file");
    File.WriteAllText(target,"patched");service.Restore(root);CheckStock();
    if(File.ReadAllText(target)!="original mod")throw new Exception("Existing mods restore failed");
    File.WriteAllText(target,"changed before install");
    try{service.Install(plan);throw new Exception("Install accepted modified target");}catch(IOException){}
    if(File.ReadAllText(target)!="changed before install")throw new Exception("Install damaged modified file");
    CheckStock();Console.WriteLine("PASS: new/existing mods install and restore; stock archive/cache untouched; missing loader/arguments and direct-write plans rejected; modified-file protection.");return;
    void CheckStock(){if(File.ReadAllText(stock)!="stock"||File.ReadAllText(cache)!="cache")throw new Exception("Stock archive/cache changed");}
}
if(args.Length<2)throw new ArgumentException("Usage: GTAVFix.Verify <extracted-font-directory> <output-directory>");
string[] names=["chinese","chinesesimp","japanese","korean","efigs"];
Directory.CreateDirectory(args[1]);
var movies=names.Select(n=>new GfxMovie(File.ReadAllBytes(Path.Combine(args[0],$"font_lib_{n}_pc.gfx")))).ToArray();
RuntimeValidation.ValidateSources(movies);
var coverage=GameLanguages.Validate(movies);
File.WriteAllText(Path.Combine(args[1],"language-coverage.json"),JsonSerializer.Serialize(coverage,new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine("PASS: game-provided source glyphs cover all 13 supported language checks.");
void MustReject(Action action,string label)
{
    bool rejected=false;
    try{action();}catch(Exception ex) when(ex is InvalidDataException or NotSupportedException or ArgumentException){rejected=true;}
    if(!rejected)throw new Exception("Invalid source accepted: "+label);
}
MustReject(()=>RuntimeValidation.ValidateSources(movies.Take(3).ToArray()),"missing language");
byte[] badMovie=File.ReadAllBytes(Path.Combine(args[0],"font_lib_chinese_pc.gfx"));badMovie[0]=0;
MustReject(()=>new GfxMovie(badMovie),"unknown GFX signature");
byte[] badFont=movies[0].Fonts[0].Data.ToArray();int nominal=Array.IndexOf(badFont,(byte)0,2)+3;badFont[nominal]=badFont[nominal+1]=0;
MustReject(()=>new GfxFont(badFont).ValidateAllGlyphs(),"invalid font metrics");
Console.WriteLine("PASS: missing-language, unknown-format and invalid-metrics sources rejected before installation.");
var report=new List<object>();
for(int i=0;i<names.Length;i++)
{
    var result=movies[i].Merge(movies);var check=new GfxMovie(result.Data);
    var expected=movies.SelectMany(m=>m.Fonts).SelectMany(f=>f.Glyphs).Select(g=>g.Code).Where(c=>c>=32&&c is not (>=0xd800 and <=0xdfff)).ToHashSet();
    RuntimeValidation.ValidateReplacement(names[i],File.ReadAllBytes(Path.Combine(args[0],$"font_lib_{names[i]}_pc.gfx")),result.Data,expected);
    foreach(var original in movies[i].Fonts)
    {
        var font=check.Fonts.Single(f=>f.Id==original.Id);var actual=font.Glyphs.Select(g=>g.Code).ToHashSet();
        if(!expected.IsSubsetOf(actual))throw new Exception("Language glyph union incomplete: "+font.Name);
        foreach(var g in original.Glyphs)
        {
            var now=font.Glyphs.Single(n=>n.Code==g.Code);
            if(g.Advance!=now.Advance||!original.Standalone(g).SequenceEqual(font.Standalone(now)))throw new Exception("Original glyph changed");
        }
        // Force traversal of every imported contour, including reference expansion.
        foreach(var g in font.Glyphs)font.Standalone(g);
        report.Add(new {locale=names[i],font=font.Name,before=original.Glyphs.Count,after=font.Glyphs.Count,hiragana=actual.Count(c=>c is >=0x3040 and <=0x309f),katakana=actual.Count(c=>c is >=0x30a0 and <=0x30ff),hangul=actual.Count(c=>c is >=0xac00 and <=0xd7af),preserved=true});
    }
    var again=check.Merge(movies);if(again.Added!=0)throw new Exception("Merge not idempotent");
    string dest=Path.Combine(args[1],$"font_lib_{names[i]}_pc.gfx");File.WriteAllBytes(dest,result.Data);
    Console.WriteLine($"{names[i]}: added {result.Added} glyphs; {result.Data.Length:N0} bytes; SHA256 {Convert.ToHexString(SHA256.HashData(result.Data))}");
}
File.WriteAllText(Path.Combine(args[1],"verification.json"),JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine("PASS: source glyph geometry/advance preserved, locale union present, imported contours readable, repeated patch adds zero glyphs.");
