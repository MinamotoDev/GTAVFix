using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace GTAVFix.Core;

public sealed class RpfArchive : IDisposable
{
    public sealed record Entry(int Index, string Name, string Path, long Offset, int StoredSize, int Size, bool Resource, bool Encrypted);
    private readonly Stream stream;
    private readonly GameKeys keys;
    private readonly byte[] entries, names;
    private readonly uint encryption;
    private readonly string archiveName;
    public uint Encryption=>encryption;
    public IReadOnlyList<Entry> Files { get; }
    public RpfArchive(Stream stream, string name, GameKeys keys)
    {
        this.stream=stream; this.keys=keys;archiveName=name;
        using var r = new BinaryReader(stream, Encoding.UTF8, true);
        stream.Position=0;
        if (r.ReadUInt32()!=0x52504637) throw new InvalidDataException(Text.Get("InvalidData","RPF_01"));
        int count=checked((int)r.ReadUInt32()), namesLength=checked((int)r.ReadUInt32()); encryption=r.ReadUInt32();
        if (count<1 || count>1000000 || namesLength<1 || namesLength>16000000) throw new InvalidDataException(Text.Get("InvalidData","RPF_02"));
        entries=ReadExactly(stream,checked(count*16)); names=ReadExactly(stream,namesLength);
        if (encryption==0x0ffffff9) { entries=keys.AesDecrypt(entries); names=keys.AesDecrypt(names); }
        else if (encryption is not (0x4e45504f or 0x0fffffff)) { entries=keys.NgDecrypt(entries,name,stream.Length); names=keys.NgDecrypt(names,name,stream.Length); }
        var list=new List<Entry>(); var seen=new HashSet<int>();
        void Visit(int index,string parent)
        {
            if (index<0||index>=count||!seen.Add(index)) throw new InvalidDataException(Text.Get("InvalidData","RPF_03"));
            int p=index*16; bool directory=U32(entries,p+4)==0x7fffff00;
            int no=directory?checked((int)U32(entries,p)):U16(entries,p);
            if (no>=names.Length) throw new InvalidDataException(Text.Get("InvalidData","RPF_04"));
            int end=Array.IndexOf(names,(byte)0,no); if(end<0) throw new InvalidDataException(Text.Get("InvalidData","RPF_05"));
            string entryName=Encoding.UTF8.GetString(names,no,end-no);
            string path=index==0?"":string.IsNullOrEmpty(parent)?entryName:parent+"/"+entryName;
            if(directory) { int first=checked((int)U32(entries,p+8)),n=checked((int)U32(entries,p+12)); for(int i=0;i<n;i++) Visit(first+i,path); }
            else
            {
                bool resource=(entries[p+7]&128)!=0; int stored=U24(entries,p+2);
                long off=(U24(entries,p+5)&0x7fffff)*512L;
                int size=resource?stored:checked((int)U32(entries,p+8));
                if(off<16||off+ (stored>0?stored:size)>stream.Length) throw new InvalidDataException(Text.Get("InvalidData","RPF_06"));
                list.Add(new Entry(index,entryName,path,off,stored,size,resource,!resource&&U32(entries,p+12)==1));
            }
        }
        Visit(0,""); Files=list;
    }
    public byte[] Extract(Entry e)
    {
        if(e.Resource) throw new NotSupportedException(Text.Get("InvalidData","RPF_07"));
        stream.Position=e.Offset; byte[] data=ReadExactly(stream,e.StoredSize>0?e.StoredSize:e.Size);
        if(e.Encrypted) data=encryption==0x0ffffff9?keys.AesDecrypt(data):keys.NgDecrypt(data,e.Name,e.Size);
        if(e.StoredSize>0) { using var z=new DeflateStream(new MemoryStream(data),CompressionMode.Decompress);using var ms=new MemoryStream();z.CopyTo(ms);data=ms.ToArray(); }
        if(data.Length!=e.Size) throw new InvalidDataException(Text.Get("InvalidData",$"RPF_08:{e.Name}"));
        return data;
    }
    public Entry Find(string suffix)=>Files.Single(e=>e.Path.EndsWith(suffix,StringComparison.OrdinalIgnoreCase));
    public byte[] RewriteSmall(IReadOnlyDictionary<string,byte[]> replacements)
    {
        using var output=new MemoryStream(); stream.Position=0;stream.CopyTo(output);
        Rewrite(output,replacements);return output.ToArray();
    }
    // Append payloads and retain the original TOC encryption mode.
    public void Rewrite(Stream output,IReadOnlyDictionary<string,byte[]> replacements)
    {
        byte[] toc=entries.ToArray();
        foreach(var item in replacements)
        {
            var e=Files.Single(e=>e.Path.Equals(item.Key,StringComparison.OrdinalIgnoreCase));
            if(e.Resource) throw new NotSupportedException(Text.Get("InvalidData","RPF_09"));
            bool nested=e.Name.EndsWith(".rpf",StringComparison.OrdinalIgnoreCase);
            byte[] payload=item.Value;
            if(!nested) {using var m=new MemoryStream();using(var z=new DeflateStream(m,CompressionLevel.SmallestSize,true)) z.Write(payload);payload=m.ToArray();}
            bool uncompressed=nested||payload.Length>0xffffff;
            if(uncompressed)payload=item.Value; // RPF binary entries encode raw length in the 32-bit field.
            if(e.Encrypted)payload=encryption==0x0ffffff9?keys.AesEncrypt(payload):keys.NgEncrypt(payload,e.Name,item.Value.Length);
            long offset=(output.Length+511)&~511L;
            if(offset/512>0x7fffff) throw new InvalidDataException(Text.Get("InvalidData","RPF_10"));
            output.Position=offset;output.Write(payload);
            int p=e.Index*16;Write24(toc,p+2,uncompressed?0:payload.Length);Write24(toc,p+5,checked((int)(offset/512)));
            BinaryPrimitives.WriteUInt32LittleEndian(toc.AsSpan(p+8),checked((uint)item.Value.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(toc.AsSpan(p+12),e.Encrypted?1u:0u);
        }
        output.SetLength((output.Length+511)&~511L);
        byte[] encodedNames=names;
        if(encryption==0x0ffffff9){toc=keys.AesEncrypt(toc);encodedNames=keys.AesEncrypt(names);}
        else if(encryption is not (0x4e45504f or 0x0fffffff))
        {toc=keys.NgEncrypt(toc,archiveName,output.Length);encodedNames=keys.NgEncrypt(names,archiveName,output.Length);}
        output.Position=12; using var w=new BinaryWriter(output,Encoding.UTF8,true);w.Write(encryption);w.Write(toc);w.Write(encodedNames);output.Flush();
    }
    internal static byte[] ReadExactly(Stream s,int n){byte[] b=new byte[n];s.ReadExactly(b);return b;}
    internal static ushort U16(byte[] b,int p)=>BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(p,2));
    internal static uint U32(byte[] b,int p)=>BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));
    private static int U24(byte[] b,int p)=>b[p]|b[p+1]<<8|b[p+2]<<16;
    private static void Write24(byte[] b,int p,int n){b[p]=(byte)n;b[p+1]=(byte)(n>>8);b[p+2]=(byte)(n>>16);}
    public void Dispose()=>stream.Dispose();
}
