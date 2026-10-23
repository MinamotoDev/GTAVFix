using System.Security.Cryptography;

namespace GTAVFix.Core;

public sealed record FaceValidation(string Name,int Before,int After,int Hiragana,int Katakana,int Hangul,bool OriginalGlyphsPreserved);
public sealed record MovieValidation(string File,string OriginalSha256,string PatchedSha256,FaceValidation[] Faces);

public static class RuntimeValidation
{
    private static bool Visible(ushort c)=>c>=32&&c is not (>=0xd800 and <=0xdfff);
    public static HashSet<ushort> ValidateSources(IReadOnlyList<GfxMovie> movies)
    {
        foreach(var movie in movies)foreach(var font in movie.Fonts)font.ValidateAllGlyphs();
        GameLanguages.Validate(movies);
        return movies.SelectMany(m=>m.Fonts).SelectMany(f=>f.Glyphs).Select(g=>g.Code).Where(Visible).ToHashSet();
    }
    public static MovieValidation ValidateReplacement(string file,byte[] before,byte[] after,HashSet<ushort> expected)
    {
        var original=new GfxMovie(before);var patched=new GfxMovie(after);
        if(original.Fonts.Count!=patched.Fonts.Count)throw new InvalidDataException(Text.Get("InvalidData",file+":FONT_COUNT"));
        var faces=new List<FaceValidation>();
        foreach(var old in original.Fonts)
        {
            var font=patched.Fonts.Single(f=>f.Id==old.Id);font.ValidateAllGlyphs();font.ValidatePreserved(old);
            var codes=font.Glyphs.Select(g=>g.Code).ToHashSet();
            if(!expected.IsSubsetOf(codes))throw new InvalidDataException(Text.Get("InvalidData",$"{file}/{font.Name}:LANGUAGE_COVERAGE"));
            faces.Add(new FaceValidation(font.Name,old.Glyphs.Count,font.Glyphs.Count,codes.Count(c=>c is >=0x3040 and <=0x309f),codes.Count(c=>c is >=0x30a0 and <=0x30ff),codes.Count(c=>c is >=0xac00 and <=0xd7af),true));
        }
        return new MovieValidation(file,Convert.ToHexString(SHA256.HashData(before)),Convert.ToHexString(SHA256.HashData(after)),faces.ToArray());
    }
}
