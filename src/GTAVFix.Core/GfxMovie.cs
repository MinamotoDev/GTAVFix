using System.Buffers.Binary;

namespace GTAVFix.Core;

public sealed class GfxMovie
{
    private sealed record Tag(int Type,byte[] Body,bool LongHeader=false);
    private readonly byte[] header;
    private readonly List<Tag> tags;
    public IReadOnlyList<GfxFont> Fonts { get; }
    public GfxMovie(byte[] data)
    {
        if(data.Length<20||!(data.AsSpan(0,3).SequenceEqual("GFX"u8)||data.AsSpan(0,3).SequenceEqual("FWS"u8)))throw new InvalidDataException(Text.Get("InvalidData","GFX_01"));
        if(RpfArchive.U32(data,4)!=data.Length)throw new InvalidDataException(Text.Get("InvalidData","GFX_02"));
        int end=8+(5+4*(data[8]>>3)+7)/8+4;header=data[..end];tags=ParseTags(data,end);
        Fonts=tags.Where(t=>t.Type==1005).Select(t=>new GfxFont(t.Body)).ToArray();
        if(Fonts.Count==0)throw new InvalidDataException(Text.Get("InvalidData","GFX_03"));
        if(tags.Any(t=>t.Type==1002))throw new NotSupportedException(Text.Get("InvalidData","GFX_04"));
    }
    private static List<Tag> ParseTags(byte[] d,int pos)
    {
        var list=new List<Tag>();
        while(pos<d.Length)
        {
            ushort h=RpfArchive.U16(d,pos);pos+=2;int n=h&63;bool longHeader=n==63;
            if(n==63){n=checked((int)RpfArchive.U32(d,pos));pos+=4;}
            if(n<0||pos+(long)n>d.Length)throw new InvalidDataException(Text.Get("InvalidData","GFX_05"));
            list.Add(new Tag(h>>6,d.AsSpan(pos,n).ToArray(),longHeader));pos+=n;
            if(h>>6==0){if(pos!=d.Length)throw new InvalidDataException(Text.Get("InvalidData","GFX_06"));break;}
        }
        return list;
    }
    private static byte[] WriteTags(IEnumerable<Tag> source)
    {
        using var output=new MemoryStream();using var w=new BinaryWriter(output);
        foreach(var t in source)
        {
            bool longHeader=t.LongHeader||t.Body.Length>=63;
            w.Write((ushort)((t.Type<<6)|(longHeader?63:t.Body.Length)));
            if(longHeader)w.Write(t.Body.Length);w.Write(t.Body);
        }
        return output.ToArray();
    }
    public (byte[] Data,int Added) Merge(IReadOnlyList<GfxMovie> sources)
    {
        var maps=new Dictionary<ushort,int[]>();var changed=new Dictionary<ushort,GfxFont>();int added=0;
        // Prefer the corresponding display face; then use each locale's main text face.
        var mainNames=new HashSet<string>(["MHeiHK-Bold","SimpChinese","MotoyaExGothic","HYRKorean","Chalet-LondonNineteenSixty"],StringComparer.OrdinalIgnoreCase);
        var all=sources.SelectMany(s=>s.Fonts).ToArray();
        foreach(var f in Fonts)
        {
            var donors=all.Where(d=>d.Name==f.Name).Concat(all.Where(d=>mainNames.Contains(d.Name))).Concat(all).Distinct().ToArray();
            var result=f.Merge(donors);maps[f.Id]=result.IndexMap;changed[f.Id]=result.Font;added+=result.Added;
        }
        if(added==0)return(Serialize(tags),0);
        List<Tag> Rewrite(List<Tag> input)=>input.Select(t=>t.Type switch
        {
            1005=>new Tag(t.Type,changed[RpfArchive.U16(t.Body,0)].Data,t.LongHeader),
            11 or 33=>new Tag(t.Type,RemapText(t.Body,t.Type==33,maps),t.LongHeader),
            39=>new Tag(39,t.Body[..4].Concat(WriteTags(Rewrite(ParseTags(t.Body,4)))).ToArray(),t.LongHeader),
            _=>t
        }).ToList();
        byte[] resultData=Serialize(Rewrite(tags));var check=new GfxMovie(resultData);
        if(check.Fonts.Count!=Fonts.Count)throw new InvalidDataException(Text.Get("InvalidData","GFX_07"));
        return(resultData,added);
    }
    private byte[] Serialize(List<Tag> list)
    {byte[] result=header.Concat(WriteTags(list)).ToArray();BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4),(uint)result.Length);return result;}

    private static byte[] RemapText(byte[] data,bool alpha,Dictionary<ushort,int[]> maps)
    {
        var r=new Bits(data);r.Skip(16);int bits=r.Read(5);r.Skip(bits*4);r.Align();
        if(r.Read(1)!=0){int n=r.Read(5);r.Skip(n*2);}if(r.Read(1)!=0){int n=r.Read(5);r.Skip(n*2);}int move=r.Read(5);r.Skip(move*2);r.Align();
        int prefix=r.Position/8,oldGlyphBits=r.Read(8),advanceBits=r.Read(8);
        int max=maps.Count==0?0:maps.Values.Max(m=>m.Length==0?0:m.Max());int newGlyphBits=Math.Max(oldGlyphBits,Width(max));
        using var output=new MemoryStream();output.Write(data.AsSpan(0,prefix));output.WriteByte((byte)newGlyphBits);output.WriteByte((byte)advanceBits);
        var w=new BitWriter(output);ushort font=0;
        while(true)
        {
            int flags=r.Read(8);w.Write(flags,8);if(flags==0)break;
            if((flags&0x80)==0)throw new InvalidDataException(Text.Get("InvalidData","GFX_08"));
            if((flags&8)!=0){font=(ushort)(r.Read(8)|(r.Read(8)<<8));w.Write(font&255,8);w.Write(font>>8,8);}
            int bytes=((flags&4)!=0?(alpha?4:3):0)+((flags&1)!=0?2:0)+((flags&2)!=0?2:0)+((flags&8)!=0?2:0);
            for(int i=0;i<bytes;i++)w.Write(r.Read(8),8);
            int count=r.Read(8);w.Write(count,8);
            for(int i=0;i<count;i++)
            {
                int g=r.Read(oldGlyphBits);if(maps.TryGetValue(font,out var map)){if(g>=map.Length)throw new InvalidDataException(Text.Get("InvalidData","GFX_09"));g=map[g];}
                w.Write(g,newGlyphBits);w.Write(r.Read(advanceBits),advanceBits);
            }
            r.Align();w.Align();
        }
        r.Align();if(r.Position!=data.Length*8)throw new InvalidDataException(Text.Get("InvalidData","GFX_10"));
        w.Align();return output.ToArray();
    }
    private static int Width(int value){int n=1;while((value>>n)!=0)n++;return n;}
    private sealed class Bits(byte[] data)
    {
        public int Position {get;private set;}
        public int Read(int n){if(n<0||n>32||Position+(long)n>data.Length*8)throw new InvalidDataException(Text.Get("InvalidData","GFX_11"));uint v=0;for(int i=0;i<n;i++,Position++)v=(v<<1)|(uint)((data[Position/8]>>(7-Position%8))&1);return unchecked((int)v);}
        public void Skip(int n){if(n<0||Position+(long)n>data.Length*8)throw new InvalidDataException(Text.Get("InvalidData","GFX_12"));Position+=n;}
        public void Align()=>Position=(Position+7)&~7;
    }
    private sealed class BitWriter(Stream stream)
    {
        int value,count;
        public void Write(int v,int n){for(int i=n-1;i>=0;i--){value=(value<<1)|((v>>i)&1);if(++count==8){stream.WriteByte((byte)value);count=value=0;}}}
        public void Align(){if(count>0){stream.WriteByte((byte)(value<<(8-count)));count=value=0;}}
    }
}
