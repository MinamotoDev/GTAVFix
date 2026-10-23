using System.Buffers.Binary;
using System.Text;

namespace GTAVFix.Core;

public static class ModEnvironment
{
    public static void Validate(string game)
    {
        ValidatePlugin(Path.Combine(game,"OpenRPF.asi"),"OpenRPF.asi");
        // Enhanced uses Alexander Blade's xinput1_4 proxy; dinput8 alone targets Legacy.
        ValidatePlugin(Path.Combine(game,"xinput1_4.dll"),"ASI Loader (xinput1_4.dll)");
        byte[] loader=File.ReadAllBytes(Path.Combine(game,"xinput1_4.dll"));
        if(!Encoding.Latin1.GetString(loader).Contains("ASI",StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(Text.Get("InvalidPlugin","ASI Loader (xinput1_4.dll)"));
        string args=string.Join(" ",new[]{"args.txt","commandline.txt"}.Where(n=>File.Exists(Path.Combine(game,n))).Select(n=>File.ReadAllText(Path.Combine(game,n))));
        if(!args.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries).Any(s=>s.Equals("-nobattleye",StringComparison.OrdinalIgnoreCase)||s.Equals("-noBE",StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(Text.Get("NeedStory"));
    }
    private static void ValidatePlugin(string path,string label)
    {
        if(!File.Exists(path))throw new FileNotFoundException(Text.Get("MissingLoader",label));
        byte[] data=File.ReadAllBytes(path);
        if(data.Length<256||data[0]!='M'||data[1]!='Z')throw new InvalidDataException(Text.Get("InvalidPlugin",label));
        int pe=BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(60));
        if(pe<64||pe>data.Length-24||BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(pe))!=0x4550||BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(pe+4))!=0x8664)
            throw new InvalidDataException(Text.Get("InvalidPlugin",label));
    }
}
