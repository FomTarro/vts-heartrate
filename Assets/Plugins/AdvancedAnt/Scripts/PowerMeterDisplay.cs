using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ANT_Managed_Library;
using System;
/*

    Frequencies such as 4.06 Hz (ANT+ Heart
Rate) and 4.005 Hz (ANT+ Bike Power) will
periodically “drift” into each other, or into
other channel periods that may be present
in the vicinity. During this overlap, channel
collisions may occur as the radio can only
service channel at a time. 

    */
public class PowerMeterDisplay : MonoBehaviour {

    public bool autoStartScan = true; //start scan on play
    public bool connected = false; //will be set to true once connected
 
    //windows and mac settings
    public bool autoConnectToFirstSensorFound = true; //for windows and mac, either connect to the first sensor found or let you pick a sensor manually in the scanResult list with your own UI and call ConnectToDevice(AntDevice device)
    public List<AntDevice> scanResult;

    //android settings
    public bool useAndroidUI = true; //will open the unified ant+ UI on the android app if set to true, otherwise will connect to the first found device
    public bool skipPreferredSearch = true;  //- True = Don't automatically connect to user's preferred device, but always go to search for other devices.

    //the sensor values we receive fron the onReceiveData event
    public int instantaneousPower; // the instantaneous power in watt
    public int instantaneousCadence; // crank cadence in RPM if available ( 255 indicates invalid)

    private AntChannel backgroundScanChannel;
    private AntChannel deviceChannel;

    public int deviceID = 0; //set this to connect to a specific device ID

    private byte[] pageToSend;

    void Start() {

        if (autoStartScan)
            StartScan();

    }

    //Start a background Scan to find the device
    public void StartScan() {


        Debug.Log("Looking for ANT + power meter sensor");
#if UNITY_ANDROID && !UNITY_EDITOR
        //java: connect_power(String gameobjectName, boolean useAndroidUI, boolean skipPreferredSearch, int deviceID)

        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("connect_power", this.gameObject.name, useAndroidUI,skipPreferredSearch,deviceID);
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


  
    void ANTPLUG_Receive_calculatedPower(string s) {
        instantaneousPower = (int)float.Parse(s);
    }
    void ANTPLUG_Receive_calculatedCadence(string s) {
        instantaneousCadence = (int)float.Parse(s);
    }



    //Windows and mac 
    //If the device is found
    void ReceivedBackgroundScanData(Byte[] data) {

        byte deviceType = (data[12]); // extended info Device Type byte

        switch (deviceType) {

            case AntplusDeviceType.BikePower: {
                    int deviceNumber = (data[10]) | data[11] << 8;
                    byte transType = data[13];
                    foreach (AntDevice d in scanResult) {
                        if (d.deviceNumber == deviceNumber && d.transType == transType) //device already found
                            return;
                    }

                    Debug.Log("Powermeter sensor found " + deviceNumber);

                    AntDevice foundDevice = new AntDevice();
                    foundDevice.deviceType = deviceType;
                    foundDevice.deviceNumber = deviceNumber;
                    foundDevice.transType = transType;
                    foundDevice.period = 8182;
                    foundDevice.radiofreq = 57;
                    foundDevice.name = "Powermeter(" + foundDevice.deviceNumber + ")";
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

    int update_event_count = 0;
    int sameEventCounter = 0;
    //Deal with the received Data
    public void Data(Byte[] data) {

        if (data[0] == 0x10) {

            if (data[1] == update_event_count)
                sameEventCounter++;
            else
                sameEventCounter = 0;

            update_event_count = data[1];

            if (sameEventCounter > 3) {
                instantaneousPower = 0;
                instantaneousCadence = 0;
            } else {
                instantaneousPower = (data[6]) | data[7] << 8;
                instantaneousCadence = data[3];
            }

        }
    }


    public void Calibrate() {

        Debug.Log("Sending : Manual Zero Calibration request");
        pageToSend = new byte[8] { 0x01, 0xAA, 0x0FF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        deviceChannel.sendAcknowledgedData(pageToSend);

    }

    void ChannelResponse(ANT_Response response) {

        if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_FAILED_0x06) {
            deviceChannel.sendAcknowledgedData(pageToSend); //send the page again if the transfer failed

        }
    }

}
