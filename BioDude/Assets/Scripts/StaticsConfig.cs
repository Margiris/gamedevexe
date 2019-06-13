using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticsConfig
{
    public const int MainMenuIdx = 0;
    public const int LobbyIdx = 1;
    public const int SingleLvl1 = 2;

#if !UNITY_WEBGL
    public const bool IsServer = true;
#else
    public const bool IsServer = false;
#endif

    public const string SERVER_IP = "127.0.0.1";
//    public const string SERVER_IP = "localhost";
    public const int SERVER_PORT = 8000;
    public const int PORT_OFFSET = 0;

    public const int RESPONSE_TIMEOUT = 1000;

    /* error values:
        0 - port not occupied and server not running on that port
        1 - port in use by BioDude server
        2 - port in use by another process
    */
    public static readonly string[] PortErrorMessages =
    {
        "",
        "",
        "Game ID unavailable",
        "Unknown Error"
    };
}