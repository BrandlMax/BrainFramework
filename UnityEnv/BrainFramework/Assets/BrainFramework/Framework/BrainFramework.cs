using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using WebSocketSharp;


public class BrainFramework : MonoBehaviour
{
    // ----- Inspector Menu -----
    [Header("Connection")]
    public string socketUrl = "wss://localhost:6868";

    [Header("Emotiv")]
    public string clientId;
    public string clientSecret;
    public string headsetId = "EPOCPLUS-3B9AXXXX";

    [Header("Settings")]
    public string Profile;


    // ----- Private Globals -----
    private WebSocket WS;
    private bool READY = false;
    private bool STREAM = false;
    private bool LoggedIn = false;
    private string TOKEN;
    private string SESSION;
    private string TRAINING;

    public class BRAIN_CLASS
    {
        public string command = null;
        public string eyeAction = null;
        public string upperFaceAction = null;
        public string lowerFaceAction = null;
    }

    public BRAIN_CLASS BRAIN = new BRAIN_CLASS();


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
        On("SessionCreated", () => {
            loadProfile("load");
        });

        // Training
        this.On("trainingStarted", () =>
        {
            Debug.LogWarning($"Training {TRAINING} Started.");
        });

        this.On("trainingSucceeded", () =>
        {
            Debug.LogWarning($"Training {TRAINING} Succeeded.");
            training(TRAINING, "accept");
        });

        this.On("trainingCompleted", () =>
        {
            Debug.LogWarning($"Training {TRAINING} Completed.");
            //SaveProfile();
        });
    }


    // ----------- EMOTIV FUNCTIONS ------------

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
        // Debug.Log(e.Data.ToString());

        // HANDLE MESSAGES

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

            // WEIRD ERROR WITH WEBSOCKET?
            //if (msg.result.warning.message != null)
            //{
            //    Debug.LogError("Warning Code " + msg.result.warning.code + ": " + msg.result.warning.message);
            //}

            // createSession
            if (msg.result.id != null)
            {
                SESSION = msg.result.id;
                Emit("SessionCreated");
                // activateSession();
            }

            // loadProfile
            if (msg.result.action != null && msg.result.name != null)
            {
                if (msg.result.action == "load")
                {
                    Debug.Log(msg.result.message + " : " + msg.result.name);
                    // Now Everything is set and done;
                    READY = true;
                    Emit("READY");
                }
                else
                {
                    Debug.LogWarning("Profile Saved!");
                }

            }

            // STREAM

            if (msg.result.failure != null)
            {
                Debug.Log("Stream Pending");
            }

            // Commands
            if (msg.com != null)
            {
                BRAIN.command = msg.com[0].ToString();
            }

            // FaceActions
            if (msg.fac != null)
            {
                BRAIN.eyeAction = msg.fac[0].ToString();
                BRAIN.upperFaceAction = msg.fac[1].ToString();
                BRAIN.lowerFaceAction = msg.fac[3].ToString();
            }

            // Training
            if (msg.result.action != null && msg.result.status != null)
            {
                if (msg.result.status == "accept")
                {
                    Debug.LogWarning(msg.result.message);
                }
            }
            if (msg.sys != null)
            {
                switch (msg.sys[1])
                {
                    case "MC_Started":
                        Emit("trainingStarted");
                        Debug.LogWarning($"Training Started");
                        break;
                    case "MC_Succeeded":
                        Emit("trainingSucceeded");
                        Debug.LogWarning($"Training Succeeded");
                        break;
                    case "MC_Completed":
                        Emit("trainingCompleted");
                        Debug.LogWarning($"Training Completed");
                        break;
                    default:
                        Debug.LogWarning($"Emotiv System Message: { msg.sys[1] }");
                        break;
                }
            }

            if (msg.fac != null || msg.com != null)
            {
                Emit("STREAM");
            }
        }


    }

    private void _close(object sender, CloseEventArgs e)
    {
        Debug.Log("Webserver Closed:" +  e.Reason);
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

    private void loadProfile(string action)
    {
        string loadProfileReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""setupProfile"",
            ""params"": {
                ""cortexToken"": """ + TOKEN + @""",
                ""headset"": """ + headsetId + @""",
                ""profile"": """ +  Profile + @""",
                ""status"": """ + action + @"""
            }
        }";

        // Debug.LogError(action);

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
                ""streams"": [""com"",""fac"",""sys""]
            }
        }";

        WS.Send(createSessionReq);
        STREAM = true;
    }

    private void training(string action, string status)
    {
        string createSessionReq = @"{
            ""id"": 1, 
            ""jsonrpc"": ""2.0"", 
            ""method"": ""training"",
            ""params"": {
                ""action"": """ + action + @""",
                ""cortexToken"": """ + TOKEN + @""",
                ""detection"": ""mentalCommand"",
                ""session"": """ + SESSION + @""",
                ""status"": """ + status + @"""
            }
        }";

        WS.Send(createSessionReq);
        STREAM = true;
    }

    // SHORTCUTS
    public void StartStream()
    {
        if (!STREAM)
        {
            subscribe();
        }
    }

    public void StartTraining(string action)
    {
        TRAINING = action;
        training(action, "start");
    }

    public void SaveProfile()
    {
        loadProfile("save");
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
