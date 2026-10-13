using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ANT_Managed_Library;
using System;

public class CadenceDisplay : MonoBehaviour {

    public bool autoStartScan = true; //start scan on play
    public bool connected = false; //will be set to true once connected
    public float wheelCircumference = 2.096f; //700*23C, set this to your wheels size

    //windows and mac settings
    public bool autoConnectToFirstSensorFound = true; //for windows and mac, either connect to the first sensor found or let you pick a sensor manually in the scanResult list with your own UI and call ConnectToDevice(AntDevice device)
    public List<AntDevice> scanResult;

    //android settings
    public bool useAndroidUI = true; //will open the unified ant+ UI on the android app if set to true, otherwise will connect to the first found device
    public bool skipPreferredSearch = true;  //- True = Don't automatically connect to user's preferred device, but always go to search for other devices.

    //the sensor values we receive fron the onReceiveData event
    public int cadence; // The cadence in rpm

    private AntChannel backgroundScanChannel;
    private AntChannel deviceChannel;


    private int stopRevCounter_cadence = 0;
    private int prev_measTime_cadence = 0;
    private int prev_revCount_cadence = 0;
    public int deviceID = 0; //set this to connect to a specific device ID

    void Start() {

        if (autoStartScan)
            StartScan();

    }

    //Start a background Scan to find the device
    public void StartScan() {

        Debug.Log("Looking for ANT + Cadence sensor");
#if UNITY_ANDROID && !UNITY_EDITOR
        
        //Java: connect_cadence(String gameobjectName, float wheel, boolean useAndroidUI, boolean skipPreferredSearch, int deviceID, boolean speedCadence ) {

        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("connect_cadence", this.gameObject.name, wheelCircumference, useAndroidUI,skipPreferredSearch,deviceID, false);
            }
        }
#else


        AntManager.Instance.Init();
        scanResult = new List<AntDevice>();
        backgroundScanChannel = AntManager.Instance.OpenBackgroundScanChannel(0);
        backgroundScanChannel.onReceiveData += ReceivedBackgroundScanData;
#endif

    }

    //Android function
    void ANTPLUG_ConnectEvent(string resultCode) {
        switch (resultCode) {
            case "SUCCESS":
                connected = true;
                break;
            case "CHANNEL_NOT_AVAILABLE":
                //Channel Not Available

                break;
            case "ADAPTER_NOT_DETECTED":
                //ANT Adapter Not Available. Built-in ANT hardware or external adapter required.
                Debug.Log("ANT Adapter Not Available. Built-in ANT hardware or external adapter required.");
                break;
            case "BAD_PARAMS":

                //Bad request parameters.

                break;
            case "OTHER_FAILUR":

                //RequestAccess failed. See logcat for details.

                break;
            case "DEPENDENCY_NOT_INSTALLED":
            //You need to install the ANT+ Plugins service or you may need to update your existing version if you already have it. 

            case "USER_CANCELLED":
                //USER_CANCELLED
                break;
            case "UNRECOGNIZED":
                //UNRECOGNIZED. PluginLib Upgrade Required?",

                break;
            default:
                //UNRECOGNIZED
                break;
        }
    }

    void ANTPLUG_StateChange(string newDeviceState) {
        switch (newDeviceState) {
            case "DEAD":
                connected = false;
                break;
            case "CLOSED":

                break;
            case "SEARCHING":
                //searching
                break;
            case "TRACKING":
                //tracking
                break;
            case "PROCESSING_REQUEST":

                break;
            default:
                //UNRECOGNIZED
                break;
        }
    }


    void ANTPLUG_Receive_calculatedCadence(string s) {
        cadence = (int)float.Parse(s);
    }

    //Windows and mac 
    //If the device is found
    void ReceivedBackgroundScanData(Byte[] data) {

        byte deviceType = (data[12]); // extended info Device Type byte

        switch (deviceType) {

            case AntplusDeviceType.BikeCadence: {
                    int deviceNumber = (data[10]) | data[11] << 8;
                    byte transType = data[13];
                    foreach (AntDevice d in scanResult) {
                        if (d.deviceNumber == deviceNumber && d.transType == transType) //device already found
                            return;
                    }

                    Debug.Log("Cadence sensor found " + deviceNumber);

                    AntDevice foundDevice = new AntDevice();
                    foundDevice.deviceType = deviceType;
                    foundDevice.deviceNumber = deviceNumber;
                    foundDevice.transType = transType;
                    foundDevice.period = 8102;
                    foundDevice.radiofreq = 57;
                    foundDevice.name = "BikeCadence(" + foundDevice.deviceNumber + ")";
                    scanResult.Add(foundDevice);
                    if (autoConnectToFirstSensorFound) {
                        ConnectToDevice(foundDevice);
                    }
                    break;
                }

            default: {

                    break;
                }
        }

    }

    void ConnectToDevice(AntDevice device) {
        AntManager.Instance.CloseBackgroundScanChannel();
        byte channelID = AntManager.Instance.GetFreeChannelID();
        deviceChannel = AntManager.Instance.OpenChannel(ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00, channelID, (ushort)device.deviceNumber, device.deviceType, device.transType, (byte)device.radiofreq, (ushort)device.period, false);
        connected = true;
        deviceChannel.onReceiveData += Data;
        deviceChannel.onChannelResponse += ChannelResponse;

        deviceChannel.hideRXFAIL = true;
    }


    //Deal with the received Data
    public void Data(Byte[] data) {


        //CADENCE
        int measTime_cadence = (data[4]) | data[5] << 8;
        int revCount_cadence = (data[6]) | data[7] << 8;

        if (prev_measTime_cadence != 0 && measTime_cadence != prev_measTime_cadence && prev_measTime_cadence < measTime_cadence && prev_revCount_cadence < revCount_cadence) {
            cadence = (60 * (revCount_cadence - prev_revCount_cadence) * 1024) / (measTime_cadence - prev_measTime_cadence);
            stopRevCounter_cadence = 0;

        } else
            stopRevCounter_cadence++;

        if (stopRevCounter_cadence >= 5) {
            stopRevCounter_cadence = 5;
            cadence = 0;
        }


        prev_measTime_cadence = measTime_cadence;
        prev_revCount_cadence = revCount_cadence;




    }



    void ChannelResponse(ANT_Response response) {


    }

}
