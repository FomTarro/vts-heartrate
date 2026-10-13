ADVANCED ANT+ FOR UNITY

*** using the ANT library on windows and macOS

To Get started you need to sign up on thisisant.com and become a ANT+ Adopter for devices documentation and network key

To obtain the network key:
Register https://www.thisisant.com/register/
Once your basic user account is activated, login and go to your MyANT+ page  https://www.thisisant.com/my-ant to add ANT+ Adopter
Next search "Network Keys" on thisisant.com, we want the first key on the txt

The key must be set in AntManager.cs ==>replace  readonly byte[] NETWORK_KEY = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

For more info about ANT+ https://www.thisisant.com/developer/ant-plus/ant-plus-basics/

USAGE

1. Init the device, this is you usb2 or usb-m dongle
AntManager.Instance.Init();

2. Subscribe to device event
AntManager.Instance.onDeviceResponse += OnDeviceResponse;  
   
3. Open a channel with device configuration
AntManager.Instance.OpenChannel(...);

4. Subscribe to response and data event
myChannel.onReceiveData += ReceivedAntData;
myChannel.onChannelResponse += OnChannelResponse;


A detailed example is available in the antDemo.cs file

Device settings are found on thisisant.com 
example: ANT_Device_Profile_Heart_Rate_Monitor.pdf 



FAQ

- How do I configure a channel to connect to a HR SENSOR ?
To read data from an HR sensor the correct configuration as found in ANT+_Device_Profile_-_Heart_Rate_Rev_2.00.pdf page 11 ont thisisant.com
 AntManager.Instance.OpenChannel(ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00, 2, 0, 120, 0, 57, 8070, false); 
 the instantaneous HR is on data[7].
 
 -How to get data from a power Meter ?
 Settings found in ANT+_Device_Profile_-_Bicycle_Power_-_Rev4.2.pdf page 19
 powerDisplay = AntManager.Instance.OpenChannel(ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00, 2, 0, 11, 0, 57, 8182, false); 
 then in the data event method, as described in page 31 for Instantaneous Power
 int power= (data[6]) | data[7] << 8;
 
 -How can I auto connect to multiple bike speed sensor ?
 have a look at the MultiBikeSpeedDisplayExample.cs and demo scene. We create a Bike Display class and open a new channel everytime the background scan finds a sensor of the correct type.

 -Why do I get RX Fail messages ?
 Lots of rx fails means the channel period is wrong. Otherwise it is normal to have some amount of rx fails. You can now turn the log off with mychannel.hideRXFAIL = true

 -What are extended info ?
 you will find the info related to the background search extended data in this file:
https://www.thisisant.com/resources/ant-message-protocol-and-usage/
Page 35 of 134 
