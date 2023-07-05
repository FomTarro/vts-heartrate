using System;
using System.Collections.Generic;
using ANT_Managed_Library;
using UnityEngine;

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
	private ConnectionData _connectedDevice = new ConnectionData(null, null, null);

	private float _timeout = 0f;
	private const float TIMEOUT_SECONDS = 5f;

	//Start a background Scan to find the device
	public void StartScan(Action<HttpUtils.ConnectionStatus> onStatus) {
		try {
			Debug.Log("Looking for ANT+ HeartRate sensors...");
			this._scanResult = new List<AntDevice>();
			if(this._backgroundScanChannel != null){
				this._backgroundScanChannel.ReOpen();
			}
			if (this._connectedDevice.device != null) {
				this._scanResult.Add(this._connectedDevice.device);
			}
		}
		catch (Exception e) {
			Debug.LogError(string.Format("Error scanning for ANT+ Devices: {0}", e));
			HttpUtils.ConnectionStatus errorStatus = new HttpUtils.ConnectionStatus();
			if (e.GetType() == typeof(ANT_Exception)) {
				errorStatus.message = "Unable to initialize USB:0";
			}
			else {
				errorStatus.message = e.Message;
			}
			errorStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
			onStatus.Invoke(errorStatus);
		}
	}

	//If the device is found 
	void OnReceivedBackgroundScanData(Byte[] data) {
		try{
			byte deviceType = (data[12]); // extended info Device Type byte

			if (deviceType == AntplusDeviceType.HeartRate) {
				int deviceNumber = (data[10]) | data[11] << 8;
				byte transType = data[13];
				foreach (AntDevice d in _scanResult) {
					if (d.deviceNumber == deviceNumber && d.transType == transType) //device already found
						return;
				}

				Debug.Log("New ANT+ heart rate sensor found: " + deviceNumber);

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
		}catch(Exception e){
			Debug.LogError("Error parsing ANT+ Background Scan Data: " + System.Text.Encoding.UTF8.GetString(data, 1, data.Length - 1) + " -> " + e);
		}

	}

	public void ConnectToDevice(AntDevice device, Action<HttpUtils.ConnectionStatus> onStatus) {
		if (device != null) {
			try {
				DisconnectFromDevice(onStatus);
				Debug.Log("Connecting to ANT+ device: " + device.name + "...");
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
					this._connectedDevice = new ConnectionData(device, deviceChannel, onStatus);
					HttpUtils.ConnectionStatus connectedStatus = new HttpUtils.ConnectionStatus();
					connectedStatus.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
					onStatus.Invoke(connectedStatus);
				};
				this._timeout = TIMEOUT_SECONDS;
			}
			catch (Exception e) {
				Debug.LogError("Error conencting to ANT+ device: " + e);
				HttpUtils.ConnectionStatus errorStatus = new HttpUtils.ConnectionStatus();
				errorStatus.message = e.Message;
				errorStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
				onStatus.Invoke(errorStatus);
			}
		}
		else {
			HttpUtils.ConnectionStatus errorStatus = new HttpUtils.ConnectionStatus();
			errorStatus.message = "No device selected";
			errorStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
			onStatus.Invoke(errorStatus);
		}
	}

	public void DisconnectFromDevice(Action<HttpUtils.ConnectionStatus> onStatus, bool isTimeout = false) {
		HttpUtils.ConnectionStatus disconnectStatus = new HttpUtils.ConnectionStatus();
		this._timeout = 0;
		if (!isTimeout) {
			Debug.Log("Manually disconnecting from ANT+ device: " + this._connectedDevice +"...");
			if (this._connectedDevice.channel != null) {
				Debug.Log("Closing ANT+ channel...");
				this._connectedDevice.channel.Close();
			}
			// no connected device, wipe all connection data
			this._connectedDevice = new ConnectionData(null, null, null);
			disconnectStatus.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
		}
		else {
			// In case of timeout...
			Debug.LogWarning("Connection to ANT+ device " + this._connectedDevice + " was lost!");
			// keep the channel alive to attempt a reconnect (should the device come back in to range),
			// but clear the device so that it no longer appears on a refreshed dropdown (if the device isn't truly unplugged, it will show up in the next scan anyway)
			disconnectStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
			disconnectStatus.message = "Connection to device was lost";
		}
		onStatus.Invoke(disconnectStatus);
	}

	//Deal with the received Data
	public void OnData(Byte[] data) {
		// refresh timeout window
		if(this._timeout <= 0){
			Debug.Log("Connection to ANT+ device " + this._connectedDevice + " was re-established");
		}
		this._timeout = TIMEOUT_SECONDS;
		try{
			this._heartRate = (data[7]);
		}catch(Exception e){
			Debug.LogError("Error parsing ANT+ Device Data: " + System.Text.Encoding.UTF8.GetString(data, 1, data.Length - 1) + " -> " + e);
		}
	}

	private void Update() {
		if (this._timeout > 0) {
			this._timeout = this._timeout - Time.deltaTime;
			if (this._timeout <= 0) {
				DisconnectFromDevice(
					this._connectedDevice.onStatusChange != null
					? this._connectedDevice.onStatusChange
					: (s) => { },
				true);
			}
		}else{
			// if(this._connectedDevice.device != null){
			// 	Debug.Log("Attempting to reconnect to HRM device...");
			// 	ConnectToDevice(this._connectedDevice.device, this._connectedDevice.onTimeout);
			// }
		}
	}

	void OnChannelResponse(ANT_Response response) {

	}

	public override void Initialize() {
		try{
			AntManager.Instance.Init();
			AntManager.Instance.onSerialError += OnSerialError;
			this._backgroundScanChannel = AntManager.Instance.OpenBackgroundScanChannel(0);
			this._backgroundScanChannel.onReceiveData += OnReceivedBackgroundScanData;
		}catch(Exception e){
			Debug.LogError("Critical ANT+ error: " + e);
		}
	}

	private void OnSerialError(SerialError error) {
		Debug.LogError("ANT+ serial error: " + error.error);
		if (this._connectedDevice.onStatusChange != null) {
			Action<HttpUtils.ConnectionStatus> onError = this._connectedDevice.onStatusChange;
			DisconnectFromDevice(onError);
			HttpUtils.ConnectionStatus errorStatus = new HttpUtils.ConnectionStatus();
			errorStatus.message = "Serial error: " + error.error;
			errorStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
			onError.Invoke(errorStatus);
		}
	}

	private struct ConnectionData {
		public AntDevice device;
		public AntChannel channel;
		public Action<HttpUtils.ConnectionStatus> onStatusChange;
		public ConnectionData(AntDevice device, AntChannel channel, Action<HttpUtils.ConnectionStatus> onStatusChange) {
			this.device = device;
			this.channel = channel;
			this.onStatusChange = onStatusChange;
		}

		public override string ToString() {
			return device != null ? device.name : "NO_DEVICE";
		}
	}
}
