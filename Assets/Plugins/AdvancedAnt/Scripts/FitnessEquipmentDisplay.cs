using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ANT_Managed_Library;
using System;



public class TrainerCapabilities
{
    public int maximumResistance;
    public bool basicResistanceNodeSupport;
    public bool targetPowerModeSupport;
    public bool simulationModeSupport;

}

public class CommandStatus
{

    public int lastReceivedCommandId;
    public int status;
    public int lastReceivedSequenceNumber;
    public byte byte_4;
    public byte byte_5;
    public byte byte_6;
    public byte byte_7;
}

public class UserConfiguration
{

    public float bicycleWeight;
    public float userWeight;

}

public class FitnessEquipmentDisplay : MonoBehaviour
{

    public bool autoStartScan = true; //start scan on play
    public bool connected = false; //will be set to true once connected



    //windows and mac settings
    public bool autoConnectToFirstSensorFound = true; //for windows and mac, either connect to the first sensor found 
    public List<AntDevice> scanResult; //or let you pick a sensor manually in the scanResult list with your own UI and call ConnectToDevice(AntDevice device)

    //android settings
    public bool useAndroidUI = true; //will open the unified ant+ UI on the android app if set to true, otherwise will connect to the first found device
    public bool skipPreferredSearch = true;  //- True = Don't automatically connect to user's preferred device, but always go to search for other devices.

    //the sensor values we receive fron the onReceiveData event
    public float speed; //Instantaneous speed
    public float elapsedTime; //Accumulated value of the elapsed time since start of workout in seconds
    public int heartRate; //0xFF indicates invalid
    public int distanceTraveled; //Accumulated value of the distance traveled since start of workout in meters
    public int instantaneousPower; //Stationary Bike specific
    public int cadence; //Specific Trainer Data

    private TrainerCapabilities trainerCapabilities = new TrainerCapabilities();

    private AntChannel backgroundScanChannel;
    private AntChannel deviceChannel;
    private byte[] pageToSend;
    private bool request_page_54 = false;
    private bool request_page_55 = false;
    private bool request_page_71 = false;
    public int deviceID = 0; //set this to connect to a specific device ID
    void Start()
    {


        if (autoStartScan)
            StartScan();

    }

    //Start a background Scan to find the device
    public void StartScan()
    {

        Debug.Log("Looking for ANT + Fitness Equipment sensor");
#if UNITY_ANDROID && !UNITY_EDITOR
        //java : connect_fitness(String gameobjectName, boolean useAndroidUI, boolean skipPreferredSearch, int deviceID)

        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
              
                    activity.Call("connect_fitness", this.gameObject.name, useAndroidUI, skipPreferredSearch,deviceID);
                
            }
        }
#else

        AntManager.Instance.Init();
        AntManager.Instance.onDeviceResponse += OnDeviceResponse;
        AntManager.Instance.onSerialError += OnSerialError; //if usb dongle is unplugged for example

        scanResult = new List<AntDevice>();
        backgroundScanChannel = AntManager.Instance.OpenBackgroundScanChannel(0);
        backgroundScanChannel.onReceiveData += ReceivedBackgroundScanData;
#endif


    }

    void OnDeviceResponse(ANT_Response response)
    {
        Debug.Log("device:" + response.getMessageID().ToString());
    }

    void OnSerialError(SerialError serialError)
    {
        Debug.Log("Error:" + serialError.error.ToString());

      

    }
    //Android function
    void ANTPLUG_ConnectEvent(string resultCode)
    {
        switch (resultCode)
        {
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

    void ANTPLUG_StateChange(string newDeviceState)
    {
        switch (newDeviceState)
        {
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

    void ANTPLUG_Receive_instantaneousHeartRate(string s)
    {
        heartRate = (int)float.Parse(s) * 2;
    }
    void ANTPLUG_Receive_elapsedTime(string s)
    {
        elapsedTime = float.Parse(s);
    }
    void ANTPLUG_Receive_instantaneousSpeed(string s)
    {
        speed = float.Parse(s) * 3.6f;
    }
    void ANTPLUG_Receive_cumulativeDistance(string s)
    {
        distanceTraveled = (int)float.Parse(s);
    }
    //Bike trainer
    void ANTPLUG_Receive_trainer_instantaneousPower(string s)
    {
        instantaneousPower = (int)float.Parse(s);
    }
    void ANTPLUG_Receive_trainer_instantaneousCadence(string s)
    {
        cadence = (int)float.Parse(s);
    }

    //receive user config from android
    void ANTPLUG_Receive_trainer_userconfig(string jsonstring)
    {
        // cadence = (int)float.Parse(s);
    }

    //receive trainer Capabilities from android
    void ANTPLUG_Receive_trainer_capabilities(string jsonstring)
    {

        if (request_page_54 == true)
        {
            request_page_54 = false;
            trainerCapabilities = JsonUtility.FromJson<TrainerCapabilities>(jsonstring);
            Debug.Log("max resistance is: " + trainerCapabilities.maximumResistance);
            Debug.Log("basicResistanceNodeSupport: " + trainerCapabilities.basicResistanceNodeSupport);
            Debug.Log("targetPowerModeSupport: " + trainerCapabilities.targetPowerModeSupport);
            Debug.Log("simulationModeSupport: " + trainerCapabilities.simulationModeSupport);
        }

    }
    //receive command status from android
    void ANTPLUG_Receive_Trainer_commandStatus(string jsonstring)
    {


        if (request_page_71 == true)
        {
            request_page_71 = false;

            CommandStatus status = JsonUtility.FromJson<CommandStatus>(jsonstring);

            ReadCommandStatus(status);
        }

    }
    //receive user config from android
    void ANTPLUG_Receive_userConfiguration(string jsonstring)
    {


        if (request_page_55 == true)
        {
            request_page_55 = false;

            UserConfiguration conf = JsonUtility.FromJson<UserConfiguration>(jsonstring);

            ReadUserConFig(conf);
        }

    }
    //Windows and mac 
    //If the device is found
    void ReceivedBackgroundScanData(Byte[] data)
    {

        byte deviceType = (data[12]); // extended info Device Type byte
        Debug.Log("background scan found device type:" + deviceType);
        switch (deviceType)
        {

            case AntplusDeviceType.FitnessEquipment:
                {
                    int deviceNumber = (data[10]) | data[11] << 8;
                    byte transType = data[13];
                    foreach (AntDevice d in scanResult)
                    {
                        if (d.deviceNumber == deviceNumber && d.transType == transType) //device already found
                            return;
                    }

                    Debug.Log("FitnessEquipmentfound " + deviceNumber);

                    AntDevice foundDevice = new AntDevice();
                    foundDevice.deviceType = deviceType;
                    foundDevice.deviceNumber = deviceNumber;
                    foundDevice.transType = transType;
                    foundDevice.period = 8192;
                    foundDevice.radiofreq = 57;
                    foundDevice.name = "FitnessEquipment(" + foundDevice.deviceNumber + ")";
                    scanResult.Add(foundDevice);
                    if (autoConnectToFirstSensorFound)
                    {
                        ConnectToDevice(foundDevice);
                    }
                    break;
                }

            default:
                {

                    break;
                }
        }

    }

    void ConnectToDevice(AntDevice device)
    {
        AntManager.Instance.CloseBackgroundScanChannel();
        byte channelID = AntManager.Instance.GetFreeChannelID();
        deviceChannel = AntManager.Instance.OpenChannel(ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00, channelID, (ushort)device.deviceNumber, device.deviceType, device.transType, (byte)device.radiofreq, (ushort)device.period, false);
        connected = true;
        deviceChannel.onReceiveData += Data;
        deviceChannel.onChannelResponse += ChannelResponse;
        deviceChannel.hideRXFAIL = true;
    }


    //Deal with the received Data
    int prevDistance = 0;
    float prevTime = 0;
    bool firstDistanceInfo;
    bool firstTimeInfo;

    float pTime = 0;
    public void Data(Byte[] data)
    {

        Debug.Log(Time.time - pTime);
        pTime = Time.time;
        // General FE Data
        if (data[0] == 16)
        {

            if (prevDistance < data[3])
                distanceTraveled += data[3] - prevDistance;

            if (firstDistanceInfo == false)
            {
                distanceTraveled -= data[3];
                firstDistanceInfo = true;
            }
            prevDistance = data[3];

            // elapsed time

            if (prevTime < data[2])
                elapsedTime += (data[2] - prevTime) * 0.25f;

            if (firstTimeInfo == false)
            {
                elapsedTime -= data[2] * 0.25f;
                firstTimeInfo = true;
            }

            prevTime = data[2];

            //heart rate
            heartRate = data[6];

            //speed
            speed = ((data[4]) | data[5] << 8) * 0.0036f;



        }

        if (data[0] == 25)
        {  // Specific Trainer Data

            cadence = data[2];
            int nibble2 = (byte)(data[6] & 0xf);
            instantaneousPower = (data[5]) | nibble2 << 8;


        }
        else if (data[0] == 54 && request_page_54 == true)
        { //response to trainer capabilities request from PC & MAC


            request_page_54 = false;

            trainerCapabilities.maximumResistance = (data[5]) | data[6] << 8;
            trainerCapabilities.basicResistanceNodeSupport = (data[7] >> 0) != 0;
            trainerCapabilities.targetPowerModeSupport = (data[7] >> 1) != 0;
            trainerCapabilities.simulationModeSupport = (data[7] >> 2) != 0;
            Debug.Log("max resistance is: " + trainerCapabilities.maximumResistance + " N");
            Debug.Log("basicResistanceNodeSupport: " + trainerCapabilities.basicResistanceNodeSupport);
            Debug.Log("targetPowerModeSupport: " + trainerCapabilities.targetPowerModeSupport);
            Debug.Log("simulationModeSupport: " + trainerCapabilities.simulationModeSupport);

        }
        else if (data[0] == 55 && request_page_55 == true)
        {   //user configuration data page
            request_page_55 = false;

            int bikeWeight = (data[4] >> 4) | (data[5] << 4);
            int riderWeight = (data[1]) | (data[2] << 8);

            UserConfiguration conf = new UserConfiguration();
            conf.userWeight = riderWeight * 0.01f;
            conf.bicycleWeight = bikeWeight * 0.05f;

            ReadUserConFig(conf);

        }
        else if (data[0] == 71 & request_page_71 == true)
        {   //command status data page

            request_page_71 = false;
            CommandStatus status = new CommandStatus();
            status.lastReceivedCommandId = data[1];
            status.lastReceivedSequenceNumber = data[2];
            status.status = data[3];
            status.byte_4 = data[4];
            status.byte_5 = data[5];
            status.byte_6 = data[6];
            status.byte_7 = data[7];
            ReadCommandStatus(status);

        }
    }

    private static void ReadCommandStatus(CommandStatus status)
    {

        Debug.Log("Last_Received_Command_ID: " + status.lastReceivedCommandId);
        Debug.Log("sequence_number: " + status.lastReceivedSequenceNumber);
        Debug.Log("command_status: " + status.status);

        if (status.lastReceivedCommandId == 51)
        {   //we are in Track resistance Mode

            int rawSlopeValue = (status.byte_5) | (status.byte_6 << 8);
            float slope = (rawSlopeValue - 20000) / 100;
            Debug.Log("current slope is: " + slope);

        }
        else if (status.lastReceivedCommandId == 48)
        {   //we are in basic resistance Mode

            int resistance = status.byte_7;
            Debug.Log("Total Resistance is: " + (resistance / 2) + "%");

        }
        else if (status.lastReceivedCommandId == 49)
        {  //we are in target power Mode

            int power = (status.byte_6) | (status.byte_7 << 8);

            Debug.Log("Target power: " + power / 4);
        }
    }

    private static void ReadUserConFig(UserConfiguration config)
    {

        Debug.Log("rider weight is: " + config.userWeight);
        Debug.Log("bike weight is: " + config.bicycleWeight);

    }


    //set the trainer in resistance mode, if the trainer has basicResistanceNodeSupport 
    public void SetTrainerResistance(int resistance)
    {
        if (!connected)
            return;
#if UNITY_ANDROID && !UNITY_EDITOR
        
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("Set_fitness_Resistance",resistance);
            }
        }
#else
        pageToSend = new byte[8] { 0x30, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, (byte)(resistance * 2) };//unit is 0.50%
        deviceChannel.sendAcknowledgedData(pageToSend);
#endif
    }

    //set the trainer in target power if the trainer has targetPowerModeSupport 
    public void SetTrainerTargetPower(int targetpower)
    {
        if (!connected)
            return;
#if UNITY_ANDROID && !UNITY_EDITOR
        
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("Set_fitness_TargetPower",targetpower);
            }
        }
#else

        Debug.Log("sending target power of " + targetpower + " watts to trainer");
        byte LSB = (byte)(targetpower * 4);
        byte MSB = (byte)((targetpower * 4 >> 8));

        pageToSend = new byte[8] { 0x31, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, LSB, MSB };//unit is 0.25w
        deviceChannel.sendAcknowledgedData(pageToSend);
#endif
    }
    //set the trainer in simulation mode if the trainer has simulationModeSupport 
    public void SetTrainerSlope(int slope)
    {
        if (!connected)
            return;
        slope = Mathf.Clamp(slope, -200, 200);

#if UNITY_ANDROID && !UNITY_EDITOR
        
          
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("Set_fitness_SimulationMode",slope);
            }
        }
#else

        slope += 200;
        // Units are 0.01%
        slope *= 100;
        int grade = (int)slope;
        byte gradeLsb = (byte)(grade);
        byte gradeMsb = (byte)(grade >> 8);
        byte[] pageToSend = new byte[8] { 0x33, 0xFF, 0xFF, 0xFF, 0xFF, gradeLsb, gradeMsb, 0xFF };
        deviceChannel.sendAcknowledgedData(pageToSend);

#endif
    }

    //send use configuration
    public void SetTrainerUserConfiguration(float bikeWeight, float userWeight)
    {
        if (!connected)
            return;



#if UNITY_ANDROID && !UNITY_EDITOR
        
        

        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("Set_fitness_UserConfiguration",bikeWeight,userWeight);
            }
        }
#else


        int rawBikeWeight = Mathf.FloorToInt((bikeWeight / 0.05f));
        int rawUserWeight = Mathf.FloorToInt((userWeight / 0.01f));
        byte weightLSB = (byte)(rawUserWeight);
        byte weightMSB = (byte)(rawUserWeight >> 8);

        byte bikeweightLSB = (byte)(rawBikeWeight << 4 | 0xF);
        byte bikeweightMSB = (byte)(rawBikeWeight >> 4);

        pageToSend = new byte[8] { 0x37, weightLSB, weightMSB, 0xFF, bikeweightLSB, bikeweightMSB, 0xFF, 0x00 };

        deviceChannel.sendAcknowledgedData(pageToSend);
#endif
    }

    //check if the last command was successfull and get current mode data (target power, resistandce, slope)
    public void RequestCommandStatus()
    {
        if (!connected)
            return;
        request_page_71 = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        
       
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("Request_CommandStatus");
            }
        }
#else

        byte[] pageToSend = new byte[8] { 0x46, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x47, 0x01 };
        deviceChannel.sendAcknowledgedData(pageToSend);
#endif


    }
    //check the supported mode
    public void RequestTrainerCapabilities()
    {
        if (!connected)
            return;
        request_page_54 = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("Request_trainer_capabilities");
            }
        }
#else

        byte[] pageToSend = new byte[8] { 0x46, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x36, 0x01 };
        deviceChannel.sendAcknowledgedData(pageToSend);
#endif


    }
    //get the user config
    public void RequestUserConfig()
    {
        if (!connected)
            return;
        request_page_55 = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("Request_UserConfig");
            }
        }
#else

        byte[] pageToSend = new byte[8] { 0x46, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x37, 0x01 };
        deviceChannel.sendAcknowledgedData(pageToSend);
#endif


    }
    void ChannelResponse(ANT_Response response)
    {

        if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_FAILED_0x06)
        {
            if (pageToSend != null)
                deviceChannel.sendAcknowledgedData(pageToSend); //send the page again if the transfer failed

        }
    }


}
