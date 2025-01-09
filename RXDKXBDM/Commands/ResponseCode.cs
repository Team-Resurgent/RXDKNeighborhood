﻿namespace RXDKXBDM.Commands
{
    public enum ResponseCode
    {
        XBDM_SUCCESS_OK = 200,
        XBDM_SUCCESS_CONNECTED = 201,
        XBDM_SUCCESS_MULTIRESPONSE = 202,
        XBDM_SUCCESS_BINRESPONSE = 203,
        XBDM_SUCCESS_READYFORBIN = 204,
        XBDM_SUCCESS_DEDICATED = 205,

        XBDM_ERROR_UNDEFINED = 400,
        XBDM_ERROR_MAXCONNECT = 401,
        XBDM_ERROR_NOSUCHFILE = 402,
        XBDM_ERROR_NOMODULE = 403,
        XBDM_ERROR_MEMUNMAPPED = 404,
        XBDM_ERROR_NOTHREAD = 405,
        XBDM_ERROR_CLOCKNOTSET = 406,
        XBDM_ERROR_INVALIDCMD = 407,
        XBDM_ERROR_NOTSTOPPED = 408,
        XBDM_ERROR_MUSTCOPY = 409,
        XBDM_ERROR_ALREADYEXISTS = 410,
        XBDM_ERROR_DIRNOTEMPTY = 411,
        XBDM_ERROR_BADFILENAME = 412,
        XBDM_ERROR_CANNOTCREATE = 413,
        XBDM_ERROR_CANNOTACCESS = 414,
        XBDM_ERROR_DEVICEFULL = 415,
        XBDM_ERROR_NOTDEBUGGABLE = 416,
        XBDM_ERROR_BADCOUNTTYPE = 417,
        XBDM_ERROR_COUNTUNAVAILABLE = 418,
        XBDM_ERROR_NOTLOCKED = 420,
        XBDM_ERROR_KEYXCHG = 421,
        XBDM_ERROR_MUSTBEDEDICATED = 422,
        XBDM_ERROR_CANNOTCONNECT = 425,
        XBDM_ERROR_CONNECTIONLOST = 426,
        XBDM_ERROR_FILEERROR = 428,
        XBDM_ERROR_ENDOFLIST = 429,
        XBDM_ERROR_BUFFER_TOO_SMALL = 430,
        XBDM_ERROR_NOTXBEFILE = 431,
        XBDM_ERROR_MEMSETINCOMPLETE = 432,
        XBDM_ERROR_NOXBOXNAME = 433,
        XBDM_ERROR_NOERRORSTRING = 434,

        ParseResponseInvalidLength = 600,
        ParseResponseInvalidCode = 601,
        TrySendStringFailed = 602,
        TryRecieveString = 603,

        ClientNotOpen,
        Timeout,
        UnexpectedResult,
        SocketError,
        BadArgument
    }
   
}
