using UnityEngine;
using UnityEngine.Events;
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

    private AntChannel _backgroundScanChannel;
    private ConnectedAntDevice _connectedDevice = new ConnectedAntDevice(null, null, null);

    //Start a background Scan to find the device
    public void StartScan() {
        Debug.Log("Looking for ANT + HeartRate sensor");
        AntManager.Instance.Init();
        this._scanResult = new List<AntDevice>();
        if(this._connectedDevice.device != null){
            this._scanResult.Add(_connectedDevice.device);
        }
        this._backgroundScanChannel = AntManager.Instance.OpenBackgroundScanChannel(0);
        this._backgroundScanChannel.onReceiveData += OnReceivedBackgroundScanData;
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
            foundDevice.name = "HRM (" + foundDevice.deviceNumber + ")";
            this._scanResult.Add(foundDevice);
            this._scanResult.Sort();
        }

    }

    public void ConnectToDevice(AntDevice device, Action<HttpUtils.ConnectionStatus> onStatus) {
        if(device != null){
            try{
                AntManager.Instance.CloseBackgroundScanChannel();
                if(this._connectedDevice.device != null && device.name != this._connectedDevice.device.name){
                    DisconnectFromDevice(onStatus);
                }
                byte channelID = AntManager.Instance.GetFreeChannelID();
                AntChannel deviceChannel = AntManager.Instance.OpenChannel(
                    0x00, channelID, 
                    (ushort)device.deviceNumber, 
                    device.deviceType, 
                    device.transType, 
                    (byte)device.radiofreq, 
                    (ushort)device.period, 
                    false);
                HttpUtils.ConnectionStatus connectingStatus = new HttpUtils.ConnectionStatus();
                connectingStatus.status = HttpUtils.ConnectionStatus.Status.CONNECTING;
                onStatus.Invoke(connectingStatus); 
                deviceChannel.onChannelResponse += OnChannelResponse;
                deviceChannel.hideRXFAIL = true;
                deviceChannel.onReceiveData += OnData;
                deviceChannel.onReceiveData += (d) => { 
                    this._connectedDevice = new ConnectedAntDevice(device, deviceChannel, onStatus);
                    HttpUtils.ConnectionStatus connectedStatus = new HttpUtils.ConnectionStatus();
                    connectedStatus.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
                    onStatus.Invoke(connectedStatus); 
                };
            }catch(Exception e){
                HttpUtils.ConnectionStatus errorStatus = new HttpUtils.ConnectionStatus();
                errorStatus.message = e.Message;
                errorStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
                onStatus.Invoke(errorStatus); 
            }
        }else{
            HttpUtils.ConnectionStatus errorStatus = new HttpUtils.ConnectionStatus();
            errorStatus.message = "No device selected";
            errorStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
            onStatus.Invoke(errorStatus); 
        }
    }

    public void DisconnectFromDevice(Action<HttpUtils.ConnectionStatus> onStatus, bool isTimeout = false){
        if(this._connectedDevice.channel != null){
            // this._connectedDevice.channel.Close();
        }
        // this._connectedDevice = new ConnectedAntDevice(null, null, null);
        HttpUtils.ConnectionStatus disconnectStatus = new HttpUtils.ConnectionStatus();
        if(!isTimeout){
            disconnectStatus.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
        }else{
            disconnectStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
            disconnectStatus.message = "Connection to device was lost";
        }
        onStatus.Invoke(disconnectStatus); 
    }

    private float _timeout = 0f;

    //Deal with the received Data
    public void OnData(Byte[] data) {
        this._timeout = 5f;
        this._heartRate = (data[7]);
    }

    private void Update(){
        if(this._timeout > 0){
            this._timeout = this._timeout - Time.deltaTime;
            if(this._timeout <= 0){
                DisconnectFromDevice(
                    this._connectedDevice.onTimeout != null 
                    ? this._connectedDevice.onTimeout 
                    : (s) => {},
                true);
            }
        }
    }

    void OnChannelResponse(ANT_Response response) {

    }

    public override void Initialize(){

    }

    private struct ConnectedAntDevice{
        public AntDevice device;
        public AntChannel channel;
        public Action<HttpUtils.ConnectionStatus> onTimeout;
        public ConnectedAntDevice(AntDevice device, AntChannel channel, Action<HttpUtils.ConnectionStatus> onTimeout){
            this.device = device;
            this.channel = channel;
            this.onTimeout = onTimeout;
        }
    }
}
