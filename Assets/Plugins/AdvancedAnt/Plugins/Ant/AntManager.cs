using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using ANT_Managed_Library;
using System.Text;
using System.Collections.Generic;


/*
 * AntManager
 *
 * Call the Init method from your script and then open a channel. 
 * Ant responses and received DATA are queued when received
 * and dequeued in the update loop by triggering events you can register on your script
 */

public class SerialError {
    public ANT_Device sender;
    public ANT_Device.serialErrorCode error;
    public bool isCritical;

    public SerialError(ANT_Device sender, ANT_Device.serialErrorCode error, bool isCritical) {
        this.sender = sender;
        this.error = error;
        this.isCritical = isCritical;
    }
}

public class AntManager : MonoBehaviour {
    static AntManager _instance;
    public static int lastChannel = 0;
    public static AntManager Instance {
        get {
            if (_instance == null)
                return _instance = (new GameObject("AntManager")).AddComponent<AntManager>();
            else
                return _instance;
        }
    }
    readonly byte[] NETWORK_KEY = { 0xB9, 0xA5, 0x21, 0xFB, 0xBD, 0x72, 0xC3, 0x45 }; // <- key is set
    //readonly byte[] NETWORK_KEY = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // COPY THE CORRECT NETWORK KEY HERE
    /*
    * To obtain the network key:
    * register on https://www.thisisant.com/register/
    * Once your basic user account is activated, login and go to your MyANT+ page  https://www.thisisant.com/my-ant to add ANT+ Adopter,
    * search "Network Keys" on thisisant.com, we want the first key on the txt
    */



    public ANT_Device device {
        get {
            return devices[0];
        }
    }
    public ANT_Device[] devices = new ANT_Device[8]; //if you need more than 8 usb devices, change this
    Queue<ANT_Response> messageQueue;
    public delegate void OnDeviceResponse(ANT_Response response);
    public event OnDeviceResponse onDeviceResponse; //ant response event
    public delegate void OnSerialError(SerialError error);
    public event OnSerialError onSerialError;
    Queue<SerialError> errorQueue;
    public List<AntChannel> channelList;
    public bool[,] channelIDUsed = new bool[8, 8];
    private AntChannel backgroundScanChannel;
    private int nScanRequest = 0;
    void Awake() {
        // Sets this to not be destroyed when reloading scene
        if (NETWORK_KEY[0] == 0x00) {
            Debug.LogWarning("NETWORK_KEY NOT SET IN ANTMANAGER.CS ! INSTRUCTIONS IN PC_MAC_readme.txt");
        }
        DontDestroyOnLoad(gameObject);
    }
    void Update() {

        if (messageQueue != null && messageQueue.Count > 0) {
            if (onDeviceResponse != null)
                onDeviceResponse(messageQueue.Dequeue());
        }

        if (onSerialError != null && errorQueue != null && errorQueue.Count > 0)
            onSerialError(errorQueue.Dequeue());

    }


    public void Init(byte USBDeviceNum = 0) {
        if (ANT_Common.getNumDetectedUSBDevices() < USBDeviceNum) {
            Debug.Log("ANT+ cannot detect USB device #" + USBDeviceNum);
            return;
        }

        // if (deviceList == null)
        //   deviceList = new List<ANT_Device>();
        if (messageQueue == null)
            messageQueue = new Queue<ANT_Response>(16);
        if (errorQueue == null)
            errorQueue = new Queue<SerialError>(16);
        if (channelList == null)
            channelList = new List<AntChannel>();

        //init the device
        if (devices[USBDeviceNum] == null) {


            devices[USBDeviceNum] = new ANT_Device(USBDeviceNum, 57000);
            devices[USBDeviceNum].deviceResponse += new ANT_Device.dDeviceResponseHandler(DeviceResponse);
            devices[USBDeviceNum].serialError += new ANT_Device.dSerialErrorHandler(SerialErrorHandler);
            devices[USBDeviceNum].ResetSystem();
            devices[USBDeviceNum].setNetworkKey(0, NETWORK_KEY, 500);

        }
    }

    public void Reconnect(ANT_Device previousDevice) {

        int usbNum = previousDevice.getOpenedUSBDeviceNum();
        devices[usbNum] = previousDevice;
        previousDevice.deviceResponse += new ANT_Device.dDeviceResponseHandler(DeviceResponse);
        previousDevice.serialError += new ANT_Device.dSerialErrorHandler(SerialErrorHandler);
        previousDevice.ResetSystem();
        previousDevice.setNetworkKey(0, NETWORK_KEY, 500);

    }
    public AntChannel OpenChannel(ANT_ReferenceLibrary.ChannelType channelType, byte userChannel, ushort deviceNum, byte deviceType, byte transType, byte radioFreq, ushort channelPeriod, bool pairing, int USBNum = 0) {
        AntChannel channel = this.gameObject.AddComponent<AntChannel>();
        channelList.Add(channel);
        channelIDUsed[USBNum, userChannel] = true;
        channel.ConfigureAnt(channelType, userChannel, deviceNum, deviceType, transType, radioFreq, channelPeriod, pairing, USBNum);
        return channel;
    }

 
    public AntChannel OpenBackgroundScanChannel(byte userChannel, byte USBDeviceNum = 0) {

        nScanRequest++;
        if (backgroundScanChannel) //if a background Scan channel already exist
            return backgroundScanChannel;

        channelIDUsed[USBDeviceNum, userChannel] = true;
        backgroundScanChannel = this.gameObject.AddComponent<AntChannel>();
        channelList.Add(backgroundScanChannel);
        backgroundScanChannel.ConfigureScan(userChannel, USBDeviceNum);

        return backgroundScanChannel;
    }

    public AntChannel OpenContinuousScanChannel(byte radioFreq, byte USBDeviceNum = 0) {

        AntChannel channel = this.gameObject.AddComponent<AntChannel>();
        channel.ConfigureContinuousScan(0x00, radioFreq, USBDeviceNum);
        return channel;
    }

    public void CloseBackgroundScanChannel() {
        Invoke("CloseBackgroundScan", 1);

    }

    void CloseBackgroundScan() {
        nScanRequest--;
        if (nScanRequest == 0) {
            backgroundScanChannel.Close();
            backgroundScanChannel = null;
            Debug.Log("all devices connected, closing background scan channel");
        }
    }

    public byte GetFreeChannelID(int USBNum = 0) {

        for (int id = 1; id <= 8; id++) {
            if (channelIDUsed[USBNum, id] == false)
                return (byte)id;
        }
        Debug.LogWarning("no free ANT + channel available!");
        return 0;
    }


    /*
    * DeviceResponse
    * Called whenever a message is received from ANT unless that message is a 
    * channel event message. 
    * response: ANT message
    */

    void DeviceResponse(ANT_Response response) {
        if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40)
            messageQueue.Enqueue(response);
    }

    void SerialErrorHandler(ANT_Device sender, ANT_Device.serialErrorCode error, bool isCritical) {
        if (onSerialError != null) {
            SerialError serialError = new SerialError(sender, error, isCritical);
            errorQueue.Enqueue(serialError);


        }

    }

    void OnApplicationQuit() {
        //dispose the device on app quit or the application will freeze

        foreach (ANT_Device device in devices) {
            if (device != null)
                device.Dispose();
        }
    }
}
