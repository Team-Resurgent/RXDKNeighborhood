using Dia2Lib;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RXDKXBDM
{
    public class SymbolInfo
    {
        public required string Type;
        public required string Name;
    }

    public class PdbParser : IDisposable
    {
        IDiaDataSource? _diaSource;
        IDiaSession? _diaSession;
        IntPtr _diaHandle;

        [Flags]
        enum NameSearchOptions
        {
            nsNone,
            nsfCaseSensitive = 0x1,
            nsfCaseInsensitive = 0x2,
            nsfFNameExt = 0x4,
            nsfRegularExpression = 0x8,
            nsfUndecoratedName = 0x10
        }

        private void ReleaseObject(object value)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            Marshal.ReleaseComObject(value);
        }

        public void Dispose()
        {
            if (_diaSession != null)
            {
                ReleaseObject(_diaSession);
                _diaSession = null;
            }

            if (_diaSource != null)
            {
                ReleaseObject(_diaSource);
                _diaSource = null;
            }
        }


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("msdia140.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern object DllGetClassObject([In] in Guid rclSid, [In] in Guid rIid);

        [ComImport]
        [ComVisible(false)]
        [Guid("00000001-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDiaClassFactory
        {
            void CreateInstance([MarshalAs(UnmanagedType.Interface)] object? aggregator,[In] in Guid refIid, [MarshalAs(UnmanagedType.Interface)] out object createdObject);
        }

        public bool LoadPdb(string pdbPath)
        {
            var userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RXDKNeighborhood");
            var diaPath = Path.Combine(userFolder, "msdia140.dll");
            if (!File.Exists(diaPath))
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("RXDKXBDM.msdia140.dll");
                using var file = File.Create(diaPath);
                stream?.CopyTo(file);
            }
            if (_diaHandle == IntPtr.Zero)
            {
                _diaHandle = LoadLibrary(diaPath);
                int err = Marshal.GetLastWin32Error();
                if (_diaHandle == IntPtr.Zero)
                {
                    return false;
                }
            }
            var guid = new Guid("{e6756135-1e65-4d17-8576-610761398c3c}");
            var classFactory = DllGetClassObject(guid, typeof(IDiaClassFactory).GetTypeInfo().GUID);
            if (classFactory is not IDiaClassFactory factoryInstance)
            {
                return false;
            }
            factoryInstance.CreateInstance(null, typeof(IDiaDataSource).GetTypeInfo().GUID, out var createdObject);
            if (createdObject is not IDiaDataSource dataSourceInstance)
            {
                return false;
            }
            _diaSource = dataSourceInstance;
            _diaSource.loadDataFromPdb(pdbPath);
            _diaSource.openSession(out _diaSession);
            return true;
        }

        public void ClosePdb()
        {
            Dispose();
        }

        public bool TryGetFilenames(out string[] filenames)
        {
            var filelist = new List<string>();

            if (_diaSession != null)
            {
                _diaSession.getEnumTables(out var enumTables);
                if (enumTables != null)
                {
                    foreach (IDiaTable table in enumTables)
                    {
                        if (table is IDiaEnumSourceFiles enumSourceFiles)
                        {
                            var sourceFileEnum = enumSourceFiles.GetEnumerator();
                            while (sourceFileEnum.MoveNext())
                            {
                                var sourceFile = (IDiaSourceFile)sourceFileEnum.Current;
                                filelist.Add(sourceFile.fileName);
                            }
                        }
                    }
                }
            }
            filenames = filelist.ToArray();
            return false;
        }

        public bool TryGetRvaByFileLine(string filename, uint line, uint column, out uint rva)
        {
            if (_diaSession != null)
            {
                _diaSession.findFile(null, filename, (uint)NameSearchOptions.nsfCaseInsensitive, out var files);
                foreach (IDiaSourceFile file in files)
                {
                    var compilandsEnum = file.compilands.GetEnumerator();
                    while (compilandsEnum.MoveNext())
                    {
                        _diaSession.findLinesByLinenum((IDiaSymbol)compilandsEnum.Current, file, line, column, out IDiaEnumLineNumbers lines);
                        var lineEnum = lines.GetEnumerator();
                        while (lineEnum.MoveNext())
                        {
                            var lineItem = (IDiaLineNumber)lineEnum.Current;
                            rva = lineItem.relativeVirtualAddress;
                            return true;
                        }
                    }
                }
            }
            rva = 0;
            return false;
        }

        public bool TryGetFileLineByRva(uint rva, out string file, out uint line, out uint column)
        {
            if (_diaSession != null)
            {
                IDiaEnumLineNumbers? lines = null;
                IDiaLineNumber? lineItem = null;
                IDiaSourceFile? sourceFile = null;
                try
                {
                    _diaSession.findLinesByRVA(rva, 1, out lines);
                    var lineEnum = lines.GetEnumerator();
                    while (lineEnum.MoveNext())
                    {
                        lineItem = (IDiaLineNumber)lineEnum.Current;
                        sourceFile = lineItem.sourceFile;

                        line = lineItem.lineNumber;
                        column = lineItem.columnNumber;
                        file = sourceFile.fileName;
                        return true;
                    }
                }
                finally
                {
                    if (sourceFile != null)
                    {
                        ReleaseObject(sourceFile);
                    }
                    if (lineItem != null)
                    {
                        ReleaseObject(lineItem);
                    }
                    if (lines != null)
                    {
                        ReleaseObject(lines);
                    }
                }
            }
            file = string.Empty;
            line = 0;
            column = 0;
            return false;
        }

        enum LocationType
        {
            LocIsNull,
            LocIsStatic,
            LocIsTLS,
            LocIsRegRel,
            LocIsThisRel,
            LocIsEnregistered,
            LocIsBitField,
            LocIsSlot,
            LocIsIlRel,
            LocInMetaData,
            LocIsConstant,
            LocIsRegRelAliasIndir,
            LocTypeMax
        };

        enum BasicType
        {
            btNoType = 0,
            btVoid = 1,
            btChar = 2,
            btWChar = 3,
            btInt = 6,
            btUInt = 7,
            btFloat = 8,
            btBCD = 9,
            btBool = 10,
            btLong = 13,
            btULong = 14,
            btCurrency = 25,
            btDate = 26,
            btVariant = 27,
            btComplex = 28,
            btBit = 29,
            btBSTR = 30,
            btHresult = 31,
            btChar16 = 32,  // char16_t
            btChar32 = 33,  // char32_t
            btChar8 = 34   // char8_t
        };

        enum CV_HREG_e
        {
            CV_REG_EAX = 17,
            CV_REG_ECX = 18,
            CV_REG_EDX = 19,
            CV_REG_EBX = 20,
            CV_REG_ESP = 21,
            CV_REG_EBP = 22,
            CV_REG_ESI = 23,
            CV_REG_EDI = 24,
            CV_REG_EIP = 33,
            CV_ALLREG_VFRAME = 30006,
        }

        private string GetRegisterName(uint reg)
        {
            var register = (CV_HREG_e)reg;
            switch (register)
            {
                case CV_HREG_e.CV_REG_EAX: return "EAX";
                case CV_HREG_e.CV_REG_EBX: return "EBX";
                case CV_HREG_e.CV_REG_ECX: return "ECX";
                case CV_HREG_e.CV_REG_EDX: return "EDX";
                case CV_HREG_e.CV_REG_ESI: return "ESI";
                case CV_HREG_e.CV_REG_EDI: return "EDI";
                case CV_HREG_e.CV_REG_EBP: return "EBP";
                case CV_HREG_e.CV_REG_ESP: return "ESP";
                case CV_HREG_e.CV_REG_EIP: return "EIP";
                case CV_HREG_e.CV_ALLREG_VFRAME: return "VFRAME";
                default:
                    return "UNKNOWN";
            }
        }

        public static string GetDataSymbolType(IDiaSymbol symbol)
        {
            if (symbol == null) return "Unknown";

            IDiaSymbol type = symbol.type;
            if (type == null) return "Unknown";

            return ResolveType(type);
        }

        private static string ResolveType(IDiaSymbol type)
        {
            if (type == null) return "Unknown";

            var symTag = (SymTagEnum)type.symTag;

            // Follow typedefs
            while (symTag == SymTagEnum.SymTagTypedef)
            {
                type = type.type;
                if (type == null) return "Unknown";
            }

            // Pointers
            if (symTag == SymTagEnum.SymTagPointerType)
            {
                IDiaSymbol targetType = type.type;
                string targetName = ResolveType(targetType);
                return targetName + "*";
            }

            // Arrays
            if (symTag == SymTagEnum.SymTagArrayType)
            {
                IDiaSymbol elementType = type.type;
                string elementName = ResolveType(elementType);
                return elementName + "[]";
            }

            // Base types
            if (symTag == SymTagEnum.SymTagBaseType)
            {
                switch (type.baseType)
                {
                    case 0x1: return "void";
                    case 0x2: return "char";
                    case 0x3: return "wchar";
                    case 0x6: return $"int{type.length * 8}_t";
                    case 0x7: return $"uint{type.length * 8}_t";
                    case 0x8: return type.length == 4 ? "float" : type.length == 8 ? "double" : "float(?)";
                    case 0xA: return "bool";
                    case 0xD: return $"int{type.length * 8}_t";
                    case 0xE: return $"uint{type.length * 8}_t";
                    default: return $"Unknown";
                }
            }

            // User-defined types
            if (symTag == SymTagEnum.SymTagUDT)
                return $"struct {type.name}";

            if (symTag == SymTagEnum.SymTagEnum)
                return $"enum {type.name}";

            return type.name ?? "Unknown";
        }



        public bool TryGetSymbolsByRva(uint addr, uint thread, out SymbolInfo[] variables)
        {
            var variableList = new List<SymbolInfo>();

            if (_diaSession == null)
            {
                variables = variableList.ToArray();
                return false;
            }

            _diaSession.findSymbolByRVA(addr, SymTagEnum.SymTagFunction, out var function);
            function.findChildren(SymTagEnum.SymTagNull, null, (uint)NameSearchOptions.nsNone, out var symbols);

            var symbolsEnum = symbols.GetEnumerator();
            while (symbolsEnum.MoveNext())
            {
                var symbol = (IDiaSymbol)symbolsEnum.Current;
                if (SymTagEnum.SymTagData == (SymTagEnum)symbol.symTag)
                {
                    switch ((LocationType)symbol.locationType)
                    {
                        case LocationType.LocIsRegRel:
                            {

                                var symbolInfo = new SymbolInfo { Type = GetDataSymbolType(symbol), Name = symbol.name };
                                variableList.Add(symbolInfo);

                                //var type = GetDataSymbolType(symbol);
                                //Console.WriteLine($"    {symbol.name}: {type} Size: {symbol.type.length}");
                                //Console.WriteLine($"    Register: {GetRegisterName(symbol.registerId)}, Offset: {symbol.offset}");

                                //var contextInfo = launcher.GetContextInfo(addr, thread);
                                //if (contextInfo != null)
                                //{
                                //    //launcher.BaseAddress()
                                //    var memdata = launcher.GetMem((uint)((contextInfo.Ebp + symbol.offset)), (uint)symbol.type.length);
                                //    if (type.Equals("bool"))
                                //    {
                                //        Console.WriteLine($"    Contents: {(memdata[0] == 1 ? "true" : "false")}");
                                //    }
                                //    else if (type.Equals("char*"))
                                //    {
                                //        Console.Write("    Contents: ");
                                //        uint value = BitConverter.ToUInt32(memdata, 0);
                                //        var memdata2 = launcher.GetMem(value, 100);
                                //        foreach (byte b in memdata2)
                                //        {
                                //            if (b == 0)
                                //            {
                                //                break;
                                //            }
                                //            Console.Write((char)b);
                                //        }
                                //        Console.WriteLine();
                                //    }
                                //    else
                                //    {
                                //        uint value = BitConverter.ToUInt32(memdata, 0);
                                //        Console.WriteLine($"    Contents: Ptr(0x{value:x8})");
                                //    }
                                //}
                            }
                            break;
                        case LocationType.LocIsEnregistered:
                            {
                                //var symbolInfo = new SymbolInfo { Type = GetDataSymbolType(symbol), Name = symbol.name };
                                //variableList.Add(symbolInfo);
                                //Console.WriteLine($"    {symbol.name}: {GetDataSymbolType(symbol)} Size: {symbol.type.length}");
                                //Console.WriteLine($"    Register: {GetRegisterName(symbol.registerId)}");
                            }
                            break;
                    }
                }
            }
            variables = variableList.ToArray();
            return true;
 
        }

        //public bool TryGetSymbolsByRva(uint addr, uint thread, Launcher launcher)
        //{
        //    Console.Clear();

        //    if (_diaSession != null)
        //    {
        //        _diaSession.findSymbolByRVA(addr, SymTagEnum.SymTagFunction, out var function);
        //        function.findChildren(SymTagEnum.SymTagNull, null, (uint)NameSearchOptions.nsNone, out var symbols);

        //        var symbolsEnum = symbols.GetEnumerator();
        //        while (symbolsEnum.MoveNext())
        //        {
        //            var symbol = (IDiaSymbol)symbolsEnum.Current;
        //            if (SymTagEnum.SymTagData == (SymTagEnum)symbol.symTag)
        //            {
        //                switch ((LocationType)symbol.locationType)
        //                {
        //                    case LocationType.LocIsRegRel:

        //                        var type = GetDataSymbolType(symbol);
        //                        Console.WriteLine($"    {symbol.name}: {type} Size: {symbol.type.length}");
        //                        Console.WriteLine($"    Register: {GetRegisterName(symbol.registerId)}, Offset: {symbol.offset}");

        //                        var contextInfo = launcher.GetContextInfo(addr, thread);
        //                        if (contextInfo != null)
        //                        {
        //                            //launcher.BaseAddress()
        //                            var memdata = launcher.GetMem((uint)((contextInfo.Ebp + symbol.offset)), (uint)symbol.type.length);
        //                            if (type.Equals("bool"))
        //                            {
        //                                Console.WriteLine($"    Contents: {(memdata[0] == 1 ? "true" : "false")}");
        //                            }
        //                            else if (type.Equals("char*"))
        //                            {
        //                                Console.Write("    Contents: ");
        //                                uint value = BitConverter.ToUInt32(memdata, 0);
        //                                var memdata2 = launcher.GetMem(value, 100);
        //                                foreach (byte b in memdata2)
        //                                {
        //                                    if (b == 0)
        //                                    {
        //                                        break;
        //                                    }
        //                                    Console.Write((char)b);
        //                                }
        //                                Console.WriteLine();
        //                            }
        //                            else
        //                            {
        //                                uint value = BitConverter.ToUInt32(memdata, 0);
        //                                Console.WriteLine($"    Contents: Ptr(0x{value:x8})");
        //                            }
        //                        }
        //                        break;
        //                    case LocationType.LocIsEnregistered:
        //                        Console.WriteLine($"    {symbol.name}: {GetDataSymbolType(symbol)} Size: {symbol.type.length}");
        //                        Console.WriteLine($"    Register: {GetRegisterName(symbol.registerId)}");
        //                        break;
        //                }
        //            }
        //            Console.WriteLine();
        //        }
        //    }
        //    return true;
        //}
    }

}