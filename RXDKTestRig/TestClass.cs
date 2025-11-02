using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

public static class PdbRvaResolver
{
    public static (string file, int line)? GetFileLineFromRva(string dllPath, string pdbPath, int rva)
    {
        using var peStream = File.OpenRead(dllPath);
        using var peReader = new PEReader(peStream);

        using var pdbStream = File.OpenRead(pdbPath);
        using var provider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
        var reader = provider.GetMetadataReader();

        foreach (var methodHandle in reader.MethodDebugInformation)
        {
            var methodDebug = reader.GetMethodDebugInformation(methodHandle);
            if (methodDebug.SequencePointsBlob.IsNil)
                continue;

            // Get the method’s containing method definition to check its RVA
            var methodDefHandle = methodDebug.Document.IsNil ? default : (MethodDefinitionHandle)methodHandle.ToDefinitionHandle();
            if (methodDefHandle.IsNil) continue;

            var methodDef = peReader.GetMetadataReader().GetMethodDefinition(methodDefHandle);
            int methodRva = methodDef.RelativeVirtualAddress;

            // Skip if the RVA isn’t in the range
            if (methodRva == 0 || rva < methodRva)
                continue;

            // Read all sequence points (line info)
            var points = methodDebug.GetSequencePoints().ToArray();
            if (points.Length == 0) continue;

            // Find nearest sequence point
            var nearest = points.OrderBy(p => Math.Abs(rva - (methodRva + p.Offset)))
                                .FirstOrDefault();

            if (nearest.StartLine != 0 && !nearest.Document.IsNil)
            {
                var doc = reader.GetDocument(nearest.Document);
                var filePath = reader.GetString(doc.Name);
                return (filePath, nearest.StartLine);
            }
        }

        return null;
    }
}