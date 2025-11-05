using Dia2Lib;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RXDKXBDM
{
    public class StructMember
    {
        public string Name { get; set; } = "";
        public DetailedTypeInfo Type { get; set; } = new();
        public uint Offset { get; set; } = 0;
        public uint Size { get; set; } = 0;
    }

    public class DetailedTypeInfo
    {
        public string TypeName { get; set; } = "Unknown";
        public uint Size { get; set; } = 0;
        public bool IsPointer { get; set; } = false;
        public bool IsArray { get; set; } = false;
        public bool IsStruct { get; set; } = false;
        public bool IsEnum { get; set; } = false;
        public bool IsBasicType { get; set; } = false;
        public uint ElementSize { get; set; } = 0;  // For arrays - size of each element
        public uint ElementCount { get; set; } = 0; // For arrays - number of elements
        public DetailedTypeInfo? BaseType { get; set; } = null; // For pointers and arrays - what they point to/contain
        public List<StructMember> StructMembers { get; set; } = new(); // For structs - list of members
        public bool IsValid { get; set; } = true; // False if type couldn't be resolved

        public override string ToString()
        {
            if (!IsValid) return "Invalid Type";
            
            var result = TypeName;
            if (Size > 0) result += $" (size: {Size})";
            if (IsArray && ElementCount > 0) result += $" [count: {ElementCount}]";
            if (IsPointer && BaseType != null) result += $" -> {BaseType.TypeName}";
            if (IsStruct && StructMembers.Count > 0) result += $" [{StructMembers.Count} members]";
            
            return result;
        }
    }

    public class SymbolInfo
    {
        public required DetailedTypeInfo? Type;
        public required string Name;
        public required string Register;
        public required long Offset;
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

        public static DetailedTypeInfo GetDataSymbolType(IDiaSymbol symbol)
        {
            if (symbol == null) 
                return new DetailedTypeInfo { TypeName = "Unknown", IsValid = false };

            IDiaSymbol type = symbol.type;
            if (type == null) 
                return new DetailedTypeInfo { TypeName = "Unknown", IsValid = false };

            return ResolveType(type);
        }

        private static DetailedTypeInfo ResolveType(IDiaSymbol type)
        {
            if (type == null) 
                return new DetailedTypeInfo { TypeName = "Unknown", IsValid = false };

            var symTag = (SymTagEnum)type.symTag;

            // Follow typedefs
            while (symTag == SymTagEnum.SymTagTypedef)
            {
                type = type.type;
                if (type == null) 
                    return new DetailedTypeInfo { TypeName = "Unknown", IsValid = false };
                symTag = (SymTagEnum)type.symTag;
            }

            var result = new DetailedTypeInfo();

            // Pointers
            if (symTag == SymTagEnum.SymTagPointerType)
            {
                result.IsPointer = true;
                result.Size = (uint)type.length; // Pointer size (usually 4 or 8 bytes)
                
                IDiaSymbol targetType = type.type;
                if (targetType != null)
                {
                    result.BaseType = ResolveType(targetType);
                    result.TypeName = result.BaseType.TypeName + "*";
                }
                else
                {
                    result.TypeName = "void*";
                }
                return result;
            }

            // Arrays
            if (symTag == SymTagEnum.SymTagArrayType)
            {
                result.IsArray = true;
                result.Size = (uint)type.length; // Total array size
                
                IDiaSymbol elementType = type.type;
                if (elementType != null)
                {
                    result.BaseType = ResolveType(elementType);
                    result.ElementSize = result.BaseType.Size;
                    result.ElementCount = result.Size / result.ElementSize;
                    result.TypeName = result.BaseType.TypeName + "[]";
                }
                else
                {
                    result.TypeName = "Unknown[]";
                    result.IsValid = false;
                }
                return result;
            }

            // Base types
            if (symTag == SymTagEnum.SymTagBaseType)
            {
                result.IsBasicType = true;
                result.Size = (uint)type.length;
                
                switch (type.baseType)
                {
                    case 0x1: 
                        result.TypeName = "void";
                        result.Size = 0; // void has no size
                        break;
                    case 0x2: 
                        result.TypeName = "char";
                        break;
                    case 0x3: 
                        result.TypeName = "wchar";
                        break;
                    case 0x6: 
                        result.TypeName = $"int{type.length * 8}";
                        break;
                    case 0x7: 
                        result.TypeName = $"uint{type.length * 8}";
                        break;
                    case 0x8: 
                        result.TypeName = type.length == 4 ? "float" : type.length == 8 ? "double" : "float(?)";
                        break;
                    case 0xA: 
                        result.TypeName = "bool";
                        break;
                    case 0xD: 
                        result.TypeName = $"int{type.length * 8}";
                        break;
                    case 0xE: 
                        result.TypeName = $"uint{type.length * 8}";
                        break;
                    default: 
                        result.TypeName = "Unknown";
                        result.IsValid = false;
                        break;
                }
                return result;
            }

            // User-defined types (structs, classes)
            if (symTag == SymTagEnum.SymTagUDT)
            {
                result.IsStruct = true;
                result.Size = (uint)type.length;
                result.TypeName = $"struct {type.name ?? "Unknown"}";
                result.StructMembers = GetStructMembers(type);
                return result;
            }

            // Enums
            if (symTag == SymTagEnum.SymTagEnum)
            {
                result.IsEnum = true;
                result.Size = (uint)type.length;
                result.TypeName = $"enum {type.name ?? "Unknown"}";
                return result;
            }

            // Fallback for other types
            result.TypeName = type.name ?? "Unknown";
            result.Size = (uint)type.length;
            if (string.IsNullOrEmpty(type.name))
            {
                result.IsValid = false;
            }
            
            return result;
        }

        private static List<StructMember> GetStructMembers(IDiaSymbol structSymbol)
        {
            var members = new List<StructMember>();
            
            try
            {
                structSymbol.findChildren(SymTagEnum.SymTagData, null, 0, out var childSymbols);
                if (childSymbols == null) return members;

                var childEnum = childSymbols.GetEnumerator();
                while (childEnum.MoveNext())
                {
                    var memberSymbol = (IDiaSymbol)childEnum.Current;
                    
                    var member = new StructMember
                    {
                        Name = memberSymbol.name ?? "Unknown",
                        Offset = (uint)memberSymbol.offset,
                        Type = ResolveType(memberSymbol.type),
                        Size = (uint)(memberSymbol.type?.length ?? 0)
                    };
                    
                    members.Add(member);
                }
            }
            catch (Exception)
            {
                // If we can't get struct members, return empty list
                // This allows the struct to still be displayed with basic info
            }

            return members;
        }

        public bool TryGetSymbolsByRva(uint addr, out SymbolInfo[] variables)
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
                                var symbolInfo = new SymbolInfo { Type = GetDataSymbolType(symbol), Name = symbol.name, Register = GetRegisterName(symbol.registerId), Offset = symbol.offset };
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
                                int q = 1;
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