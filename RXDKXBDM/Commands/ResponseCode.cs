﻿namespace RXDKXBDM.Commands
{
    public enum ResponseCode
    {
        SUCCESS_OK = 200,
        SUCCESS_CONNECTED = 201,
        SUCCESS_MULTIRESPONSE = 202,
        SUCCESS_BINRESPONSE = 203,
        SUCCESS_READYFORBIN = 204,
        SUCCESS_DEDICATED = 205,
        SUCCESS_CANCELLED = 206,

        ERROR_UNDEFINED = 400,
        ERROR_MAXCONNECT = 401,
        ERROR_NOSUCHFILE = 402,
        ERROR_NOMODULE = 403,
        ERROR_MEMUNMAPPED = 404,
        ERROR_NOTHREAD = 405,
        ERROR_CLOCKNOTSET = 406,
        ERROR_INVALIDCMD = 407,
        ERROR_NOTSTOPPED = 408,
        ERROR_MUSTCOPY = 409,
        ERROR_ALREADYEXISTS = 410,
        ERROR_DIRNOTEMPTY = 411,
        ERROR_BADFILENAME = 412,
        ERROR_CANNOTCREATE = 413,
        ERROR_CANNOTACCESS = 414,
        ERROR_DEVICEFULL = 415,
        ERROR_NOTDEBUGGABLE = 416,
        ERROR_BADCOUNTTYPE = 417,
        ERROR_COUNTUNAVAILABLE = 418,
        ERROR_NOTLOCKED = 420,
        ERROR_KEYXCHG = 421,
        ERROR_MUSTBEDEDICATED = 422,
        ERROR_CANNOTCONNECT = 425,
        ERROR_CONNECTIONLOST = 426,
        ERROR_FILEERROR = 428,
        ERROR_ENDOFLIST = 429,
        ERROR_BUFFER_TOO_SMALL = 430,
        ERROR_NOTXBEFILE = 431,
        ERROR_MEMSETINCOMPLETE = 432,
        ERROR_NOXBOXNAME = 433,
        ERROR_NOERRORSTRING = 434,

        ERROR_INTERNAL_ERROR = 500
    }
}
