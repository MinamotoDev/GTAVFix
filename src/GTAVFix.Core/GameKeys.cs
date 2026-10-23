using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace GTAVFix.Core;

// RPF key derivation and decryption follow CodeWalker (MIT).
public sealed class GameKeys
{
    private readonly byte[] aes;
    private readonly uint[][] subkeys;
    private readonly uint[] tables;
    private NgWordInverse[][]? inverse;
    private GameKeys(byte[] aes, byte[] magic)
    {
        this.aes = aes;
        var random = new Random(unchecked((int)Hash(aes)));
        var work = magic.ToArray();
        var noise = new byte[magic.Length];
        for (int i = 0; i < 4; i++)
        {
            random.NextBytes(noise);
            for (int j = 0; j < work.Length; j++) work[j] = unchecked((byte)(work[j] - noise[j]));
        }
        work = AesDecrypt(work);
        using var deflate = new DeflateStream(new MemoryStream(work), CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflate.CopyTo(output);
        byte[] plain = output.ToArray();
        if (plain.Length < 306272) throw new InvalidDataException(Text.Get("InvalidData","NG_01"));
        subkeys = Enumerable.Range(0, 101).Select(k => Enumerable.Range(0, 68)
            .Select(i => BinaryPrimitives.ReadUInt32LittleEndian(plain.AsSpan(k * 272 + i * 4))).ToArray()).ToArray();
        tables = Enumerable.Range(0, 17 * 16 * 256)
            .Select(i => BinaryPrimitives.ReadUInt32LittleEndian(plain.AsSpan(27472 + i * 4))).ToArray();
    }

    public static GameKeys Load(string exe, CancellationToken cancel = default)
    {
        byte[] data = File.ReadAllBytes(exe);
        ReadOnlySpan<byte> expected = [0xa0,0x79,0x61,0x28,0xa7,0x75,0x72,0x0a,0xc2,0x04,0xd9,0x81,0x9f,0x68,0xc1,0x72,0xe3,0x95,0x2c,0x6d];
        Span<byte> hash = stackalloc byte[20];
        for (int i = 0; i <= data.Length - 32; i++)
        {
            if ((i & 0xffff) == 0) cancel.ThrowIfCancellationRequested();
            SHA1.HashData(data.AsSpan(i, 32), hash);
            if (!hash.SequenceEqual(expected)) continue;
            using var stream = typeof(GameKeys).Assembly.GetManifestResourceStream("GTAVFix.Core.Resources.magic.dat")!;
            using var output = new MemoryStream(); stream.CopyTo(output);
            return new GameKeys(data.AsSpan(i, 32).ToArray(), output.ToArray());
        }
        throw new InvalidDataException(Text.Get("InvalidData","NG_02"));
    }

    internal byte[] AesDecrypt(byte[] input)
    {
        using var cipher = Aes.Create(); cipher.Key = aes;
        byte[] result = input.ToArray(); int n = input.Length & ~15;
        cipher.DecryptEcb(input.AsSpan(0, n), result.AsSpan(0, n), PaddingMode.None);
        return result;
    }

    internal byte[] AesEncrypt(byte[] input)
    {
        using var cipher=Aes.Create();cipher.Key=aes;
        byte[] result=input.ToArray();int n=input.Length&~15;
        cipher.EncryptEcb(input.AsSpan(0,n),result.AsSpan(0,n),PaddingMode.None);return result;
    }

    internal byte[] NgEncrypt(byte[] input,string name,long length)
    {
        inverse??=Enumerable.Range(0,17).Select(round=>Enumerable.Range(0,4).Select(word=>
        {
            int[][] groups=[[0,7,10,13],[1,4,11,14],[2,5,8,15],[3,6,9,12]];
            int[] positions=round<2||round==16?Enumerable.Range(word*4,4).ToArray():groups[word];
            return new NgWordInverse(positions.Select(p=>tables.AsSpan((round*16+p)*256,256).ToArray()).ToArray(),positions);
        }).ToArray()).ToArray();
        uint seed=Hash(Encoding.UTF8.GetBytes(name.ToLowerInvariant()));
        uint[] key=subkeys[(int)(unchecked(seed+checked((uint)length)+61u)%101)];
        byte[] result=input.ToArray();Span<byte> current=stackalloc byte[16];Span<byte> next=stackalloc byte[16];
        for(int off=0;off+16<=input.Length;off+=16)
        {
            input.AsSpan(off,16).CopyTo(current);
            for(int round=16;round>=0;round--)
            {
                for(int word=0;word<4;word++)inverse[round][word].Apply(BinaryPrimitives.ReadUInt32LittleEndian(current.Slice(word*4,4))^key[round*4+word],next);
                next.CopyTo(current);
            }
            current.CopyTo(result.AsSpan(off,16));
        }
        if(!NgDecrypt(result,name,length).AsSpan().SequenceEqual(input))throw new InvalidDataException(Text.Get("InvalidData","NG_03"));
        return result;
    }

    public void VerifyNgRoundTrip(byte[] ciphertext,string name,long length)
    {
        byte[] plain=NgDecrypt(ciphertext,name,length);
        if(!NgEncrypt(plain,name,length).AsSpan().SequenceEqual(ciphertext))throw new InvalidDataException(Text.Get("InvalidData","NG_04"));
    }

    // Each white-box lookup is a permuted 8-bit subspace of a 32-bit word.
    // Construct and exhaustively validate its inverse, rather than distributing encryption tables.
    private sealed class NgWordInverse
    {
        private readonly uint[] basis=new uint[32],coordinates=new uint[32];
        private readonly byte[][] decode=Enumerable.Range(0,4).Select(_=>new byte[256]).ToArray();
        private readonly uint[][] transform=Enumerable.Range(0,4).Select(_=>new uint[256]).ToArray();
        private readonly uint constant;
        private readonly int[] positions;
        public NgWordInverse(uint[][] t,int[] positions)
        {
            this.positions=positions;constant=t[0][0]^t[1][0]^t[2][0]^t[3][0];
            for(int j=0;j<4;j++)
            {
                int selected=0;
                for(int x=1;x<256&&selected<8;x++)
                    if(Insert(t[j][x]^t[j][0],1u<<(j*8+selected)))selected++;
                if(selected!=8)throw new NotSupportedException(Text.Get("InvalidData","NG_05"));
            }
            for(int j=0;j<4;j++)
            {
                var used=new bool[256];uint mask=255u<<(j*8);
                for(int x=0;x<256;x++)
                {
                    uint code=Reduce(t[j][x]^t[j][0]);
                    if((code&~mask)!=0)throw new NotSupportedException(Text.Get("InvalidData","NG_06"));
                    int b=(int)((code>>(j*8))&255);
                    if(used[b])throw new InvalidDataException(Text.Get("InvalidData","NG_07"));
                    used[b]=true;decode[j][b]=(byte)x;
                }
                for(int x=0;x<256;x++)transform[j][x]=Reduce((uint)x<<(j*8));
            }
        }
        private bool Insert(uint value,uint code)
        {
            for(int bit=31;bit>=0;bit--)if((value&(1u<<bit))!=0)
            {
                if(basis[bit]==0){basis[bit]=value;coordinates[bit]=code;return true;}
                value^=basis[bit];code^=coordinates[bit];
            }
            return false;
        }
        private uint Reduce(uint value)
        {
            uint code=0;
            for(int bit=31;bit>=0;bit--)if((value&(1u<<bit))!=0)
            {
                if(basis[bit]==0)throw new NotSupportedException(Text.Get("InvalidData","NG_08"));
                value^=basis[bit];code^=coordinates[bit];
            }
            return code;
        }
        public void Apply(uint word,Span<byte> output)
        {
            word^=constant;
            uint code=transform[0][word&255]^transform[1][(word>>8)&255]^transform[2][(word>>16)&255]^transform[3][word>>24];
            for(int j=0;j<4;j++)output[positions[j]]=decode[j][(code>>(j*8))&255];
        }
    }

    internal byte[] NgDecrypt(byte[] input, string name, long length)
    {
        uint seed = Hash(Encoding.UTF8.GetBytes(name.ToLowerInvariant()));
        int index = (int)(unchecked(seed + checked((uint)length) + 61u) % 101);
        uint[] key = subkeys[index]; byte[] result = input.ToArray();
        Span<byte> current = stackalloc byte[16]; Span<byte> next = stackalloc byte[16];
        ReadOnlySpan<int> a = [0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15];
        ReadOnlySpan<int> b = [0,7,10,13,1,4,11,14,2,5,8,15,3,6,9,12];
        for (int off = 0; off + 16 <= input.Length; off += 16)
        {
            input.AsSpan(off,16).CopyTo(current);
            for (int round = 0; round < 17; round++)
            {
                var map = round < 2 || round == 16 ? a : b;
                for (int col = 0; col < 4; col++)
                {
                    uint value = key[round * 4 + col];
                    for (int n = 0; n < 4; n++)
                    {
                        int t = map[col * 4 + n];
                        value ^= tables[(round * 16 + t) * 256 + current[t]];
                    }
                    BinaryPrimitives.WriteUInt32LittleEndian(next.Slice(col * 4,4),value);
                }
                next.CopyTo(current);
            }
            current.CopyTo(result.AsSpan(off,16));
        }
        return result;
    }

    private static uint Hash(ReadOnlySpan<byte> bytes)
    {
        uint h=0;
        unchecked { foreach (byte b in bytes) { h+=b; h+=h<<10; h^=h>>6; } h+=h<<3; h^=h>>11; h+=h<<15; }
        return h;
    }
}
