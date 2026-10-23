using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace GTAVFix.Core;

public static class Text
{
    private static readonly Dictionary<string,Dictionary<string,string>> Catalog=Load();
    public static IReadOnlyCollection<string> Languages=>Catalog.Keys;
    public static string Language=>Resolve(CultureInfo.CurrentUICulture);
    public static string Resolve(CultureInfo culture)
    {
        string name=culture.Name.ToLowerInvariant(),language=culture.TwoLetterISOLanguageName;
        if(language=="zh")return name.Contains("hant")||name is "zh-tw" or "zh-hk" or "zh-mo" or "zh-cht"?"zh-Hant":"zh-Hans";
        if(language=="pt")return "pt-BR";
        if(language=="es")
        {
            string region=name.Split('-').Last();
            return new[]{"419","ar","bo","cl","co","cr","cu","do","ec","gt","hn","mx","ni","pa","pe","pr","py","sv","us","uy","ve"}.Contains(region)?"es-419":"es";
        }
        return Catalog.ContainsKey(language)?language:"en";
    }
    public static string Get(string key,params object?[] args)=>ForLanguage(Language,key,args);
    public static string ForLanguage(string language,string key,params object?[] args)
    {
        var strings=Catalog.GetValueOrDefault(language)??Catalog["en"];
        string template=strings.GetValueOrDefault(key)??Catalog["en"][key];
        return args.Length==0?template:string.Format(CultureInfo.CurrentCulture,template,args);
    }
    public static IReadOnlyDictionary<string,string> Entries(string language)=>Catalog[language];
    private static Dictionary<string,Dictionary<string,string>> Load()
    {
        var assembly=typeof(Text).Assembly;var result=new Dictionary<string,Dictionary<string,string>>();
        const string prefix="GTAVFix.Core.Resources.Strings.";
        foreach(string name in assembly.GetManifestResourceNames().Where(n=>n.StartsWith(prefix)&&n.EndsWith(".json")))
        {
            using var stream=assembly.GetManifestResourceStream(name)!;
            result.Add(name[prefix.Length..^5],JsonSerializer.Deserialize<Dictionary<string,string>>(stream)!);
        }
        return result;
    }
}
