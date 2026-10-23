using System.Buffers.Binary;
using System.Text;

namespace GTAVFix.Core;

public sealed class GfxFont
{
    public sealed record Glyph(ushort Code, short Advance, uint Offset);
    public byte[] Data { get; }
    public ushort Id { get; }
    public string Name { get; }
    public ushort NominalSize { get; }
    public IReadOnlyList<Glyph> Glyphs { get; }
    private readonly int countOffset, blobStart, blobLength, infoStart;
    public GfxFont(byte[] data)
    {
        Data=data;Id=RpfArchive.U16(data,0);
        int end=Array.IndexOf(data,(byte)0,2);if(end<0) throw new InvalidDataException(Text.Get("InvalidData","FONT_01"));
        Name=Encoding.UTF8.GetString(data,2,end-2);
        NominalSize=RpfArchive.U16(data,end+3);countOffset=end+11;
        int count=checked((int)RpfArchive.U32(data,countOffset));
        blobLength=checked((int)RpfArchive.U32(data,countOffset+4));blobStart=countOffset+8;infoStart=checked(blobStart+blobLength);
        if(count>65535||infoStart<0||infoStart+(long)count*8>=data.Length) throw new InvalidDataException(Text.Get("InvalidData","FONT_02"));
        var glyphs=new List<Glyph>();
        for(int i=0;i<count;i++) {int p=infoStart+i*8;var g=new Glyph(RpfArchive.U16(data,p),unchecked((short)RpfArchive.U16(data,p+2)),RpfArchive.U32(data,p+4));if(g.Offset+2<blobStart||g.Offset+2>=infoStart) throw new InvalidDataException(Text.Get("InvalidData","FONT_03"));glyphs.Add(g);}
        if(glyphs.Select(g=>g.Code).Distinct().Count()!=count) throw new InvalidDataException(Text.Get("InvalidData","FONT_04"));
        Glyphs=glyphs;
    }
    private static int Read15(byte[] b,ref int p){int t=b[p++];return (t&1)==0?t>>1:(t>>1)|(b[p++]<<7);}
    private static int Read30(byte[] b,ref int p)
    {int t=b[p++],n=t&3,v=t>>2;for(int i=0;i<n;i++)v|=b[p++]<<(6+i*8);return v;}
    private static void Write30(Stream s,int value)
    {int n=value<=63?0:value<=16383?1:value<=4194303?2:3;s.WriteByte((byte)((value<<2)|n));for(int i=0;i<n;i++)s.WriteByte((byte)(value>>(6+i*8)));}
    // Expand contour references so an imported glyph has no offsets into its donor font.
    public byte[] Standalone(Glyph g)
    {
        int p=checked((int)g.Offset+2),begin=p;
        for(int i=0;i<4;i++)Read15(Data,ref p);
        int contours=Read15(Data,ref p);
        using var output=new MemoryStream();output.Write(Data.AsSpan(begin,p-begin));
        ReadOnlySpan<int> sizes=[2,3,2,3,2,3,4,5,3,4,5,6,7,8,9,10];
        for(int i=0;i<contours;i++)
        {
            begin=p;Read15(Data,ref p);Read15(Data,ref p);output.Write(Data.AsSpan(begin,p-begin));
            int count=Read30(Data,ref p),edgePos=p;
            if((count&1)!=0) {edgePos=checked(2+(count>>1));count=Read30(Data,ref edgePos);if((count&1)!=0)throw new InvalidDataException(Text.Get("InvalidData","FONT_05"));}
            int edges=count>>1,end=edgePos;
            for(int e=0;e<edges;e++) {if(end>=infoStart) throw new InvalidDataException(Text.Get("InvalidData","FONT_06"));end+=sizes[Data[end]&15];}
            if(end>infoStart)throw new InvalidDataException(Text.Get("InvalidData","FONT_07"));
            Write30(output,edges<<1);output.Write(Data.AsSpan(edgePos,end-edgePos));
            if((RereadReference(Data,begin)&1)==0) p=end;
        }
        return output.ToArray();
    }
    private static int RereadReference(byte[] b,int p){Read15(b,ref p);Read15(b,ref p);return Read30(b,ref p);}

    public void ValidateAllGlyphs()
    {
        if(NominalSize==0||Glyphs.Count==0)throw new InvalidDataException(Text.Get("InvalidData",$"FONT_08:{Name}"));
        int previous=-1;
        foreach(var glyph in Glyphs)
        {
            if(glyph.Code<=previous)throw new InvalidDataException(Text.Get("InvalidData",$"FONT_09:{Name}"));
            previous=glyph.Code;Standalone(glyph);
        }
    }

    public void ValidatePreserved(GfxFont original)
    {
        if(!Data.AsSpan(0,countOffset).SequenceEqual(original.Data.AsSpan(0,original.countOffset)))throw new InvalidDataException(Text.Get("InvalidData",$"FONT_10:{Name}"));
        if(!Data.AsSpan(infoStart+Glyphs.Count*8).SequenceEqual(original.Data.AsSpan(original.infoStart+original.Glyphs.Count*8)))throw new InvalidDataException(Text.Get("InvalidData",$"FONT_11:{Name}"));
        var index=Glyphs.ToDictionary(g=>g.Code);
        foreach(var old in original.Glyphs)
        {
            if(!index.TryGetValue(old.Code,out var now)||now.Advance!=old.Advance||!Standalone(now).SequenceEqual(original.Standalone(old)))throw new InvalidDataException(Text.Get("InvalidData",$"FONT_12:{Name}:{old.Code:X4}"));
        }
    }

    public (GfxFont Font,int[] IndexMap,int Added) Merge(IEnumerable<GfxFont> donors)
    {
        var all=Glyphs.ToDictionary(g=>g.Code);using var blob=new MemoryStream();blob.Write(Data.AsSpan(blobStart,blobLength));
        int added=0;
        foreach(var donor in donors)
        {
            if(donor.NominalSize!=NominalSize) throw new InvalidDataException(Text.Get("InvalidData",$"FONT_13:{Name}:{donor.Name}"));
            foreach(var glyph in donor.Glyphs)
            {
                // Surrogate code units and control characters are not standalone visible glyphs.
                if(glyph.Code<32||glyph.Code is >=0xd800 and <=0xdfff||all.ContainsKey(glyph.Code))continue;
                uint offset=checked((uint)(blobStart-2+blob.Length));byte[] shape=donor.Standalone(glyph);
                blob.Write(shape);all.Add(glyph.Code,new Glyph(glyph.Code,glyph.Advance,offset));added++;
            }
        }
        if(added==0)return(this,Enumerable.Range(0,Glyphs.Count).ToArray(),0);
        if(all.Count>65535)throw new InvalidDataException(Text.Get("InvalidData","FONT_14"));
        var sorted=all.Values.OrderBy(g=>g.Code).ToArray();var byCode=sorted.Select((g,i)=>(g.Code,i)).ToDictionary(x=>x.Code,x=>x.i);
        using var output=new MemoryStream();output.Write(Data.AsSpan(0,countOffset));
        using var w=new BinaryWriter(output,Encoding.UTF8,true);w.Write(sorted.Length);w.Write(checked((uint)blob.Length));blob.Position=0;blob.CopyTo(output);
        foreach(var g in sorted){w.Write(g.Code);w.Write(g.Advance);w.Write(g.Offset);}
        output.Write(Data.AsSpan(infoStart+Glyphs.Count*8));
        var merged=new GfxFont(output.ToArray());
        var map=Glyphs.Select(g=>byCode[g.Code]).ToArray();
        // Existing raw glyph data, metrics and kerning stay byte-for-byte unchanged.
        for(int i=0;i<Glyphs.Count;i++)
        {
            var old=Glyphs[i];var now=merged.Glyphs[map[i]];
            if(now.Advance!=old.Advance||now.Offset!=old.Offset)throw new InvalidDataException(Text.Get("InvalidData","FONT_15"));
        }
        if(!Data.AsSpan(blobStart,blobLength).SequenceEqual(merged.Data.AsSpan(blobStart,blobLength)))throw new InvalidDataException(Text.Get("InvalidData","FONT_16"));
        return(merged,map,added);
    }
}
