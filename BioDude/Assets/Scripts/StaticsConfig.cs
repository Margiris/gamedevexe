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

    public const int PORT_OFFSET = 0;

    public static readonly string[] PortErrorMessages =
    {
        "",
        "Selected game ID unavailable",
        "Unknown Error"
    };
}