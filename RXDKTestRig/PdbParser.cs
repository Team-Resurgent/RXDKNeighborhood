// PdbSimpleLines.cs
// Usage: dotnet run -- <path-to-pdb>
// Pure C#, cross-platform. MSF reader + heuristic line extraction.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class PdbSimpleLines
{
    const int PdbSignatureLength = 32;

    static readonly string[] FileExts = new[] { ".c", ".cpp", ".cc", ".cxx", ".h", ".hpp", ".inl", ".asm", ".s" };

    public static void Main(string file)
    {

        string pdbPath = file;
        if (!File.Exists(pdbPath)) { Console.WriteLine("File not found: " + pdbPath); return; }

        try
        {
            using var fs = File.OpenRead(pdbPath);
            using var br = new BinaryReader(fs);

            // Read header
            Console.WriteLine($"Reading PDB header from file: {pdbPath} (size: {fs.Length} bytes)");
            
            var magicBytes = br.ReadBytes(PdbSignatureLength);
            string magic = Encoding.ASCII.GetString(magicBytes).TrimEnd('\0');
            Console.WriteLine($"Magic: '{magic}' (length: {magic.Length})");
            
            // Clean up magic string - sometimes has trailing garbage
            magic = magic.Trim('\0', ' ', '\t', '\r', '\n');
            
            if (!magic.StartsWith("Microsoft C/C++ MSF"))
            {
                Console.WriteLine("Warning: PDB signature not recognized. Found: " + magic);
            }
            
            int blockSize = br.ReadInt32();
            int freeBlockMap = br.ReadInt32();
            int numBlocks = br.ReadInt32();
            int numDirectoryBytes = br.ReadInt32();
            int blockMapAddr = br.ReadInt32();

            Console.WriteLine($"Block size: {blockSize}");
            Console.WriteLine($"Free block map: {freeBlockMap}");
            Console.WriteLine($"Number of blocks: {numBlocks}");
            Console.WriteLine($"Directory bytes: {numDirectoryBytes}");
            Console.WriteLine($"Block map address: {blockMapAddr}");

            if (blockSize <= 0 || blockSize > 65536)
                throw new Exception($"Invalid block size: {blockSize}");
            if (numDirectoryBytes <= 0 || numDirectoryBytes > fs.Length)
                throw new Exception($"Invalid directory size: {numDirectoryBytes}");

            int numDirectoryBlocks = (numDirectoryBytes + blockSize - 1) / blockSize;
            Console.WriteLine($"Directory spans {numDirectoryBlocks} blocks");
            
            // Handle special case where block map address is 0 (some VC70 variants)
            if (blockMapAddr == 0)
            {
                Console.WriteLine("Block map address is 0 - trying alternative approach");
                // For some VC70 PDBs, the directory block addresses are stored differently
                // Try reading from the end of the file or use a different calculation
                
                // Alternative 1: Block map might be at the last few blocks
                int[] candidateBlockMapAddrs = { numBlocks - 1, numBlocks - 2, 1, 2 };
                
                foreach (int candidateAddr in candidateBlockMapAddrs)
                {
                    Console.WriteLine($"Trying block map at block {candidateAddr}");
                    if (TryReadBlockMapAt(fs, br, candidateAddr, blockSize, numDirectoryBlocks, out var foundDirBlocks))
                    {
                        blockMapAddr = candidateAddr;
                        Console.WriteLine($"Successfully found block map at block {candidateAddr}");
                        break;
                    }
                }
                
                if (blockMapAddr == 0)
                {
                    Console.WriteLine("Could not locate block map, falling back to heuristic scan");
                    // Fall back to scanning the entire file for directory blocks
                    var fallbackResults = FallbackToHeuristicScan(fs, blockSize);
                    
                    // Print results from fallback
                    Console.WriteLine($"\nFallback scan found {fallbackResults.Count} potential files with line info:\n");
                    foreach (var result in fallbackResults)
                    {
                        Console.WriteLine($"=== {result.FilePath} ===");
                        int show = Math.Min(10, result.LinePairs.Count);
                        for (int i = 0; i < show; i++)
                        {
                            var (offs, line) = result.LinePairs[i];
                            Console.WriteLine($"  Line {line,5}  Off=0x{offs:X8}");
                        }
                        if (result.LinePairs.Count > show)
                            Console.WriteLine($"  ... (+{result.LinePairs.Count - show} more)");
                        Console.WriteLine();
                    }
                    
                    return;
                }
            }
            else if (blockMapAddr < 0 || blockMapAddr >= numBlocks)
            {
                Console.WriteLine($"Invalid block map address: {blockMapAddr}, trying alternatives");
                blockMapAddr = Math.Min(blockMapAddr, numBlocks - 1);
                blockMapAddr = Math.Max(blockMapAddr, 1);
                Console.WriteLine($"Adjusted block map address to: {blockMapAddr}");
            }

            // --- 2️⃣ Locate directory blocks (FIXED) ---
            long blockMapSeekPos = blockMapAddr * (long)blockSize;
            Console.WriteLine($"Seeking to block map at 0x{blockMapSeekPos:X}");
            
            if (blockMapSeekPos >= fs.Length)
                throw new Exception($"Block map position 0x{blockMapSeekPos:X} beyond file size {fs.Length}");
                
            fs.Seek(blockMapSeekPos, SeekOrigin.Begin);

            // Each block number is 4 bytes, so:
            int blockMapSize = numDirectoryBlocks * 4;
            int blocksForBlockMap = (blockMapSize + blockSize - 1) / blockSize;
            
            Console.WriteLine($"Block map size: {blockMapSize} bytes across {blocksForBlockMap} blocks");

            var dirBlockAddrs = new List<int>();

            for (int b = 0; b < blocksForBlockMap; b++)
            {
                int maxToRead = Math.Min(blockSize, blockMapSize - b * blockSize);
                Console.WriteLine($"Reading block map chunk {b}: {maxToRead} bytes");
                
                // Check if we can read this much
                if (fs.Position + maxToRead > fs.Length)
                {
                    maxToRead = (int)(fs.Length - fs.Position);
                    Console.WriteLine($"Adjusted read size to {maxToRead} bytes (end of file)");
                    if (maxToRead <= 0) break;
                }
                
                var buf = br.ReadBytes(maxToRead);
                Console.WriteLine($"Actually read {buf.Length} bytes");
                
                for (int i = 0; i < buf.Length; i += 4)
                {
                    if (i + 4 <= buf.Length)
                    {
                        int blockAddr = BitConverter.ToInt32(buf, i);
                        dirBlockAddrs.Add(blockAddr);
                        Console.WriteLine($"  Directory block: {blockAddr}");
                    }
                }
            }
            
            Console.WriteLine($"Found {dirBlockAddrs.Count} directory block addresses");

            // Filter out invalid directory blocks (zeros and duplicates)
            var validDirBlocks = new List<int>();
            var seenBlocks = new HashSet<int>();
            
            foreach (var addr in dirBlockAddrs)
            {
                if (addr > 0 && addr < numBlocks && !seenBlocks.Contains(addr))
                {
                    validDirBlocks.Add(addr);
                    seenBlocks.Add(addr);
                }
            }
            
            Console.WriteLine($"Found {validDirBlocks.Count} valid directory blocks out of {dirBlockAddrs.Count} total");
            
            if (validDirBlocks.Count == 0)
            {
                Console.WriteLine("No valid directory blocks found - falling back to heuristic scan");
                var fallbackResults = FallbackToHeuristicScan(fs, blockSize);
                
                // Print results from fallback
                Console.WriteLine($"\nFallback scan found {fallbackResults.Count} potential files with line info:\n");
                foreach (var result in fallbackResults)
                {
                    Console.WriteLine($"=== {result.FilePath} ===");
                    int show = Math.Min(10, result.LinePairs.Count);
                    for (int i = 0; i < show; i++)
                    {
                        var (offs, line) = result.LinePairs[i];
                        Console.WriteLine($"  Line {line,5}  Off=0x{offs:X8}");
                    }
                    if (result.LinePairs.Count > show)
                        Console.WriteLine($"  ... (+{result.LinePairs.Count - show} more)");
                    Console.WriteLine();
                }
                
                return;
            }
            
            // Calculate actual directory size based on valid blocks
            int actualDirectorySize = Math.Min(numDirectoryBytes, validDirBlocks.Count * blockSize);
            var dirBytes = new byte[actualDirectorySize];
            int bytesRemaining = actualDirectorySize;
            int destOffset = 0;
            
            Console.WriteLine($"Reading directory: {actualDirectorySize} bytes across {validDirBlocks.Count} valid blocks");
            
            for (int i = 0; i < validDirBlocks.Count && bytesRemaining > 0; i++)
            {
                var addr = validDirBlocks[i];
                int toRead = Math.Min(blockSize, bytesRemaining);
                long seekPos = addr * (long)blockSize;
                
                Console.WriteLine($"Block {i}: addr={addr}, seekPos=0x{seekPos:X}, toRead={toRead}, fileSize={fs.Length}");
                
                // Check if seek position is valid
                if (seekPos >= fs.Length)
                {
                    Console.WriteLine($"[ERROR] Seek position 0x{seekPos:X} beyond file size {fs.Length}");
                    break;
                }
                
                fs.Seek(seekPos, SeekOrigin.Begin);
                int actuallyRead = fs.Read(dirBytes, destOffset, toRead);
                
                Console.WriteLine($"  Read {actuallyRead}/{toRead} bytes");
                
                if (actuallyRead == 0)
                {
                    Console.WriteLine("[WARN] Read 0 bytes, stopping directory read");
                    break;
                }
                
                if (actuallyRead != toRead)
                {
                    Console.WriteLine($"[WARN] Short read: expected {toRead}, got {actuallyRead}");
                    // Don't throw, just log and continue with what we got
                }
                
                destOffset += actuallyRead;
                bytesRemaining -= actuallyRead;
            }
            
            if (bytesRemaining > 0)
                Console.WriteLine($"[WARN] Directory read incomplete: {bytesRemaining} bytes remaining");
            
            // If we got some directory data, try to continue
            if (destOffset == 0)
            {
                Console.WriteLine("Failed to read any directory data - falling back to heuristic scan");
                var fallbackResults = FallbackToHeuristicScan(fs, blockSize);
                
                // Print results from fallback
                Console.WriteLine($"\nFallback scan found {fallbackResults.Count} potential files with line info:\n");
                foreach (var result in fallbackResults)
                {
                    Console.WriteLine($"=== {result.FilePath} ===");
                    int show = Math.Min(10, result.LinePairs.Count);
                    for (int i = 0; i < show; i++)
                    {
                        var (offs, line) = result.LinePairs[i];
                        Console.WriteLine($"  Line {line,5}  Off=0x{offs:X8}");
                    }
                    if (result.LinePairs.Count > show)
                        Console.WriteLine($"  ... (+{result.LinePairs.Count - show} more)");
                    Console.WriteLine();
                }
                
                return;
            }
                
            // Adjust the array size if we read less than expected
            if (destOffset < numDirectoryBytes)
            {
                var adjustedDirBytes = new byte[destOffset];
                Array.Copy(dirBytes, adjustedDirBytes, destOffset);
                dirBytes = adjustedDirBytes;
                Console.WriteLine($"Adjusted directory size to {destOffset} bytes");
            }

            using var dirStream = new BinaryReader(new MemoryStream(dirBytes));

            int numStreams;
            try
            {
                numStreams = dirStream.ReadInt32();
                Console.WriteLine($"Directory reports {numStreams} streams");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read stream count from directory: {ex.Message}");
                Console.WriteLine("Falling back to heuristic scan");
                var fallbackResults = FallbackToHeuristicScan(fs, blockSize);
                
                // Print results from fallback
                Console.WriteLine($"\nFallback scan found {fallbackResults.Count} potential files with line info:\n");
                foreach (var result in fallbackResults)
                {
                    Console.WriteLine($"=== {result.FilePath} ===");
                    int show = Math.Min(10, result.LinePairs.Count);
                    for (int i = 0; i < show; i++)
                    {
                        var (offs, line) = result.LinePairs[i];
                        Console.WriteLine($"  Line {line,5}  Off=0x{offs:X8}");
                    }
                    if (result.LinePairs.Count > show)
                        Console.WriteLine($"  ... (+{result.LinePairs.Count - show} more)");
                    Console.WriteLine();
                }
                
                return;
            }
            
            if (numStreams < 0 || numStreams > 1_000_000)
            {
                Console.WriteLine($"Suspicious stream count: {numStreams} - falling back to heuristic scan");
                var fallbackResults = FallbackToHeuristicScan(fs, blockSize);
                
                // Print results from fallback
                Console.WriteLine($"\nFallback scan found {fallbackResults.Count} potential files with line info:\n");
                foreach (var result in fallbackResults)
                {
                    Console.WriteLine($"=== {result.FilePath} ===");
                    int show = Math.Min(10, result.LinePairs.Count);
                    for (int i = 0; i < show; i++)
                    {
                        var (offs, line) = result.LinePairs[i];
                        Console.WriteLine($"  Line {line,5}  Off=0x{offs:X8}");
                    }
                    if (result.LinePairs.Count > show)
                        Console.WriteLine($"  ... (+{result.LinePairs.Count - show} more)");
                    Console.WriteLine();
                }
                
                return;
            }

            var streamSizes = new int[numStreams];
            for (int i = 0; i < numStreams; i++)
                streamSizes[i] = dirStream.ReadInt32();

            var streamBlocks = new List<int[]>();
            for (int i = 0; i < numStreams; i++)
            {
                int numBlocksForStream = streamSizes[i] == 0 ? 0 : (streamSizes[i] + blockSize - 1) / blockSize;
                var blocks = new int[numBlocksForStream];
                for (int b = 0; b < numBlocksForStream; b++)
                    blocks[b] = dirStream.ReadInt32();
                streamBlocks.Add(blocks);
            }

            Console.WriteLine($"PDB Streams: {numStreams}");
            for (int i = 0; i < numStreams; i++)
                Console.WriteLine($"  Stream {i:D2}: {streamSizes[i]} bytes, blocks={streamBlocks[i].Length}");

            // Load all streams into memory (be mindful on huge PDBs)
            var allStreams = new List<byte[]>();
            for (int i = 0; i < numStreams; i++)
            {
                allStreams.Add(ReadStream(fs, streamBlocks[i], blockSize, streamSizes[i]));
            }

            var results = new List<FileLineInfo>();

            // Try VC70-specific DBI stream parsing first (usually stream 3)
            Console.WriteLine("\nTrying VC70 DBI stream parsing...");
            if (numStreams > 3 && allStreams[3] != null && allStreams[3].Length > 0)
            {
                var dbiResults = ParseDbiStream(allStreams[3], 3);
                results.AddRange(dbiResults);
                Console.WriteLine($"DBI stream parsing found {dbiResults.Count} files with line info");
            }

            // If DBI parsing didn't work well, fall back to heuristic scan
            if (results.Count == 0)
            {
                Console.WriteLine("\nFalling back to heuristic scanning...");
                for (int si = 0; si < allStreams.Count; si++)
                {
                    var data = allStreams[si];
                    if (data == null || data.Length == 0) continue;

                    var foundPaths = FindCandidateFilePaths(data);
                    foreach (var fp in foundPaths)
                    {
                        var candidate = TryExtractLinePairs(data, fp.Offset);
                        if (candidate.LinePairs.Count > 0)
                        {
                            candidate.StreamIndex = si;
                            results.Add(candidate);
                            Console.WriteLine($"[Found] {candidate.FilePath}  (stream {si}, offset 0x{fp.Offset:X}) -> {candidate.LinePairs.Count} entries");
                        }
                    }
                }
            }

            // Deduplicate by path and stream & print sample
            var grouped = results.GroupBy(r => (r.FilePath, r.StreamIndex)).ToList();
            Console.WriteLine($"\nSummary: found {grouped.Count} distinct file entries with line-pair heuristics.\n");

            foreach (var g in grouped)
            {
                var first = g.First();
                Console.WriteLine($"=== {first.FilePath}  (stream {first.StreamIndex}) ===");
                int show = Math.Min(100, first.LinePairs.Count);
                for (int i = 0; i < show; i++)
                {
                    var (offs, line) = first.LinePairs[i];
                    Console.WriteLine($"  Line {line,5}  Off=0x{offs:X8}");
                }
                if (first.LinePairs.Count > show)
                    Console.WriteLine($"  ... (+{first.LinePairs.Count - show} more)");
                Console.WriteLine();
            }

            if (grouped.Count == 0)
            {
                Console.WriteLine("No filename + line pairs found by heuristics. You can try increasing the search window or provide a sample PDB to iterate.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }

    // Try to read block map at a specific location
    static bool TryReadBlockMapAt(FileStream fs, BinaryReader br, int blockAddr, int blockSize, int numDirectoryBlocks, out List<int> dirBlockAddrs)
    {
        dirBlockAddrs = new List<int>();
        
        try
        {
            long blockMapSeekPos = blockAddr * (long)blockSize;
            if (blockMapSeekPos >= fs.Length)
                return false;
                
            fs.Seek(blockMapSeekPos, SeekOrigin.Begin);
            
            // Read potential directory block addresses
            int blockMapSize = numDirectoryBlocks * 4;
            int maxToRead = Math.Min(blockSize, blockMapSize);
            
            var buf = br.ReadBytes(maxToRead);
            if (buf.Length < 4)
                return false;
                
            // Extract block addresses and validate they're reasonable
            for (int i = 0; i < buf.Length; i += 4)
            {
                if (i + 4 <= buf.Length)
                {
                    int blockNum = BitConverter.ToInt32(buf, i);
                    // Validate block number is reasonable
                    if (blockNum > 0 && blockNum < fs.Length / blockSize)
                    {
                        dirBlockAddrs.Add(blockNum);
                    }
                    else if (dirBlockAddrs.Count == 0)
                    {
                        // If the first block address is invalid, this probably isn't the right location
                        return false;
                    }
                }
            }
            
            // Need at least one valid directory block
            return dirBlockAddrs.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    // Fallback to heuristic scanning when block map can't be found
    static List<FileLineInfo> FallbackToHeuristicScan(FileStream fs, int blockSize)
    {
        Console.WriteLine("Starting fallback heuristic scan...");
        var results = new List<FileLineInfo>();
        
        try
        {
            // Read the entire file in chunks and scan for patterns
            const int chunkSize = 64 * 1024; // 64KB chunks
            byte[] chunk = new byte[chunkSize];
            int offset = 0;
            
            fs.Seek(0, SeekOrigin.Begin);
            
            while (offset < fs.Length)
            {
                int bytesRead = fs.Read(chunk, 0, (int)Math.Min(chunkSize, fs.Length - offset));
                if (bytesRead == 0) break;
                
                // Look for filename patterns
                var foundPaths = FindCandidateFilePaths(chunk);
                foreach (var fp in foundPaths)
                {
                    var candidate = TryExtractLinePairs(chunk, fp.Offset);
                    if (candidate.LinePairs.Count > 0)
                    {
                        candidate.StreamIndex = -1; // Mark as heuristic
                        results.Add(candidate);
                    }
                }
                
                offset += bytesRead;
                if (offset % (1024 * 1024) == 0) // Progress every MB
                {
                    Console.WriteLine($"Scanned {offset / (1024 * 1024)} MB...");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in fallback scan: {ex.Message}");
        }
        
        Console.WriteLine($"Fallback scan found {results.Count} potential line info sections");
        return results;
    }

    // Read a stream given its block list
    static byte[] ReadStream(FileStream fs, int[] blocks, int blockSize, int streamSize)
    {
        if (streamSize == 0) return Array.Empty<byte>();
        var res = new byte[streamSize];
        int off = 0;
        foreach (var b in blocks)
        {
            fs.Seek(b * (long)blockSize, SeekOrigin.Begin);
            int toRead = Math.Min(blockSize, streamSize - off);
            int got = fs.Read(res, off, toRead);
            if (got != toRead)
                throw new EndOfStreamException("Short read while reading stream");
            off += got;
            if (off >= streamSize) break;
        }
        return res;
    }

    // Parse VC70 DBI (Debug Information) stream for line number information
    static List<FileLineInfo> ParseDbiStream(byte[] dbiData, int streamIndex)
    {
        var results = new List<FileLineInfo>();
        
        try
        {
            using var br = new BinaryReader(new MemoryStream(dbiData));
            
            // Read DBI header
            int signature = br.ReadInt32();
            int version = br.ReadInt32();
            int age = br.ReadInt32();
            short globalStreamIndex = br.ReadInt16();
            short buildNumber = br.ReadInt16();
            short publicStreamIndex = br.ReadInt16();
            short pdbDllVersion = br.ReadInt16();
            short symbolRecordsStreamIndex = br.ReadInt16();
            short pdbDllRbld = br.ReadInt16();
            int moduleInfoSize = br.ReadInt32();
            int sectionContribSize = br.ReadInt32();
            int sectionMapSize = br.ReadInt32();
            int sourceInfoSize = br.ReadInt32();
            int typeServerMapSize = br.ReadInt32();
            int mfcTypeServerIndex = br.ReadInt32();
            int optDbgHdrSize = br.ReadInt32();
            int ecSubstreamSize = br.ReadInt32();

            Console.WriteLine($"DBI Version: {version}, Age: {age}");
            Console.WriteLine($"Module Info Size: {moduleInfoSize}, Source Info Size: {sourceInfoSize}");

            // Skip to module info substream
            if (moduleInfoSize > 0)
            {
                var moduleInfos = new List<ModuleInfo>();
                int moduleInfoEnd = (int)br.BaseStream.Position + moduleInfoSize;
                
                while (br.BaseStream.Position < moduleInfoEnd)
                {
                    var modInfo = ReadModuleInfo(br);
                    if (modInfo != null)
                        moduleInfos.Add(modInfo);
                }

                // Skip section contribution and section map
                br.BaseStream.Seek(sectionContribSize + sectionMapSize, SeekOrigin.Current);

                // Parse source info substream
                if (sourceInfoSize > 0 && br.BaseStream.Position < br.BaseStream.Length)
                {
                    var sourceResults = ParseSourceInfoSubstream(br, sourceInfoSize, moduleInfos);
                    results.AddRange(sourceResults);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing DBI stream: {ex.Message}");
            // Fall back to heuristic scanning of this stream
            var foundPaths = FindCandidateFilePaths(dbiData);
            foreach (var fp in foundPaths)
            {
                var candidate = TryExtractLinePairs(dbiData, fp.Offset);
                if (candidate.LinePairs.Count > 0)
                {
                    candidate.StreamIndex = streamIndex;
                    results.Add(candidate);
                }
            }
        }

        return results;
    }

    // Read module info record
    static ModuleInfo? ReadModuleInfo(BinaryReader br)
    {
        try
        {
            if (br.BaseStream.Position + 64 > br.BaseStream.Length)
                return null;

            var info = new ModuleInfo();
            br.ReadInt32(); // unused1
            info.SectionContrib = br.ReadInt32();
            br.ReadInt16(); // flags
            info.ModuleStream = br.ReadInt16();
            info.SymByteSize = br.ReadInt32();
            info.C11ByteSize = br.ReadInt32();
            info.C13ByteSize = br.ReadInt32();
            info.SourceFileCount = br.ReadInt16();
            br.ReadInt16(); // padding
            br.ReadInt32(); // unused2
            info.SourceFileNameIndex = br.ReadInt32();
            info.PdbFilePathNameIndex = br.ReadInt32();

            // Read module name (null-terminated)
            var nameBytes = new List<byte>();
            byte b;
            while ((b = br.ReadByte()) != 0 && nameBytes.Count < 260)
                nameBytes.Add(b);
            info.ModuleName = Encoding.UTF8.GetString(nameBytes.ToArray());

            // Read object file name (null-terminated)
            var objNameBytes = new List<byte>();
            while ((b = br.ReadByte()) != 0 && objNameBytes.Count < 260)
                objNameBytes.Add(b);
            info.ObjectFileName = Encoding.UTF8.GetString(objNameBytes.ToArray());

            // Align to 4-byte boundary
            while (br.BaseStream.Position % 4 != 0)
                br.ReadByte();

            return info;
        }
        catch
        {
            return null;
        }
    }

    // Parse source info substream
    static List<FileLineInfo> ParseSourceInfoSubstream(BinaryReader br, int sourceInfoSize, List<ModuleInfo> moduleInfos)
    {
        var results = new List<FileLineInfo>();
        
        try
        {
            int sourceInfoStart = (int)br.BaseStream.Position;
            int sourceInfoEnd = sourceInfoStart + sourceInfoSize;

            // Read source file info
            while (br.BaseStream.Position < sourceInfoEnd - 8)
            {
                try
                {
                    // Look for CV_SIGNATURE_C13 (0x4)
                    int signature = br.ReadInt32();
                    int length = br.ReadInt32();
                    
                    if (signature == 0x4 && length > 0 && length < sourceInfoSize)
                    {
                        // This might be C13 line info
                        var lineInfo = ParseC13LineInfo(br, length);
                        results.AddRange(lineInfo);
                    }
                    else
                    {
                        // Skip this section
                        if (length > 0 && br.BaseStream.Position + length <= sourceInfoEnd)
                            br.BaseStream.Seek(length, SeekOrigin.Current);
                        else
                            break;
                    }
                }
                catch
                {
                    // If parsing fails, try to find the next potential signature
                    if (br.BaseStream.Position < sourceInfoEnd - 1)
                        br.BaseStream.Seek(1, SeekOrigin.Current);
                    else
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing source info: {ex.Message}");
        }

        return results;
    }

    // Parse C13 line information
    static List<FileLineInfo> ParseC13LineInfo(BinaryReader br, int length)
    {
        var results = new List<FileLineInfo>();
        
        try
        {
            int sectionStart = (int)br.BaseStream.Position;
            int sectionEnd = sectionStart + length;

            while (br.BaseStream.Position < sectionEnd - 8)
            {
                int subsectionType = br.ReadInt32();
                int subsectionLength = br.ReadInt32();

                if (subsectionLength <= 0 || br.BaseStream.Position + subsectionLength > sectionEnd)
                    break;

                // DEBUG_S_LINES = 0xF2
                if (subsectionType == 0xF2)
                {
                    var lineInfo = ReadLinesSubsection(br, subsectionLength);
                    if (lineInfo != null)
                        results.Add(lineInfo);
                }
                else
                {
                    // Skip unknown subsection
                    br.BaseStream.Seek(subsectionLength, SeekOrigin.Current);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing C13 line info: {ex.Message}");
        }

        return results;
    }

    // Read lines subsection
    static FileLineInfo? ReadLinesSubsection(BinaryReader br, int length)
    {
        try
        {
            int subsectionStart = (int)br.BaseStream.Position;
            
            // Read header
            int codeOffset = br.ReadInt32();
            short segment = br.ReadInt16();
            short flags = br.ReadInt16();
            int codeLength = br.ReadInt32();

            var fileInfo = new FileLineInfo();
            
            // Read file blocks
            while (br.BaseStream.Position < subsectionStart + length - 8)
            {
                int fileId = br.ReadInt32();
                int lineCount = br.ReadInt32();
                int fileBlockSize = br.ReadInt32();

                if (lineCount <= 0 || lineCount > 100000)
                    break;

                // Read line/offset pairs
                for (int i = 0; i < lineCount && br.BaseStream.Position < subsectionStart + length - 8; i++)
                {
                    int offset = br.ReadInt32();
                    int lineAndColumn = br.ReadInt32();
                    
                    int lineNumber = lineAndColumn & 0xFFFFFF; // Lower 24 bits
                    
                    if (lineNumber > 0 && lineNumber < 1000000)
                    {
                        fileInfo.LinePairs.Add((codeOffset + offset, lineNumber));
                    }
                }

                // Try to find filename (this is a simplified approach)
                if (string.IsNullOrEmpty(fileInfo.FilePath))
                {
                    fileInfo.FilePath = $"File_{fileId}"; // Placeholder
                }
            }

            return fileInfo.LinePairs.Count > 0 ? fileInfo : null;
        }
        catch
        {
            return null;
        }
    }

    class ModuleInfo
    {
        public int SectionContrib { get; set; }
        public short ModuleStream { get; set; }
        public int SymByteSize { get; set; }
        public int C11ByteSize { get; set; }
        public int C13ByteSize { get; set; }
        public short SourceFileCount { get; set; }
        public int SourceFileNameIndex { get; set; }
        public int PdbFilePathNameIndex { get; set; }
        public string ModuleName { get; set; } = "";
        public string ObjectFileName { get; set; } = "";
    }

    // Finds null-terminated ASCII strings that look like file paths with known extensions.
    static List<(int Offset, string Path)> FindCandidateFilePaths(byte[] data)
    {
        var res = new List<(int, string)>();
        int i = 0;
        while (i < data.Length)
        {
            // Look for ASCII printable chars and backslash or slash
            if (IsPrintable(data[i]))
            {
                int start = i;
                while (i < data.Length && IsPrintable(data[i])) i++;
                int len = i - start;
                if (len >= 4 && len <= 260) // plausible path length
                {
                    string s = Encoding.ASCII.GetString(data, start, len);
                    // Trim potential non-path prefix/suffix
                    s = s.Trim('\0', '\r', '\n');
                    // quick filter for path-like content
                    if ((s.Contains('\\') || s.Contains('/')) && FileExts.Any(ext => s.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    {
                        // basic sanity: contains a filename char
                        res.Add((start, s));
                    }
                }
            }
            else
            {
                i++;
            }
        }
        return res;
    }

    static bool IsPrintable(byte b)
    {
        return b >= 0x20 && b <= 0x7E;
    }

    // Try to extract repeating (uint32 offset, uint32 line) or (uint32 offset, uint16 line) pairs around a file path offset.
    static FileLineInfo TryExtractLinePairs(byte[] data, int pathOffset)
    {
        var info = new FileLineInfo();
        info.FilePath = ReadNullTerminatedAscii(data, pathOffset);

        // Search forward and backward within a window to find repeating numeric pairs.
        const int window = 4096; // bytes to look around the path
        int start = Math.Max(0, pathOffset - window);
        int end = Math.Min(data.Length - 8, pathOffset + window);

        var candidates = new List<(int offs, int line)>();

        // Attempt pattern: repeated (uint32 offset, uint32 line) pairs
        for (int pos = start; pos + 8 <= end; pos++)
        {
            uint a = BitConverter.ToUInt32(data, pos);
            uint b = BitConverter.ToUInt32(data, pos + 4);

            if (IsPlausibleOffset(a, data.Length) && IsPlausibleLineNumber(b))
            {
                // check for run of at least 3 pairs spaced by 8 bytes
                int run = 1;
                var runItems = new List<(int offs, int line)>();
                runItems.Add(((int)a, (int)b));
                int next = pos + 8;
                while (next + 8 <= end)
                {
                    uint a2 = BitConverter.ToUInt32(data, next);
                    uint b2 = BitConverter.ToUInt32(data, next + 4);
                    if (IsPlausibleOffset(a2, data.Length) && IsPlausibleLineNumber(b2))
                    {
                        runItems.Add(((int)a2, (int)b2));
                        run++;
                        next += 8;
                    }
                    else break;
                }

                if (run >= 3)
                {
                    // add to candidates (first run)
                    foreach (var it in runItems) candidates.Add(it);
                    break; // keep first good run
                }
            }

            // Try (uint32 offset, uint16 line) pattern (6 bytes)
            if (pos + 6 <= end)
            {
                uint a6 = BitConverter.ToUInt32(data, pos);
                ushort b6 = BitConverter.ToUInt16(data, pos + 4);
                if (IsPlausibleOffset(a6, data.Length) && IsPlausibleLineNumber(b6))
                {
                    int run = 1;
                    var runItems = new List<(int offs, int line)>() { ((int)a6, (int)b6) };
                    int next = pos + 6;
                    while (next + 6 <= end)
                    {
                        uint a2 = BitConverter.ToUInt32(data, next);
                        ushort b2 = BitConverter.ToUInt16(data, next + 4);
                        if (IsPlausibleOffset(a2, data.Length) && IsPlausibleLineNumber(b2))
                        {
                            runItems.Add(((int)a2, (int)b2));
                            run++;
                            next += 6;
                        }
                        else break;
                    }
                    if (run >= 3)
                    {
                        foreach (var it in runItems) candidates.Add(it);
                        break;
                    }
                }
            }
        }

        // De-dup and sort by offset
        var uniq = candidates.Distinct().OrderBy(p => p.offs).ToList();
        info.LinePairs.AddRange(uniq);
        return info;
    }

    static bool IsPlausibleOffset(uint a, int streamLength)
    {
        // offsets inside module / function code are usually < stream length * 16k etc,
        // but be permissive: offset should be non-zero and not extremely large.
        return a != 0 && a < (uint)(streamLength * 1000L);
    }

    static bool IsPlausibleLineNumber(uint n)
    {
        return n > 0 && n < 1000000; // generous upper bound
    }

    static bool IsPlausibleLineNumber(ushort n)
    {
        return n > 0 && n < 100000;
    }

    static string ReadNullTerminatedAscii(byte[] data, int offset)
    {
        if (offset < 0 || offset >= data.Length) return "";
        int end = offset;
        while (end < data.Length && data[end] != 0) end++;
        return Encoding.ASCII.GetString(data, offset, end - offset);
    }

    class FileLineInfo
    {
        public string FilePath { get; set; } = "";
        public int StreamIndex { get; set; }
        public List<(int offs, int line)> LinePairs { get; } = new List<(int offs, int line)>();
    }
}