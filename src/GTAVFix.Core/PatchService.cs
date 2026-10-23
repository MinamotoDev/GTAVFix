using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace GTAVFix.Core;

public sealed record FontResult(string File,int Added,int Bytes);
public sealed record PatchPlan(string GameDirectory,string TargetRelative,string OriginalHash,string PatchedHash,string StagedFile,FontResult[] Fonts,string? ValidationFile=null,bool TargetExisted=true);
public sealed record BackupRecord(string TargetRelative,string OriginalHash,string PatchedHash,bool HadCache,string? CacheHash,string Created);

public sealed class PatchService(Action<string>? log=null)
{
    private static readonly string[] FontFiles=["font_lib_chinese_pc.gfx","font_lib_chinesesimp_pc.gfx","font_lib_japanese_pc.gfx","font_lib_korean_pc.gfx","font_lib_efigs_pc.gfx"];
    private static readonly JsonSerializerOptions Json=new(){WriteIndented=true};
    private void Log(string message)=>log?.Invoke(message);
    public static string ValidateDirectory(string game)
    {
        game=Path.GetFullPath(game.Trim().Trim('"'));
        if(!File.Exists(Path.Combine(game,"GTA5_Enhanced.exe"))||!File.Exists(Path.Combine(game,"update","update.rpf")))throw new DirectoryNotFoundException(Text.Get("SelectGame"));
        return game;
    }
    private static string Target(string game)
    {
        ModEnvironment.Validate(game);
        return "mods/update/update.rpf";
    }
    public static void EnsureClosed()
    {
        foreach(string name in new[]{"GTA5_Enhanced","GTA5_Enhanced_BE"})
            foreach(var process in Process.GetProcessesByName(name)){process.Dispose();throw new InvalidOperationException(Text.Get("CloseGame"));}
    }
    public PatchPlan Prepare(string game,string workDirectory)
    {
        game=ValidateDirectory(game);EnsureClosed();Directory.CreateDirectory(workDirectory);
        string relative=Target(game),target=Path.Combine(game,relative);
        bool targetExisted=File.Exists(target);
        string source=targetExisted?target:Path.Combine(game,"update","update.rpf");
        Log(Text.Get("GameVersion",FileVersionInfo.GetVersionInfo(Path.Combine(game,"GTA5_Enhanced.exe")).FileVersion));
        Log(Text.Get("Target",relative));
        Log(Text.Get("ReadKeys"));var keys=GameKeys.Load(Path.Combine(game,"GTA5_Enhanced.exe"));
        string originalHash=HashFile(source);
        Log(Text.Get("ReadFonts"));
        using var vanilla=new RpfArchive(File.OpenRead(Path.Combine(game,"update","update.rpf")),"update.rpf",keys);
        var ve=vanilla.Find("scaleform_platform_pc.rpf");byte[] donorBytes=vanilla.Extract(ve);
        using var donors=new RpfArchive(new MemoryStream(donorBytes),ve.Name,keys);
        var sourceData=FontFiles.Select(name=>donors.Extract(donors.Find(name))).ToArray();
        var sourceMovies=sourceData.Select(data=>new GfxMovie(data)).ToArray();
        Log(Text.Get("CheckSources"));
        var expected=RuntimeValidation.ValidateSources(sourceMovies);
        using var current=new RpfArchive(File.OpenRead(source),"update.rpf",keys);
        if(targetExisted)
        {
            const string versionPath="x64/data/legal/version/version_num.xml";
            var stockVersion=vanilla.Files.SingleOrDefault(e=>e.Path==versionPath);
            var modVersion=current.Files.SingleOrDefault(e=>e.Path==versionPath);
            if(stockVersion==null||modVersion==null||!vanilla.Extract(stockVersion).SequenceEqual(current.Extract(modVersion)))
                throw new InvalidDataException(Text.Get("OutdatedMods"));
        }
        var platformEntry=current.Find("scaleform_platform_pc.rpf");
        using var platform=new RpfArchive(new MemoryStream(current.Extract(platformEntry)),platformEntry.Name,keys);
        var replacements=new Dictionary<string,byte[]>();var report=new List<FontResult>();var validations=new List<MovieValidation>();
        var fontSizes=new Dictionary<string,(int Before,int After)>();
        foreach(string name in FontFiles)
        {
            try
            {
                Log(Text.Get("MergeFont",name));var e=platform.Find(name);byte[] before=platform.Extract(e);var movie=new GfxMovie(before);
                foreach(var font in movie.Fonts)font.ValidateAllGlyphs();
                var result=movie.Merge(sourceMovies);
                validations.Add(RuntimeValidation.ValidateReplacement(name,before,result.Data,expected));
                if(result.Added>0)replacements[e.Path]=result.Data;
                if(result.Added>0)fontSizes[name]=(before.Length,result.Data.Length);
                report.Add(new FontResult(name,result.Added,result.Data.Length));
                Log(Text.Get("FontResult",name,movie.Fonts.Count,result.Added));
            }
            catch(Exception ex) when(ex is InvalidDataException or ArgumentException or IndexOutOfRangeException or NotSupportedException)
            {throw new InvalidDataException(Text.Get("ValidationFailed",name,ex.Message),ex);}
        }
        if(replacements.Count==0)throw new InvalidOperationException(Text.Get("AlreadyPatched"));
        Log(Text.Get("WriteFonts"));byte[] rewritten=platform.RewriteSmall(replacements);
        using(var check=new RpfArchive(new MemoryStream(rewritten),platformEntry.Name,keys))
        {
            if(check.Encryption!=platform.Encryption)throw new InvalidDataException(Text.Get("InvalidData","PATCH_01"));
            foreach(var item in replacements)
            {
                var old=platform.Find(item.Key);var now=check.Find(item.Key);
                if(now.Encrypted!=old.Encrypted)throw new InvalidDataException(Text.Get("InvalidData","PATCH_02"));
                if(!item.Value.SequenceEqual(check.Extract(now)))throw new InvalidDataException(Text.Get("InvalidData","PATCH_03"));
            }
        }
        Log(Text.Get("Allocate"));
        var allocationEntry=current.Find("common/data/scaleformpreallocation.xml");
        var allocation=FontAllocation.Expand(current.Extract(allocationEntry),fontSizes);
        var outerReplacements=new Dictionary<string,byte[]>{{platformEntry.Path,rewritten},{allocationEntry.Path,allocation.Data}};
        string staged=Path.Combine(Path.GetFullPath(workDirectory),"update.patched.rpf");
        if(File.Exists(staged))throw new IOException(Text.Get("WorkExists"));
        Log(Text.Get("WriteArchive"));
        File.Copy(source,staged);
        using(var output=new FileStream(staged,FileMode.Open,FileAccess.ReadWrite,FileShare.None))current.Rewrite(output,outerReplacements);
        using(var check=new RpfArchive(File.OpenRead(staged),"update.rpf",keys))
        {
            if(check.Encryption!=current.Encryption)throw new InvalidDataException(Text.Get("InvalidData","PATCH_04"));
            if(new FileInfo(staged).Length%512!=0)throw new InvalidDataException(Text.Get("InvalidData","PATCH_05"));
            if(check.Files.Count!=current.Files.Count)throw new InvalidDataException(Text.Get("InvalidData","PATCH_06"));
            if(!rewritten.SequenceEqual(check.Extract(check.Find(platformEntry.Path))))throw new InvalidDataException(Text.Get("InvalidData","PATCH_07"));
            if(!allocation.Data.SequenceEqual(check.Extract(check.Find(allocationEntry.Path))))throw new InvalidDataException(Text.Get("InvalidData","PATCH_08"));
            if(check.Find(allocationEntry.Path).Encrypted!=allocationEntry.Encrypted)throw new InvalidDataException(Text.Get("InvalidData","PATCH_09"));
            // Unrelated payloads keep their offsets, lengths, flags and bytes from the original copy.
            foreach(var old in current.Files.Where(e=>!outerReplacements.ContainsKey(e.Path)))
            {
                var now=check.Files.Single(e=>e.Index==old.Index);
                if(old!=now)throw new InvalidDataException(Text.Get("InvalidData","PATCH_10"));
            }
        }
        if(HashFile(source)!=originalHash||File.Exists(target)!=targetExisted)throw new IOException(Text.Get("ChangedFiles"));
        string validationFile=Path.Combine(Path.GetFullPath(workDirectory),"validation.json");
        var sourceReport=FontFiles.Select((name,index)=>new{file=name,sha256=Convert.ToHexString(SHA256.HashData(sourceData[index])),faces=sourceMovies[index].Fonts.Select(f=>new{f.Name,glyphs=f.Glyphs.Count,f.NominalSize})}).ToArray();
        File.WriteAllText(validationFile,JsonSerializer.Serialize(new{status="PASS",gameVersion=FileVersionInfo.GetVersionInfo(Path.Combine(game,"GTA5_Enhanced.exe")).FileVersion,source="Local game archives",sourceFonts=sourceReport,languageCoverage=GameLanguages.Validate(sourceMovies),expectedVisibleCodepoints=expected.Count,fonts=validations,allocations=allocation.Changes},Json));
        var plan=new PatchPlan(game,relative,originalHash,HashFile(staged),staged,report.ToArray(),validationFile,targetExisted);
        File.WriteAllText(Path.Combine(workDirectory,"plan.json"),JsonSerializer.Serialize(plan,Json));
        Log(Text.Get("Prepared"));return plan;
    }
    public string Install(PatchPlan plan)
    {
        string game=ValidateDirectory(plan.GameDirectory);EnsureClosed();ValidateRelative(plan.TargetRelative);
        if(plan.TargetRelative!="mods/update/update.rpf")throw new InvalidOperationException(Text.Get("DirectDisabled"));
        ModEnvironment.Validate(game);
        string target=Path.Combine(game,plan.TargetRelative);
        string source=plan.TargetExisted?target:Path.Combine(game,"update","update.rpf");
        if(File.Exists(target)!=plan.TargetExisted||HashFile(source)!=plan.OriginalHash||HashFile(plan.StagedFile)!=plan.PatchedHash)throw new IOException(Text.Get("ChangedFiles"));
        string root=Path.Combine(game,".gtavfix","backups");Directory.CreateDirectory(root);
        string backup=Path.Combine(root,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")[..8]);Directory.CreateDirectory(backup);
        // The stock cache belongs to the stock archives. OpenRPF bypasses it for mods.
        var record=new BackupRecord(plan.TargetRelative,plan.OriginalHash,plan.PatchedHash,false,null,DateTime.UtcNow.ToString("O"));
        File.WriteAllText(Path.Combine(backup,"backup.json"),JsonSerializer.Serialize(record,Json));
        if(plan.ValidationFile!=null)File.Copy(plan.ValidationFile,Path.Combine(backup,"validation.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        string temp=target+".gtavfix-"+Guid.NewGuid().ToString("N")+".tmp";
        bool installed=false;
        try
        {
            Log(Text.Get("CopyTemp"));File.Copy(plan.StagedFile,temp);
            if(HashFile(temp)!=plan.PatchedHash)throw new IOException(Text.Get("InvalidData","PATCH_11"));
            EnsureClosed();if(File.Exists(target)!=plan.TargetExisted||HashFile(source)!=plan.OriginalHash)throw new IOException(Text.Get("ChangedFiles"));
            Log(Text.Get("Installing"));
            if(plan.TargetExisted)File.Replace(temp,target,Path.Combine(backup,"update.original.rpf"));
            else
            {
                string baseline=Path.Combine(backup,"update.original.rpf");File.Copy(source,baseline);
                if(HashFile(baseline)!=plan.OriginalHash)throw new IOException(Text.Get("InvalidData","PATCH_12"));
                File.WriteAllText(Path.Combine(backup,"created-mods-target"),"");File.Move(temp,target);
            }
            installed=true;
            File.WriteAllText(Path.Combine(backup,"installed"),record.Created);
            Log(Text.Get("Installed",backup));return backup;
        }
        catch
        {
            if(installed&&HashFile(target)==plan.PatchedHash)
            {if(plan.TargetExisted)File.Replace(Path.Combine(backup,"update.original.rpf"),target,null);else File.Delete(target);}
            throw;
        }
        finally {if(File.Exists(temp))File.Delete(temp);}
    }
    public void Restore(string game)
    {
        game=ValidateDirectory(game);EnsureClosed();string root=Path.Combine(game,".gtavfix","backups");
        if(!Directory.Exists(root))throw new IOException(Text.Get("NoBackup"));
        string? backup=Directory.GetDirectories(root).OrderByDescending(s=>s,StringComparer.Ordinal).FirstOrDefault(d=>File.Exists(Path.Combine(d,"backup.json"))&&File.Exists(Path.Combine(d,"update.original.rpf"))&&!File.Exists(Path.Combine(d,"restored")));
        if(backup==null)throw new IOException(Text.Get("NoBackup"));
        var record=JsonSerializer.Deserialize<BackupRecord>(File.ReadAllText(Path.Combine(backup,"backup.json")))??throw new InvalidDataException(Text.Get("InvalidData","PATCH_13"));ValidateRelative(record.TargetRelative);
        string target=Path.Combine(game,record.TargetRelative),original=Path.Combine(backup,"update.original.rpf"),cache=Path.Combine(game,"rpf.cache"),cached=Path.Combine(backup,"rpf.cache");
        if(HashFile(original)!=record.OriginalHash)throw new IOException(Text.Get("InvalidData","PATCH_14"));
        string current=HashFile(target);
        if(current!=record.PatchedHash&&current!=record.OriginalHash)throw new IOException(Text.Get("RestoreChanged"));
        if(record.HadCache&&(!File.Exists(cached)||HashFile(cached)!=record.CacheHash))throw new IOException(Text.Get("InvalidData","PATCH_15"));
        string temp=target+".gtavfix-restore-"+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            File.Copy(original,temp);EnsureClosed();if(HashFile(target)!=current)throw new IOException(Text.Get("ChangedFiles"));
            if(File.Exists(Path.Combine(backup,"created-mods-target"))){File.Delete(temp);File.Delete(target);}
            else if(current==record.PatchedHash)File.Replace(temp,target,null);else File.Delete(temp);
            if(record.TargetRelative=="update/update.rpf")
            {
                if(File.Exists(cache))File.Move(cache,Path.Combine(backup,"rpf.cache.before-restore-"+Guid.NewGuid().ToString("N")));
                if(record.HadCache)File.Copy(cached,cache);
            }
            File.WriteAllText(Path.Combine(backup,"restored"),DateTime.UtcNow.ToString("O"));Log(Text.Get("Restored"));
        }
        finally{if(File.Exists(temp))File.Delete(temp);}
    }
    private static void ValidateRelative(string path){if(path!="update/update.rpf"&&path!="mods/update/update.rpf")throw new InvalidDataException(Text.Get("InvalidData","PATCH_16"));}
    public static string HashFile(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream));}
}
