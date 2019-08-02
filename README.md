# BrainFramework
Unity Plugin for the Emotiv EPOC v2 API

## Setup with Emotiv
1. Create an App to get clientId and clientSecret:
https://emotiv.gitbook.io/cortex-api/#create-a-cortex-app
![Create](https://raw.githubusercontent.com/BrandlMax/BrainFramework/master/_readme/CreateID.png)
2. Login and connect your Headset with the EMOTIV App.
![Connect](https://raw.githubusercontent.com/BrandlMax/BrainFramework/master/_readme/Connect.png)

## Setup in Unity
1. Install Package (export/BrainFramework.unitypackage) to your Unity Scene
2. Add Prefab "BrainFramework" to your Scene or use Example Scene
3. Fill in the settings in the BrainFramework Inspector <br />
![Inspector](https://raw.githubusercontent.com/BrandlMax/BrainFramework/master/_readme/Inpsector.png)
4. On the first run you should get a notification inside the EMOTIV App. <br /> Approve your App and restart your Unity script.

![Accept](https://raw.githubusercontent.com/BrandlMax/BrainFramework/master/_readme/Accept.png)

## A simple example to control a ball

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameObject BrainFramework;
    private BrainFramework EPOC;

    private Rigidbody Ball;

    void Start()
    {

        // ---------- BRAIN FRAMEWORK-----------
        // 1. Connect to EPOC Script
        EPOC = BrainFramework.GetComponent<BrainFramework>();

        // 2. Subscribe to Events
        EPOC.On("READY", Ready);
        EPOC.On("STREAM", Stream);
        // ------------------------------------

        // Prepare Ball
        Ball = GetComponent<Rigidbody>();
    }

    // EPOC IS READY
    void Ready()
    {
        Debug.Log("EPOC Ready!");


        // START STREAM
        EPOC.StartStream();


        // ------ TRAINING -------
        // It is best to place this command on a key, 
        // but make sure to call it after the "READY" event:

        // EPOC.StartTraining("neutral");

        // The next 8 seconds will then be recorded and saved into the profile

        // These are the parameters you could train:
        //"neutral"
        //"push"
        //"pull"
        //"lift"
        //"drop"
        //"left"
        //"right"
        //"rotateLeft"
        //"rotateRight"
        //"rotateClockwise"
        //"rotateCounterClockwise"
        //"rotateForwards"
        //"rotateReverse"
        //"disappear"

        // You could also listen to this events:
        // trainingStarted
        // trainingSucceeded
        // trainingCompleted

        // At the end of a training session you can save your progress to the profile
        // Call this after an "trainingCompleted" event or manually. 
        // EPOC.SaveProfile();

    }

    // DATA STREAM
    void Stream()
    {
        Debug.Log($"command: { EPOC.BRAIN.command } | eyeAction: { EPOC.BRAIN.eyeAction } | upperFaceAction: { EPOC.BRAIN.upperFaceAction } | lowerFaceAction: { EPOC.BRAIN.lowerFaceAction }");
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // BALL MOVEMENT EXAMPLE
        Vector3 movement = new Vector3(0.0f, 0.0f, 0.0f);

        if (EPOC.BRAIN.command == "push")
        {
            movement = new Vector3(0.0f, 0.0f, 1.0f);
        }
        if (EPOC.BRAIN.command == "pull")
        {
            movement = new Vector3(0.0f, 0.0f, -1.0f);
        }
        if (EPOC.BRAIN.command == "left")
        {
            movement = new Vector3(-1.0f, 0.0f, 0.0f);
        }
        if (EPOC.BRAIN.command == "right")
        {
            movement = new Vector3(1.0f, 0.0f, 0.0f);
        }


        Ball.AddForce(movement);
    }
}
```
