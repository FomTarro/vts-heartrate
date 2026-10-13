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

public class HeartRateDisplay : MonoBehaviour {

    public bool autoStartScan = true; //start scan on play
    public bool connected = false; //will be set to true once connected

    //windows and mac settings
    public bool autoConnectToFirstSensorFound = true; //for windows and mac, either connect to the first sensor found or let you pick a sensor manually in the scanResult list with your own UI and call ConnectToDevice(AntDevice device)
    public List<AntDevice> scanResult;

    //the sensor values we receive fron the onReceiveData event
    public float heartRate; // the computed HR count  in BPM

    private AntChannel backgroundScanChannel;
    private AntChannel deviceChannel;

    void Start() {

        if (autoStartScan)
            StartScan();

    }

    //Start a background Scan to find the device
    public void StartScan() {

        Debug.Log("Looking for ANT + HeartRate sensor");

        AntManager.Instance.Init();
        scanResult = new List<AntDevice>();
        backgroundScanChannel = AntManager.Instance.OpenBackgroundScanChannel(0);
        backgroundScanChannel.onReceiveData += ReceivedBackgroundScanData;
    }

    //Windows and mac 
    //If the device is found 
    void ReceivedBackgroundScanData(Byte[] data) {

        byte deviceType = (data[12]); // extended info Device Type byte

        switch (deviceType) {

            case AntplusDeviceType.HeartRate: {
                    int deviceNumber = (data[10]) | data[11] << 8;
                    byte transType = data[13];
                    foreach (AntDevice d in scanResult) {
                        if (d.deviceNumber == deviceNumber && d.transType == transType) //device already found
                            return;
                    }

                    Debug.Log("Heart rate sensor found " + deviceNumber);

                    AntDevice foundDevice = new AntDevice();
                    foundDevice.deviceType = deviceType;
                    foundDevice.deviceNumber = deviceNumber;
                    foundDevice.transType = transType;
                    foundDevice.period = 8070;
                    foundDevice.radiofreq = 57;
                    foundDevice.name = "heartrate(" + foundDevice.deviceNumber + ")";
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
        //HR
        heartRate = (data[7]);
    }



    void ChannelResponse(ANT_Response response) {

    }

}
