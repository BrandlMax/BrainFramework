using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RES_LOG_CLASS
{
    public string jsonrpc;
    public RESULT_CLASS[] result;
    public RESULT_CLASS undefClass;
    public string undef;
    public string[] com;
    public string[] fac;
    public string[] sys;
    public string sid;
    public string time;
}

public class RES_CLASS
{
    public string jsonrpc;
    public RESULT_CLASS result;
    public RESULT_CLASS undefClass;
    public string undef;
    public string[] com;
    public string[] fac;
    public string[] sys;
    public string sid;
    public string time;
}

[System.Serializable]
public class RESULT_CLASS
{
    public string _auth;
    public string appId;
    public string id;
    public string username;
    public string currentOSUsername;
    public string loggedInOSUsername;
    public bool accessGranted;
    public string message;
    public string cortexToken;
    public string action;
    public string name;
    public string status;
    public string[] failure;
    public WARNING_CLASS warning;
}

[System.Serializable]
public class WARNING_CLASS
{
    public string code;
    public string message;
    public string licenseUrl;
}

