using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MultipleDeviceSearch : MonoBehaviour {

    // scan filter
    public bool speedCadence;
    public bool cadence;
    public bool hr;
    public bool speed;
    public bool power;
    public bool fec;
    public bool podometer;

    public List<int> foundSpeedCadenceIDList;
    public List<int> foundCadenceIDList;
    public List<int> foundHrIDList;
    public List<int> foundSpeedIDList;
    public List<int> foundPowerIDList;
    public List<int> foundFecIDList;

    public Text debugText;
    public SpeedCadenceDisplay spcadDisplay;
    // Use this for initialization

    public void Start() {
        StartScan();
    }
    public void StartScan () {
        //in order to start a multipleDevice scan on Android
        // call start_multiple_device_search, gameobject, bool speedCadence, bool hr, bool cadence, bool speed, bool power, bool fec  as search filters

        foundSpeedCadenceIDList = new List<int>();
        foundCadenceIDList = new List<int>();
        foundHrIDList = new List<int>();
        foundSpeedIDList = new List<int>();
        foundPowerIDList = new List<int>();
        foundFecIDList = new List<int>();


#if UNITY_ANDROID && !UNITY_EDITOR
        
           Debug.Log("starting android multi device scan");
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("start_multiple_device_search",this.gameObject.name,speedCadence,hr,cadence,speed,power,fec);
            }
        }
#endif


    }

    void ANTPLUG_foundSpeedCadence(string s) {
        foundSpeedCadenceIDList.Add(int.Parse(s));
        Debug.Log("scan found s&c device " + s);
        debugText.text += s+"-";

        //found the speedCadence, set the ID and look for it
        spcadDisplay.deviceID = int.Parse(s);
        spcadDisplay.StartScan();
    }
    void ANTPLUG_foundCadence(string s) {
        foundCadenceIDList.Add(int.Parse(s));
        Debug.Log("scan found cadence device " + s);
        debugText.text += s + "-";
     

    }
    void ANTPLUG_foundHr(string s) {
        foundHrIDList.Add(int.Parse(s));
        Debug.Log("scan found device hr " + s);
        debugText.text += s + "-";
    }
    void ANTPLUG_foundSpeed(string s) {
        foundSpeedIDList.Add(int.Parse(s));
        Debug.Log("scan found speed device " + s);
        debugText.text += s + "-";
    }
    void ANTPLUG_foundPower(string s) {
        foundPowerIDList.Add(int.Parse(s));
        Debug.Log("scan found power device " + s);
        debugText.text += s;
      


    }
    void ANTPLUG_foundFec(string s) {
        foundFecIDList.Add(int.Parse(s));
        Debug.Log("scan found trainer device " + s);
        debugText.text += s + "-";
    }
   
  
}
