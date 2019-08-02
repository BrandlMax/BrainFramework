using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using WebSocketSharp;


public class BrainFramework : MonoBehaviour
{

    [Header("Connection")]
    public string socketUrl = "wss://localhost:6868";
    public string license;
    public string clientId;
    public string clientSecret;
    public string headsetId = "EPOCPLUS-3B9AXXXX";


    private WebSocket WS;

    [Header("Settings")]
    public string Profile;
    public bool Training;
    public bool Stream;

    private bool READY = false;
    private bool LoggedIn = false;
    private string TOKEN;
    private string SESSION;

    // Start is called before the first frame update
    void Start()
    {
        Connect();

        // requestAccess
        On("UserLoggedIn", requestAccess);

        // authorize
        On("AccessGranted", authorize);

        // createSession
        On("Authorized", createSession);

        // LoadProfile
        On("SessionCreated", loadProfile);
    }

    // Update is called once per frame
    void Update()
    {

    }


    // ----------- EMOTIVE FUNCTIONS ------------
    // WEBSOCKET
    private void Connect()
    {
        WS = new WebSocket(socketUrl);

        WS.OnOpen += _open;
        WS.OnMessage += _message;
        WS.OnClose += _close;

        WS.ConnectAsync();
    }

    private void _open(object sender, System.EventArgs e)
    {
        Debug.Log("Webserver Connected");

        // getUserLogin
        getUserLogin();
    }

    private void _message(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data.ToString());

        // HANDLE MESSAGES
        // getUserLogin
        if (!LoggedIn)
        {
            RES_LOG_CLASS FirstMsg = JsonUtility.FromJson<RES_LOG_CLASS>(e.Data.ToString());

            Debug.Log(FirstMsg.result[0].username);
            if (FirstMsg.result != null)
            {
                if (FirstMsg.result[0].currentOSUsername == FirstMsg.result[0].loggedInOSUsername)
                {
                    Debug.Log("You are logged in, " + FirstMsg.result[0].username + "!");
                    LoggedIn = true;
                    Emit("UserLoggedIn");
                }
                else
                {
                    Debug.LogError("Someone is apparently already logged in on another device.");
                }
            }
            else
            {
                Debug.LogError("Please login to your Emotiv App and try again.");
            }

        }
        else
        {
            RES_CLASS msg = JsonUtility.FromJson<RES_CLASS>(e.Data.ToString());

            // requestAccess
            if (msg.result.accessGranted)
            {
                if (msg.result.accessGranted)
                {
                    Debug.Log("Access Granted!");
                    Emit("AccessGranted");
                }
                else
                {
                    Debug.LogError("You do not granted access right to this application. Please use Emotiv App to proceed.");
                    Debug.LogError("If you have already approved this app via the Emtiv app, restart this application.");
                }
            }

            // authorize
            if (msg.result.cortexToken != null)
            {
                TOKEN = msg.result.cortexToken;
                Debug.Log("Authorized");
                Emit("Authorized");
            }

            if (msg.result.warning.message != null)
            {
                Debug.LogError("Warning Code " + msg.result.warning.code + ": " + msg.result.warning.message);
            }

            // createSession
            if (msg.result.id != null)
            {
                SESSION = msg.result.id;
                Emit("SessionCreated");
                // activateSession();
            }

            // loadProfile
            if (msg.result.action != null)
            {
                Debug.Log(msg.result.message + " : " + msg.result.name);
                // Now Everything is set and done;
                READY = true;
                Emit("READY");
            }
        }


    }

    private void _close(object sender, CloseEventArgs e)
    {
        Debug.Log("Webserver Closed");
    }


    // REQUESTS

    private void getUserLogin()
    {
        string getUserLoginReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""getUserLogin""
        }";
        WS.Send(getUserLoginReq);
    }

    private void requestAccess()
    {
        string requestAccessReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""requestAccess"",
            ""params"": {
                ""clientId"": """ + clientId + @""",
                ""clientSecret"": """ + clientSecret + @"""
            }
        }";

        WS.Send(requestAccessReq);
    }

    private void authorize()
    {
        string authorizeReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""authorize"",
            ""params"": {
                ""clientId"": """ + clientId + @""",
                ""clientSecret"": """ + clientSecret + @"""
            }
        }";

        WS.Send(authorizeReq);
    }

    private void createSession()
    {
        string createSessionReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""createSession"",
            ""params"": {
                ""cortexToken"": """ + TOKEN + @""",
                ""headset"": """ + headsetId + @""",
                ""status"": ""open""
            }
        }";

        WS.Send(createSessionReq);
    }

    //private void activateSession()
    //{
    //    Debug.Log(TOKEN);
    //    Debug.Log(SESSION);

    //    string activateSessionReq = @"{
    //        ""id"": 1, 
    //        ""jsonrpc"": ""2.0"", 
    //        ""method"": ""updateSession"",
    //        ""params"": {
    //            ""cortexToken"": """ + TOKEN + @""",
    //            ""session"": """ + SESSION + @""",
    //            ""status"": ""active""
    //        }
    //    }";

    //    WS.Send(activateSessionReq);
    //    Debug.Log("ACTIVATEEEEE!!!");
    //}

    private void loadProfile()
    {
        string loadProfileReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""setupProfile"",
            ""params"": {
                ""cortexToken"": """ + TOKEN + @""",
                ""headset"": """ + headsetId + @""",
                ""profile"": """ +  Profile + @""",
                ""status"": ""load""
            }
        }";

        WS.Send(loadProfileReq);
    }

    private void subscribe()
    {
        string createSessionReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""subscribe"",
            ""params"": {
                ""cortexToken"": """ + TOKEN + @""",
                ""session"": """ + SESSION + @""",
                ""streams"": ""[""met"",""dev"",""mot"",""fac"",""com"",""sys""]""
            }
        }";

        WS.Send(createSessionReq);
    }


    // EVENT MANAGER
    public void On(string Event, UnityAction Callback)
    {
        EventManager.On(Event, Callback);
    }


    public void Emit(string Event)
    {
        EventManager.Emit(Event);
    }


}
