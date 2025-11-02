using Dia2Lib;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if WINDOWS_ONLY

[SupportedOSPlatform("windows")]
public class PdbHelper : IDisposable
{
    IDiaDataSource? _diaSource;
    IDiaSession? _diaSession;

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
                    Marshal.ReleaseComObject(sourceFile);
                }
                if (lineItem != null)
                {
                    Marshal.ReleaseComObject(lineItem);
                }
                if (lines != null)
                {
                    Marshal.ReleaseComObject(lines);
                }
            }
        }
        file = string.Empty;
        line = 0;
        column = 0;
        return false;
    }
}

#endif