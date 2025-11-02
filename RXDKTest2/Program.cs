using Microsoft.DiaSymReader;
using System.Reflection;

namespace RXDKTest2
{
    public class SymReaderMetadataProvider : ISymReaderMetadataProvider
    {
        public unsafe bool TryGetStandaloneSignature(int standaloneSignatureToken, out byte* signature, out int length)
        {
            throw new NotImplementedException();
        }

        public bool TryGetTypeDefinitionInfo(int typeDefinitionToken, out string namespaceName, out string typeName, out TypeAttributes attributes)
        {
            throw new NotImplementedException();
        }

        public bool TryGetTypeReferenceInfo(int typeReferenceToken, out string namespaceName, out string typeName)
        {
            throw new NotImplementedException();
        }
    }

    internal class Program
    {

        static void Main(string[] args)
        {
            unsafe
            {

            var pdbFilePath = "C:\\Users\\eq2k\\Downloads\\Daemon-X.pdb";
            using (var pdbStream = File.OpenRead(pdbFilePath))
            {
                var metadataProvider = new SymReaderMetadataProvider();
                var reader = SymUnmanagedReaderFactory.CreateReader<ISymUnmanagedReader5>(pdbStream, metadataProvider);
                var docs = reader.GetDocuments();
                foreach (var doc in docs)
                {
                    var name = doc.GetName();
                    if (name.Contains("fileb"))
                    {
                        reader.GetPortableDebugMetadata(out var meta, out var size);
                    }
                }
 

                int q = 1;
            }

            }
        }
    }
}
