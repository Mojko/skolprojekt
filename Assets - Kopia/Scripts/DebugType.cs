using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DebugType {

    public static void logNetworked(string s)
    {
        string stringType = "";
        if(NetworkClient.active) stringType = "CLIENT";
        if(NetworkServer.active) stringType = "SERVER";
        if(NetworkServer.active && NetworkClient.active) stringType = "HOST";
        Debug.Log(stringType+": " + s);
    }
}
