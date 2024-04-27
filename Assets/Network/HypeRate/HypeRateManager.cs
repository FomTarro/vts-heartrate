using UnityEngine;
using VTS.Core;
using VTS.Unity;

public class HypeRateManager : Singleton<HypeRateManager>
{

	private IWebSocket _socket = new WebSocketSharpImpl(new UnityVTSLoggerImpl());
	private const string HYPERATE_SOCKET_URL = @"wss://app.hyperate.io/socket/websocket?token={0}";
	// "internal-testing" can be used to get random spoof data/test the pipes.
	private string _hyperateID = null;
	public string HypeRateID { get { return this._hyperateID; } }
	private string KEEP_ALIVE_MESSAGE;

	private float _timeout = 0f;
	private const float TIMEOUT_MAX = 10f;

	[SerializeField]
	private int _heartrate = 0;
	public int Heartrate { get { return this._heartrate; } }

	private System.Action<string> _onError;
	private System.Action _onData;

	public override void Initialize()
	{
		HypeRateMessage message = new HypeRateMessage("phoenix");
		message.@event = "heartbeat";
		// {\"topic\": \"phoenix\",\"event\": \"heartbeat\",\"payload\": {},\"ref\": 0}
		this.KEEP_ALIVE_MESSAGE = JsonUtility.ToJson(message);
	}

	public void SetHyperateID(string id)
	{
		this._hyperateID = id;
	}

	public void Connect(System.Action<HttpUtils.ConnectionStatus> onStatus)
	{
		Disconnect(onStatus);
		HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
		status.status = HttpUtils.ConnectionStatus.Status.CONNECTING;
		onStatus.Invoke(status);
		this._timeout = TIMEOUT_MAX;

		this._onError = (message) =>
		{
			Debug.LogError("An error occured while connecting to the HypeRate socket...");
			HttpUtils.ConnectionStatus errorStatus = new HttpUtils.ConnectionStatus();
			errorStatus.status = HttpUtils.ConnectionStatus.Status.ERROR;
			errorStatus.message = message;
			onStatus.Invoke(errorStatus);
			if (!this._socket.IsConnectionOpen())
			{
				Debug.Log("Attempting to reconnect to HypeRate socket...");
				Connect(onStatus);
			}
		};
		this._onData = () =>
		{
			HttpUtils.ConnectionStatus connectedStatus = new HttpUtils.ConnectionStatus();
			connectedStatus.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
			onStatus.Invoke(connectedStatus);
		};

		this._socket = new WebSocketSharpImpl(new UnityVTSLoggerImpl());
		this._socket.Start(string.Format(HYPERATE_SOCKET_URL, HypeRateCredentials.API_KEY),
		() =>
		{
			Debug.Log("Connected to HypeRate socket");
			string send = JsonUtility.ToJson(new HypeRateMessage("hr:" + this._hyperateID));
			Debug.Log(send);
			this._socket.Send(send);
			this._timeout = TIMEOUT_MAX;
			// this._socket.Send("{\"topic\": \"hr:"+_hyperateID+"\", \"event\": \"phx_join\", \"payload\": {}, \"ref\": 0}");
			HttpUtils.ConnectionStatus connectedStatus = new HttpUtils.ConnectionStatus();
			connectedStatus.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
			onStatus.Invoke(connectedStatus);
		},
		() =>
		{
			this._timeout = 0f;
			Debug.Log("Disconnected from the HypeRate socket");
			HttpUtils.ConnectionStatus disconnectedStatus = new HttpUtils.ConnectionStatus();
			disconnectedStatus.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
			onStatus.Invoke(disconnectedStatus);
		},
		(ex) => { this._onError.Invoke(ex.ToString()); });
	}

	public void Disconnect(System.Action<HttpUtils.ConnectionStatus> onStatus)
	{
		this._timeout = 0f;
		if (this._socket.IsConnecting() || this._socket.IsConnectionOpen())
		{
			this._socket.Stop();
		}
		HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
		status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
		onStatus.Invoke(status);
	}

	private void FixedUpdate()
	{
		this._socket.Tick(Time.deltaTime);
		if (this._socket.IsConnectionOpen())
		{
			string response = this._socket.GetNextResponse();
			HypeRateMessage message = JsonUtility.FromJson<HypeRateMessage>(response);
			if (message != null)
			{
				if (message.@event.Equals("hr_update"))
				{
					this._timeout = TIMEOUT_MAX;
					this._heartrate = message.payload.hr;
					this._onData.Invoke();
				}
				else if (message.@event.Equals("phx_reply"))
				{
					// {"event":"phx_reply","payload":{"response":{},"status":"ok"},"ref":0,"topic":"phoenix"}
					if (message.payload != null && message.payload.status.Equals("ok"))
					{
						// all good
						this._timeout = TIMEOUT_MAX;
						this._onData.Invoke();
					}
					else
					{
						Debug.LogError("Error from HypeRate socket with status: " + message.payload.status + " and error: " + message.payload.response);
						if (this._onError != null)
						{
							this._onError.Invoke("error: " + message.payload.response);
						}
					}
				}
				else
				{
					// error of some kind
					Debug.LogError("Error from HypeRate socket with status: " + message.payload.status + " and error: " + message.payload.response);
					if (this._onError != null)
					{
						this._onError.Invoke("error: " + message.payload.response);
					}
				}
			}
			this._socket.Send(KEEP_ALIVE_MESSAGE);
		}
		if (this._timeout > 0f)
		{
			this._timeout = this._timeout - Time.deltaTime;
			if (this._timeout <= 0f && this._onError != null)
			{
				if (this._socket.IsConnectionOpen())
				{
					// if we don't get data packets and the connection is open, probably a bad ID
					this._onError.Invoke("error: invalid User ID");
				}
				else
				{
					// if we don't get data packets because the socket is closed, that's a timeout
					this._onError.Invoke("error: connection timed out");
				}
			}
		}
	}

	private void OnApplicationQuit()
	{
		if (this._socket != null)
		{
			this._socket.Stop();
		}
	}

	[System.Serializable]
	public class HypeRateMessage
	{
		public string topic = "";
		public string @event = "phx_join"; // lmao don't name your variables after reserved keywords in your API schema, please!
		public HypeRatePayload payload = new HypeRatePayload();
		public int @ref = 0;

		public HypeRateMessage(string topic)
		{
			this.topic = topic;
		}

		[System.Serializable]
		public class HypeRatePayload
		{
			public int hr;
			public string response;
			public string status;
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this);
		}
	}

}
