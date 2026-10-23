using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace GTAVFix.Core;

public sealed record AllocationResult(string Font,int OriginalKiB,int PatchedKiB);

public static class FontAllocation
{
    // Extend the game's existing 16 KiB allocation buckets for expanded font movies.
    public static (byte[] Data,AllocationResult[] Changes) Expand(byte[] xml,IReadOnlyDictionary<string,(int Before,int After)> sizes)
    {
        using var reader=XmlReader.Create(new MemoryStream(xml),new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null});
        var document=XDocument.Load(reader,LoadOptions.PreserveWhitespace);var changes=new List<AllocationResult>();
        foreach(var pair in sizes)
        {
            if(pair.Value.After<=pair.Value.Before)continue;
            string movieName=Path.GetFileNameWithoutExtension(pair.Key);
            var movie=document.Root?.Elements("movie").SingleOrDefault(e=>string.Equals((string?)e.Attribute("name"),movieName,StringComparison.OrdinalIgnoreCase))
                ??throw new InvalidDataException(Text.Get("InvalidData","ALLOC_01")+movieName);
            int peak=(int?)movie.Attribute("peakSize")??throw new InvalidDataException(Text.Get("InvalidData","ALLOC_02"));
            int extra=checked(((pair.Value.After-pair.Value.Before+1023)/1024)*2+1024);
            int blocks=checked((extra+15)/16),budget=checked(peak+blocks*16);
            var allocator=movie.Elements("SFAlloc").SingleOrDefault(e=>(int?)e.Element("Size")?.Attribute("value")==16)??throw new InvalidDataException(Text.Get("InvalidData","ALLOC_03"));
            var bucket=allocator.Elements("Size").SingleOrDefault(e=>(int?)e.Attribute("value")==16);
            if(bucket==null)throw new InvalidDataException(Text.Get("InvalidData","ALLOC_04"));
            var count=bucket.ElementsAfterSelf().FirstOrDefault();
            if(count?.Name!="Count")throw new InvalidDataException(Text.Get("InvalidData","ALLOC_05"));
            int original=(int?)count.Attribute("value")??throw new InvalidDataException(Text.Get("InvalidData","ALLOC_06"));
            count.SetAttributeValue("value",checked(original+blocks));movie.SetAttributeValue("peakSize",budget);
            changes.Add(new AllocationResult(movieName,peak,budget));
        }
        using var output=new MemoryStream();
        using(var writer=XmlWriter.Create(output,new XmlWriterSettings{Encoding=new UTF8Encoding(false),OmitXmlDeclaration=document.Declaration==null,Indent=false,NewLineHandling=NewLineHandling.None}))document.Save(writer);
        byte[] result=output.ToArray();
        using var check=XmlReader.Create(new MemoryStream(result),new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null});XDocument.Load(check);
        return(result,changes.ToArray());
    }
}
