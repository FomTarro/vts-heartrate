using System.Collections.Generic;
using UnityEngine;

public class APIManager : Singleton<APIManager> {

	[SerializeField]
	private WebSocketServer _server = null;

	private int _heartrate = 0;
	public int Heartrate { get { return this._heartrate; } }

	public int Port { get { return this._server ? this._server.Port : 8214; } }

	private Dictionary<string, PluginSaveData> _tokenToSessionMap = new Dictionary<string, PluginSaveData>();
	public List<PluginSaveData> ApprovedPlugins { get { return new List<PluginSaveData>(this._tokenToSessionMap.Values); } }

	[SerializeField]
	private BaseUnityEndpoint _dataService = null;
	[SerializeField]
	private BaseUnityEndpoint _eventsService = null;
	[SerializeField]
	private BaseUnityEndpoint _inputService = null;

	public override void Initialize() {
		FromTokenSaveData(SaveDataManager.Instance.ReadTokenSaveData());
	}

	private void OnApplicationQuit() {
		Stop((s) => { });
	}

	public void SetPort(int port) {
		this._server.SetPort(port);
	}

	public void Stop(System.Action<HttpUtils.ConnectionStatus> onStatus) {
		if (this._server != null) {
			this._server.StopServer();
			HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
			status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
			status.message = Localization.LocalizationManager.Instance.GetString("settings_api_server_stopped");
			onStatus.Invoke(status);
			Debug.Log("API Server stopped.");
		}
	}

	public void StartOnPort(int port, System.Action<HttpUtils.ConnectionStatus> onStatus) {
		Stop(onStatus);
		try {
			this._server.StartServer();
			HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
			status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
			status.message = Localization.LocalizationManager.Instance.GetString("settings_api_server_started");
			onStatus.Invoke(status);
			Debug.LogFormat("API Server started on port {0}.", port);
		}
		catch (System.Exception e) {
			Debug.LogError(e);
			HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
			status.status = HttpUtils.ConnectionStatus.Status.ERROR;
			status.message = e.Message;
			onStatus.Invoke(status);
		}
	}
	
	#region Statistics
	
	public WebSocketServer.EndpointStatistics GetDataStatistics(){
		return this._server.Statistics[this._dataService];
	}

	public WebSocketServer.EndpointStatistics GetEventStatistics(){
		return this._server.Statistics[this._eventsService];
	}

	public WebSocketServer.EndpointStatistics GetInputStatistics(){
		return this._server.Statistics[this._inputService];
	}

	#endregion

	#region Sending

	public void SendData(DataMessage dataMessage) {
		Debug.Log(this._server);
		this._server.SendToAll(JsonUtility.ToJson(dataMessage), this._dataService);
	}

	public void SendEvent(EventMessage eventMessage) {
		this._server.SendToAll(JsonUtility.ToJson(eventMessage), this._eventsService);
	}

	public void ProcessInput(string message, string clientID) {
		try {
			APIMessage apiMessage = JsonUtility.FromJson<APIMessage>(message);
			PluginSaveData authenticatedClient = APIManager.Instance.PluginDataFromSessionID(clientID);
			if ("InputRequest".Equals(apiMessage.messageType)) {
				InputMessage inputRequest = JsonUtility.FromJson<InputMessage>(message);
				if (authenticatedClient != null && authenticatedClient.authenticated == true) {
					APIManager.Instance._heartrate = inputRequest.data.heartrate;
					// echo the request back as confirmation
					inputRequest.messageType = "InputResponse";
					this._server.SendToClient(JsonUtility.ToJson(inputRequest), this._inputService, clientID);
				}
				else {
					ErrorMessage errorResponse = new ErrorMessage(
						ErrorMessage.StatusCode.FORBIDDEN,
						"Client is not authenticated."
					);
					this._server.SendToClient(JsonUtility.ToJson(errorResponse), this._inputService, clientID);
				}
			}
			else if ("AuthenticationRequest".Equals(apiMessage.messageType)) {
				AuthenticationMessage authRequest = JsonUtility.FromJson<AuthenticationMessage>(message);
				if (authRequest.data.token != null
				&& APIManager.Instance._tokenToSessionMap.ContainsKey(authRequest.data.token)) {
					// they send us a token that has been approved by the user
					PluginSaveData existingPluginData = APIManager.Instance._tokenToSessionMap[authRequest.data.token];
					APIManager.Instance._tokenToSessionMap.Remove(authRequest.data.token);
					PluginSaveData pluginData = new PluginSaveData(
						authRequest.data.token,
						clientID,
						authRequest.data.pluginName != null ? authRequest.data.pluginName : existingPluginData.pluginName,
						authRequest.data.pluginAuthor != null ? authRequest.data.pluginAuthor : existingPluginData.pluginAuthor,
						authRequest.data.pluginAbout != null ? authRequest.data.pluginAbout : existingPluginData.pluginAbout,
						true); // flag this plugin as authenticated = true
					APIManager.Instance._tokenToSessionMap.Add(authRequest.data.token, pluginData);
					Debug.LogFormat("Authenticating session ID: {0}", clientID);

					AuthenticationMessage authResponse = new AuthenticationMessage(
						authRequest.data.pluginName,
						authRequest.data.pluginAuthor,
						authRequest.data.pluginAbout,
						authRequest.data.token,
						true);

					this._server.SendToClient(JsonUtility.ToJson(authResponse), this._inputService, clientID);
					SaveDataManager.Instance.WriteTokenSaveData(APIManager.Instance.ToTokenSaveData());
				}
				else if (authRequest.data.pluginAuthor != null && authRequest.data.pluginName != null) {
					// they do not send us an approved token, but do include plugin name and plugin author
					Dictionary<string, string> strings = new Dictionary<string, string>();
					strings.Add("settings_api_server_approve_plugin_tooltip_populated",
						string.Format(Localization.LocalizationManager.Instance.GetString("settings_api_server_approve_plugin_tooltip"),
							authRequest.data.pluginName,
							authRequest.data.pluginAuthor));
					Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
					// ask user to approve plugin
					UIManager.Instance.ShowPopUp(
						"settings_api_server_approve_plugin_title",
						"settings_api_server_approve_plugin_tooltip_populated",
						new PopUp.PopUpOption(
							"settings_api_server_button_approve",
							ColorUtils.ColorPreset.GREEN,
							() => {
								// Respond with new token
								AuthenticationMessage authResponse = new AuthenticationMessage(
									authRequest.data.pluginName,
									authRequest.data.pluginAuthor,
									authRequest.data.pluginAbout,
									System.Guid.NewGuid().ToString(),
									false);

								// Store this token and relevant metadata
								PluginSaveData data = new PluginSaveData(authResponse.data.token,
									clientID,
									authResponse.data.pluginName,
									authResponse.data.pluginAuthor,
									authRequest.data.pluginAbout,
									false);
								APIManager.Instance._tokenToSessionMap.Add(authResponse.data.token, data);
								this._server.SendToClient(JsonUtility.ToJson(authResponse), this._inputService, clientID);
								SaveDataManager.Instance.WriteTokenSaveData(APIManager.Instance.ToTokenSaveData());
								UIManager.Instance.HidePopUp();
							}),
						new PopUp.PopUpOption(
							"settings_api_server_button_deny",
							ColorUtils.ColorPreset.RED,
							() => {
								ErrorMessage errorResponse = new ErrorMessage(
									ErrorMessage.StatusCode.FORBIDDEN,
									"User had denied this authentication request."
								);
								this._server.SendToClient(JsonUtility.ToJson(errorResponse), this._inputService, clientID);
								UIManager.Instance.HidePopUp();
							})
					);
				}
				else {
					// Malformed Authentication Request
					ErrorMessage errorResponse = new ErrorMessage(
						ErrorMessage.StatusCode.BAD_REQUEST,
						"Message data must contain pluginName and pluginAuthor."
					);
					this._server.SendToClient(JsonUtility.ToJson(errorResponse), this._inputService, clientID);
				}
			}
			else {
				// Unknown Message Type
				ErrorMessage errorResponse = new ErrorMessage(
					ErrorMessage.StatusCode.BAD_REQUEST,
					string.Format("Message type of {0} is not recognized.", apiMessage.messageType)
				);
				this._server.SendToClient(JsonUtility.ToJson(errorResponse), this._inputService, clientID);
			}
		}
		catch (System.Exception ex) {
			Debug.LogErrorFormat("Error parsing incoming socket message on path {0}: {1}, {2}",
			this._inputService.Path, message, ex);
		}
	}

	#endregion

	#region Data Serialization

	private PluginSaveData PluginDataFromSessionID(string sessionID) {
		foreach (PluginSaveData data in this._tokenToSessionMap.Values) {
			if (data.sessionID.Equals(sessionID)) {
				return data;
			}
		}
		return null;
	}

	public void RevokeTokenData(string token) {
		if (this._tokenToSessionMap.ContainsKey(token)) {
			this._tokenToSessionMap.Remove(token);
			SaveDataManager.Instance.WriteTokenSaveData(ToTokenSaveData());
		}
	}

	private TokenSaveData ToTokenSaveData() {
		TokenSaveData data = new TokenSaveData(this._tokenToSessionMap.Values);
		return data;
	}

	private void FromTokenSaveData(TokenSaveData data) {
		foreach (PluginSaveData p in data.tokens) {
			p.authenticated = false; // this is important!
			this._tokenToSessionMap.Add(p.token, p);
		}
	}

	[System.Serializable]
	public class TokenSaveData {
		public List<PluginSaveData> tokens = new List<PluginSaveData>();

		public TokenSaveData() {
			this.tokens = new List<PluginSaveData>();
		}
		public TokenSaveData(IEnumerable<PluginSaveData> data) {
			this.tokens = new List<PluginSaveData>(data);
		}

		public override string ToString() {
			return JsonUtility.ToJson(this);
		}
	}

	[System.Serializable]
	public class PluginSaveData {
		public string token;
		public string sessionID;
		public string pluginName;
		public string pluginAuthor;
		public string pluginAbout;
		public bool authenticated = false;
		public PluginSaveData(string token, string sessionID, string pluginName, string pluginAuthor, string pluginAbout, bool authenticated) {
			this.token = token;
			this.sessionID = sessionID;
			this.pluginName = pluginName;
			this.pluginAuthor = pluginAuthor;
			this.pluginAbout = pluginAbout != null && pluginAbout.Trim().Length > 512 ? pluginAbout.Trim().Substring(0, 512) : pluginAbout;
			this.authenticated = authenticated;
		}
	}

	#endregion

}

