using Dia2Lib;
using RXDKXBDM.Commands;
using SharpPdb.Windows;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using static SharpPdb.Windows.DebugSubsections.LinesSubsection;

[SupportedOSPlatform("windows")]
public class PdbParser : IDisposable
{
    IDiaDataSource? _diaSource;
    IDiaSession? _diaSession;

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

    public void Dispose()
    {
        if (_diaSession != null)
        {
            Marshal.ReleaseComObject(_diaSession);
            _diaSession = null;
        }

        if (_diaSource != null)
        {
            Marshal.ReleaseComObject(_diaSource);
            _diaSource = null;
        }
    }

    public void LoadPdb(string pdbPath)
    {
        _diaSource = new DiaSource();
        _diaSource.loadDataFromPdb(pdbPath);
        _diaSource.openSession(out _diaSession);
    }

    public void ClosePdb()
    {
        Dispose();
    }

    public bool TryGetRvaByFileLine(string filename, uint line, uint column, out uint rva)
    {
        if (_diaSession != null)
        {
            _diaSession.findFile(null, filename, (uint)(NameSearchOptions.nsfCaseInsensitive | NameSearchOptions.nsfFNameExt), out var files);
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
            _diaSession.findLinesByRVA(rva, 1, out IDiaEnumLineNumbers lines);
            var lineEnum = lines.GetEnumerator();
            while (lineEnum.MoveNext())
            {
                var lineItem = (IDiaLineNumber)lineEnum.Current;
                line = lineItem.lineNumber;
                column = lineItem.columnNumber;
                file = lineItem.sourceFile.fileName;
                return true;
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
                case 0x6: return $"int{type.length*8}_t";
                case 0x7: return $"uint{type.length*8}_t";
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

    public bool TryGetSymbolsByRva(uint rva)
    {
        Console.Clear();

        if (_diaSession != null)
        {
            _diaSession.findSymbolByRVA(rva, SymTagEnum.SymTagFunction, out var function);
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
                            Console.WriteLine($"    {symbol.name}: {GetDataSymbolType(symbol)} Size: {symbol.type.length}");
                            Console.WriteLine($"    Register: {GetRegisterName(symbol.registerId)}, Offset: {symbol.offset}");
                            break;
                        case LocationType.LocIsEnregistered:
                            Console.WriteLine($"    {symbol.name}: {GetDataSymbolType(symbol)} Size: {symbol.type.length}");
                            Console.WriteLine($"    Register: {GetRegisterName(symbol.registerId)}");
                            break;
                    }
                }
                Console.WriteLine();
            }
        }
        return true;
    }
}