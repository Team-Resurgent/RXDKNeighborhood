
using SixLabors.ImageSharp.PixelFormats;
using static SharpPdb.Windows.DebugSubsections.LinesSubsection;
using System.Net;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace RXDKTestRig
{
    //    XBEINFO NAME = "E:\PrometheOSXbe.xbe" ONDISKONLY
    //REBOOT WAIT WARM
    //TITLE NOPERSIST
    //TITLE NAME = "PrometheOs.xbe" DIR="e:\Prometheosxbe\" CMDLINE
    //DEBUGGER CONNECT
    //BREAK START
    //STOPON CREATETHREAD
    //GO
    //GETPID
    //STOPON CREATETHREAD FCE
    //THREADS
    //MODULES
    //threadinfo THREAD=28
    //CONTINUE THREAD = 28
    //GETCONTEXT THREAD = 28 CONTROL INT F P
    //GETCONTEXT THREAD = 28
    //MODSECTIONS NAME = "PrometheOSxbe.exe"
    //BREAK ADDR = 0X0024c b50
    //BREAK ADDR=0x001e0 eb1 CLEAR


//Dia2Dump.exe -lsrc h:\git\prometheos-builder\tools\prometheosxbe\prometheosxbe\kratos.cpp xdk.pdb

//gives for example

//line 276 at[00233A81][0002:000677C1], len = 0x1B

//00233A81 is important virtual address
//len is bytes of code for line


////kratos start wps
//line 153 at[0023360E][0002:0006734E], len = 0x13

    internal class Program
    {

        [SupportedOSPlatform("windows")]
        static void Main(string[] args)
        {
            using var pdbParser = new PdbParser();

            pdbParser.LoadPdb("H:\\Git\\PrometheOS-Builder\\Tools\\PrometheOSXbe\\PrometheOSXbe\\Debug-Dummy\\PrometheOSXbe.pdb");
            //if (pdbParser.TryGetRvaByFileLine("main.cpp", 809, 0, out var rva))
            //{
            //    pdbParser.TryGetSymbolsByRva(rva);
            //}

                //var pdb = new SharpPdb.Windows.PdbFile("Xdk.pdb");
                //PdbStringTable namesStream = pdb.InfoStream.NamesMap;

                //string pdbPath = @"C:\path\to\your.pdb";
                //string sourceFile = @"C:\path\to\file.cpp";
                //int lineNumber = 123; // the line you’re investigating


                //var diaSource = new DiaSource();
                //diaSource.loadDataFromPdb(@"xdk.pdb");

                //IDiaSession session;
                //diaSource.openSession(out session);\
                //session.globalScope.findInlineeLines
                //var m = session.findFile(null, "*", NameSearchOptions.None);


                //    //foreach (IDiaSourceFile file in sourceFiles)
                //    //{
                //    //    Console.WriteLine(file.fileName);
                //    //}


                //    var pdb = new SharpPdb.Native.PdbFileReader("Xdk.pdb");

                //var symbols = pdb.PublicSymbols;
                //for (int i = 0; i < symbols.Length; i++)
                //{
                //    var symbol = symbols[i];
                //    if (symbol.Name.Contains("getmodel", StringComparison.CurrentCultureIgnoreCase))
                //    {
                //        int qqq = 1;
                //    }

                //        //System.Diagnostics.Debug.Print($"{symbol.Name}");
                //}


                //for (int i = 0; i < pdb.Functions.Count; i++)
                //{

                //    var function = pdb.Functions[i];
                //    var s = pdb.PublicSymbols;
                //    //var symbol = pdb.PublicSymbols .Find(s => s.Address == fn.Address);
                //    var qq = 1;

                //System.Diagnostics.Debug.Print($"{function.Name}");

                //var linesProp = function.GetType().GetProperty("Lines") ?? function.GetType().GetProperty("LineNumbers");

                //var ooo = 1;
                //foreach (var lineInfo in function.li.LineInfos)
                //{
                //    var source = lineInfo.SourceFile?.Name ?? "<unknown>";
                //    Console.WriteLine($"  {source}:{lineInfo.LineNumber}  Address=0x{lineInfo.Address:X}");
                //}


                //using (var pdb = NativePdbReader.Open(pdbPath))
                //{
                //    foreach (var module in pdb.Modules)
                //    {
                //        foreach (var symbol in module.Symbols)
                //        {
                //            // Each symbol may have line info (function, block, etc.)
                //            foreach (var line in symbol.Lines)
                //            {
                //                if (line.SourceFile?.Name?.EndsWith(sourceFile, StringComparison.OrdinalIgnoreCase) == true &&
                //                    line.LineNumber == lineNumber)
                //                {
                //                    Console.WriteLine($"Symbol: {symbol.Name}");
                //                    Console.WriteLine($"Address: 0x{symbol.Address:X}");
                //                    Console.WriteLine($"Line: {line.LineNumber} in {line.SourceFile.Name}");
                //                    Console.WriteLine();
                //                }
                //            }
                //        }
                //    }
                //}

            _ = Task.Run(async () =>
            {
                var launcher = new Launcher();
                launcher.OnXbeLoaded += () =>
                {
                    if (pdbParser.TryGetRvaByFileLine("main.cpp", 809, 0, out var rva))
                    {
                        launcher.AddBreakpoint(rva);
                    }
                };
                launcher.OnBreakpoint += (addr, thread) =>
                {
                    pdbParser.TryGetFileLineByRva(addr, out var file, out var line, out var col);
                    launcher.GetContextInfo(addr, thread);
                    pdbParser.TryGetSymbolsByRva(addr, thread, launcher);
                    launcher.SendContinue(thread);
                };
                await launcher.Launch();
            });

            while (true)
            {
                Task.Delay(1000).Wait();
            }
        }

    
    }
}
