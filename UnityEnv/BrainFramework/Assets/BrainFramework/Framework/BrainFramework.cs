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

    private WebSocket WS;

    [Header("Settings")]
    public string Profile;
    public bool Training;
    public bool Subscribed;

    private bool LoggedIn = false;

    // Start is called before the first frame update
    void Start()
    {
        Connect();

        // requestAccess
        On("UserLoggedIn", requestAccess);

        // authorize
        // On("AccessGranted", authorize);

        // createSession with Profile

        // subscribe
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
        Debug.Log("Got Message");
        // HANDLE MESSAGES
        // getUserLogin
        if (!LoggedIn)
        {
            RES_LOG_CLASS msg = JsonUtility.FromJson<RES_LOG_CLASS>(e.Data.ToString());

            Debug.Log(msg.result[0].username);
            if (msg.result != null)
            {
                if (msg.result[0].currentOSUsername == msg.result[0].loggedInOSUsername)
                {
                    Debug.Log("You are logged in, " + msg.result[0].username + "!");
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
                Debug.Log("Access Granted!");
            }
            else
            {
                Debug.LogError("You do not granted access right to this application. Please use Emotiv App to proceed.");
                Debug.LogError("If you have already approved this app via the Emtiv app, restart this application.");
            }

            // authorize
            //if (msg.result.cortexToken != null)
            //{

            //}

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
        Debug.Log("START REQUEST");

        string requestAccessReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""requestAccess"",
            ""params"": {
                ""clientId"": """ + clientId + @""",
                ""clientSecret"": """ + clientSecret + @"""
            }
        }";

        Debug.Log(requestAccessReq);

        WS.Send(requestAccessReq);
    }

    private void authorize()
    {

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
