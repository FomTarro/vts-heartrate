using UnityEngine;
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

public class AntPlusManager : Singleton<AntPlusManager> {

    [SerializeField]
    private List<AntDevice> _scanResult = new List<AntDevice>();
    public List<AntDevice> Devices { get { return this._scanResult; } }

    //the sensor values we receive fron the onReceiveData event
    [SerializeField]
    private int _heartRate = 0;
    public int Heartrate { get { return this._heartRate; } }

    private AntChannel backgroundScanChannel;
    private AntChannel deviceChannel;

    //Start a background Scan to find the device
    public void StartScan() {
        Debug.Log("Looking for ANT + HeartRate sensor");
        AntManager.Instance.Init();
        _scanResult = new List<AntDevice>();
        backgroundScanChannel = AntManager.Instance.OpenBackgroundScanChannel(0);
        backgroundScanChannel.onReceiveData += OnReceivedBackgroundScanData;
    }

    //Windows and mac 
    //If the device is found 
    void OnReceivedBackgroundScanData(Byte[] data) {
        byte deviceType = (data[12]); // extended info Device Type byte

        if(deviceType == AntplusDeviceType.HeartRate){
            int deviceNumber = (data[10]) | data[11] << 8;
            byte transType = data[13];
            foreach (AntDevice d in _scanResult) {
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
            _scanResult.Add(foundDevice);
        }

    }

    public void ConnectToDevice(AntDevice device, Action<HttpUtils.ConnectionStatus> setStatus) {
        Debug.Log(device);
        AntManager.Instance.CloseBackgroundScanChannel();
        byte channelID = AntManager.Instance.GetFreeChannelID();
        deviceChannel = AntManager.Instance.OpenChannel(ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00, channelID, (ushort)device.deviceNumber, device.deviceType, device.transType, (byte)device.radiofreq, (ushort)device.period, false);
        HttpUtils.ConnectionStatus connectingStatus = new HttpUtils.ConnectionStatus();
        connectingStatus.message = "Connecting...";
        connectingStatus.status = HttpUtils.ConnectionStatus.Status.CONNECTING;
        setStatus(connectingStatus); 
        deviceChannel.onReceiveData += OnData;
        deviceChannel.onReceiveData += (d) => { 
            HttpUtils.ConnectionStatus connectedStatus = new HttpUtils.ConnectionStatus();
            connectedStatus.message = "Connected";
            connectedStatus.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
            setStatus(connectedStatus); 
        };
        deviceChannel.onChannelResponse += OnChannelResponse;
        deviceChannel.hideRXFAIL = true;
    }


    //Deal with the received Data
    public void OnData(Byte[] data) {
        this._heartRate = (data[7]);
    }

    void OnChannelResponse(ANT_Response response) {

    }

    public override void Initialize(){

    }
}
