using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Screenshotter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)){
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            UnityEngine.ScreenCapture.CaptureScreenshot(Path.Combine(Application.persistentDataPath, ((long)timeSpan.TotalSeconds)+".png"), 1);
        }
    }
}
