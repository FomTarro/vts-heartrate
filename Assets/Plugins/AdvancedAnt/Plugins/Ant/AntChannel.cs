using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using ANT_Managed_Library;
using System.Text;
using System.Collections.Generic;
////////////////////////////////////////////////////////////////////////////////
// AntManager
//
// Call the main Init method from your script to open a channel. 
// Ant responses and received DATA are queued when received and dequeued in the update loop by triggering events you can register on your script
// 
////////////////////////////////////////////////////////////////////////////////
public class AntChannel : MonoBehaviour
{
    public ANT_Device device;
    ANT_Channel channel;

    public byte[] txBuffer = { 0, 0, 0, 0, 0, 0, 0, 0 };

    bool broadcasting;

    Queue<byte[]> RXQueue;
    Queue<ANT_Response> messageQueue;


    public delegate void OnReceiveData(byte[] data);
    public event OnReceiveData onReceiveData; // data event

    public delegate void OnChannelResponse(ANT_Response response);
    public event OnChannelResponse onChannelResponse; //ant response event


    //channel configuration
    ANT_ReferenceLibrary.ChannelType channelType;
    public byte userChannel; //{ get; private set; }
    ushort deviceNum;
    byte deviceType;
    byte transType;
    byte radioFreq;
    ushort channelPeriod;
    bool pairing;

    bool isBackgroundScan = false;
    public bool hideRXFAIL = false;

    void Update() {
        if (RXQueue != null && RXQueue.Count > 0) {
            if (onReceiveData != null)
                onReceiveData(RXQueue.Dequeue());

        }

        if (messageQueue != null && messageQueue.Count > 0) {
            if (onChannelResponse != null)
                onChannelResponse(messageQueue.Dequeue());
        }
    }


    ////////////////////////////////////////////////////////////////////////////////
    // ConfigureAnt
    //
    // You can find how to initialize devices on thisisant.com in the download documents section
    // ANT+ DEVICE PROFILES
    // 
    ////////////////////////////////////////////////////////////////////////////////
    public void ConfigureAnt(ANT_ReferenceLibrary.ChannelType channelType, byte userChannel, ushort deviceNum, byte deviceType, byte transType, byte radioFreq, ushort channelPeriod, bool pairing, int USBNum)
    {
        this.channelType = channelType;
        this.userChannel = userChannel;
        this.deviceNum = deviceNum;
        this.deviceType = deviceType;
        this.transType = transType;
        this.radioFreq = radioFreq;
        this.channelPeriod = channelPeriod;
        this.pairing = pairing;

        RXQueue = new Queue<byte[]>(16);
        messageQueue = new Queue<ANT_Response>(16);
        device = AntManager.Instance.devices[USBNum];
        channel = device.getChannel(userChannel);
        channel.channelResponse += new dChannelResponseHandler(ChannelResponse);
        channel.assignChannel(channelType, 0, 0);
        channel.setChannelID(deviceNum, pairing, deviceType, transType, 0);
        channel.setChannelFreq(radioFreq, 0);
        channel.setChannelPeriod(channelPeriod, 0);
        channel.setLowPrioritySearchTimeout(0);
        isBackgroundScan = false;
        channel.openChannel();
        
        broadcasting = true;
    }
    public void ConfigureScan(byte userChannel, ushort USBNum)
    {
        this.userChannel = userChannel;
        RXQueue = new Queue<byte[]>(16);
        messageQueue = new Queue<ANT_Response>(16);
        device = AntManager.Instance.devices[USBNum];
        device.enableRxExtendedMessages(true, 500);
        channel = device.getChannel(userChannel);
        channel.channelResponse += new dChannelResponseHandler(ChannelResponse);

        channel.assignChannelExt(ANT_ReferenceLibrary.ChannelType.ADV_TxRx_Only_or_RxAlwaysWildCard_0x40, 0, ANT_ReferenceLibrary.ChannelTypeExtended.ADV_AlwaysSearch_0x01, 500);
        channel.setChannelID(0, false, 0, 0, 500);
        channel.setChannelFreq(57, 500);
        channel.setChannelSearchTimeout(0);
        channel.setLowPrioritySearchTimeout((byte)0xFF);
        isBackgroundScan = true;
        channel.openChannel();

        broadcasting = true;

    }

    public void ConfigureContinuousScan(ANT_ReferenceLibrary.ChannelType channelType, byte radioFreq, ushort USBNum) {
        userChannel = 0;
        RXQueue = new Queue<byte[]>(16);
        messageQueue = new Queue<ANT_Response>(16);
        device = AntManager.Instance.devices[USBNum];
        device.enableRxExtendedMessages(true, 500);
        channel = device.getChannel(0);
        channel.channelResponse += new dChannelResponseHandler(ChannelResponse);

        channel.assignChannelExt(ANT_ReferenceLibrary.ChannelType.ADV_TxRx_Only_or_RxAlwaysWildCard_0x40, 0, ANT_ReferenceLibrary.ChannelTypeExtended.ADV_AlwaysSearch_0x01, 500);
        channel.setChannelID(0, false, 0, 0, 500);
        channel.setChannelFreq(radioFreq, 500);
        channel.setChannelSearchTimeout(0);
        channel.setLowPrioritySearchTimeout((byte)0xFF);
        isBackgroundScan = true;
        channel.openChannel();
        device.openRxScanMode();
        broadcasting = true;

    }


    public void Close() {
        if (channel != null) {
            channel.closeChannel();
            channel.Dispose();
        }
        if (isBackgroundScan) {
            device.enableRxExtendedMessages(false, 500);
            isBackgroundScan = false;
        }
        broadcasting = false;
        AntManager.Instance.channelList.Remove(this);
        AntManager.Instance.channelIDUsed[device.getOpenedUSBDeviceNum(), userChannel] = false;
        Destroy(this);
    }

    public void PauseChannel() {
        broadcasting = false;
    }

    public void ReOpen() {
        if (broadcasting)
            return;

        if (!isBackgroundScan)
            ConfigureAnt(channelType, userChannel, deviceNum, deviceType, transType, radioFreq, channelPeriod, pairing, device.getOpenedUSBDeviceNum());
        else
            ConfigureScan(userChannel, (ushort)device.getOpenedUSBDeviceNum());
    }

    public void ReOpen(ANT_Device device)
    {
        if (broadcasting)
            return;

        this.device = device;

        if (!isBackgroundScan)
            ConfigureAnt(channelType, userChannel, deviceNum, deviceType, transType, radioFreq, channelPeriod, pairing, device.getOpenedUSBDeviceNum());
        else
            ConfigureScan(userChannel, (ushort)device.getOpenedUSBDeviceNum());
    }
      

    public void sendAcknowledgedData(byte[] data) {
      
        channel.sendAcknowledgedData(data);
    }

    ////////////////////////////////////////////////////////////////////////////////
    // ChannelResponse
    //
    // Called whenever a channel event is received. 
    // 
    // response: ANT message
    ////////////////////////////////////////////////////////////////////////////////
    void ChannelResponse(ANT_Response response) {


        //With the messageQueue we can deal with ANT response in the Unity main thread
        if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40)
            messageQueue.Enqueue(response);

        try {
            switch ((ANT_ReferenceLibrary.ANTMessageID)response.responseID) {
                case ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40: {
                        switch (response.getChannelEventCode()) {
                            // This event indicates that a message has just been
                            // sent over the air. We take advantage of this event to set
                            // up the data for the next message period.   
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_TX_0x03: {
                                    // Broadcast data will be sent over the air on
                                    // the next message period
                                    if (broadcasting) {
                                        channel.sendBroadcastData(txBuffer);
                                    }
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_SEARCH_TIMEOUT_0x01: {
                                    
                                    Debug.Log("Search Timeout");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_0x02: {
                                    if (!hideRXFAIL)
                                        Debug.Log("Rx Fail");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_RX_FAILED_0x04: {
                                    Debug.Log("Burst receive has failed");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_COMPLETED_0x05: {
                                    Debug.Log("Transfer Completed");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_FAILED_0x06: {
                                    Debug.Log("Transfer Failed");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_CHANNEL_CLOSED_0x07: {
                                    channel.unassignChannel(500);
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_GO_TO_SEARCH_0x08: {
                                    Debug.Log("Go to Search");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_CHANNEL_COLLISION_0x09: {
                                    Debug.Log("Channel Collision");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_START_0x0A: {
                                    Debug.Log("Burst Started");
                                    break;
                                }
                            default: {
                                    Debug.Log("Unhandled Channel Event " + response.getChannelEventCode());
                                    break;
                                }
                        }
                        break;
                    }
                case ANT_ReferenceLibrary.ANTMessageID.BROADCAST_DATA_0x4E:
                case ANT_ReferenceLibrary.ANTMessageID.ACKNOWLEDGED_DATA_0x4F:
                case ANT_ReferenceLibrary.ANTMessageID.BURST_DATA_0x50:
                case ANT_ReferenceLibrary.ANTMessageID.EXT_BROADCAST_DATA_0x5D:
                case ANT_ReferenceLibrary.ANTMessageID.EXT_ACKNOWLEDGED_DATA_0x5E:
                case ANT_ReferenceLibrary.ANTMessageID.EXT_BURST_DATA_0x5F: {
                        if (response.isExtended() && isBackgroundScan == true)
                            RXQueue.Enqueue(response.messageContents);
                        else
                            RXQueue.Enqueue(response.getDataPayload());
                        break;
                    }
                default: {
                        Debug.Log("Unknown Message " + response.responseID);
                        break;
                    }
            }
        } catch (Exception ex) {
            Debug.Log("Channel response processing failed with exception: " + ex.Message);
        }
    }

}
