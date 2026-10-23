namespace GTAVFix.Core;

public sealed record GameLanguage(string Code,int FontIndex,string RequiredCharacters);
public sealed record LanguageCoverage(string Language,int Required,int Available,int Missing);

public static class GameLanguages
{
    // Source libraries: Traditional Chinese, Simplified Chinese, Japanese, Korean, EFIGS.
    public static IReadOnlyList<GameLanguage> All {get;}=[
        new("zh-Hant",0,"繁體漢語"),new("zh-Hans",1,"简体汉语"),
        new("en",4,"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"),
        new("fr",4,"ÀÂÆÇÈÉÊËÎÏÔŒÙÛÜŸàâæçèéêëîïôœùûüÿ"),
        new("it",4,"ÀÈÉÌÒÙàèéìòù"),new("de",4,"ÄÖÜäöüß"),
        new("es",4,"ÁÉÍÓÚÑÜáéíóúñü¿¡"),
        new("ja",2,"あいうえおアイウエオ日本語"),new("ko",3,"한글한국어"),
        new("pl",4,"ĄĆĘŁŃÓŚŹŻąćęłńóśźż"),
        new("pt-BR",4,"ÀÁÂÃÇÉÊÍÓÔÕÚÜàáâãçéêíóôõúü"),
        new("ru",4,"АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя"),
        new("es-419",4,"ÁÉÍÓÚÑÜáéíóúñü¿¡")
    ];
    public static LanguageCoverage[] Validate(IReadOnlyList<GfxMovie> movies)
    {
        if(movies.Count!=5)throw new InvalidDataException(Text.Get("MissingLanguage"));
        var codes=movies.Select(m=>m.Fonts.SelectMany(f=>f.Glyphs).Select(g=>g.Code).ToHashSet()).ToArray();
        return All.Select(language=>
        {
            var required=language.RequiredCharacters.Distinct().ToArray();
            var missing=required.Where(c=>!codes[language.FontIndex].Contains(c)).ToArray();
            if(missing.Length>0)throw new InvalidDataException(Text.Get("MissingCharacter",language.Code,((int)missing[0]).ToString("X4")));
            return new LanguageCoverage(language.Code,required.Length,required.Length-missing.Length,missing.Length);
        }).ToArray();
    }
}
