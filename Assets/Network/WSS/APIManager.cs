using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using VTS.Networking.Impl;

public class APIManager : Singleton<APIManager>
{
    private WebSocketServer _server = null;

    // data api
    private APIEndpoint _dataService = new APIEndpoint("/data", () => {}, (a, b) => {});
    public APIEndpoint DataEndpoint { get { return this._dataService; } }
    // event api
    private APIEndpoint _eventsService = new APIEndpoint("/events", () => {}, (a, b) => {});
    public APIEndpoint EventsEndpoint { get { return this._eventsService; } }
    // input api
    private APIEndpoint _inputService = new APIEndpoint("/input",  () => {}, (a, b) => { 
        APIManager.Instance.ProcessInput(APIManager.Instance._inputService, a, b);
    });
    public APIEndpoint InputEndpoint { get { return this._inputService; } }

    private int _heartrate = 0;
    public int Heartrate { get { return this._heartrate; } }

    private int _port;
    public int Port { get { return this._port; } }

    private Dictionary<string, PluginSaveData> _tokenToSessionMap = new Dictionary<string, PluginSaveData>();
    public List<PluginSaveData> ApprovedPlugins { get { return new List<PluginSaveData>(this._tokenToSessionMap.Values); } }

    public override void Initialize(){
        FromTokenSaveData(SaveDataManager.Instance.ReadTokenSaveData());
    }

    private void OnApplicationQuit(){
        Stop((s) => {});
    }

    public void SetPort(int port){
        this._port = port;
    }

    public void Stop(System.Action<HttpUtils.ConnectionStatus> onStatus){
        if(this._server != null){
            this._server.Stop();
            HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
            status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
            onStatus.Invoke(status);
            Debug.Log("API Server stopped.");
        }
    }

    public void StartOnPort(int port, System.Action<HttpUtils.ConnectionStatus> onStatus){
        Stop(onStatus);
        try{
            this._server = new WebSocketServer(port, false);
            this._dataService.Start(this._server);
            this._eventsService.Start(this._server);
            this._inputService.Start(this._server);
            this._server.Start();
            HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
            status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
            onStatus.Invoke(status);
            Debug.LogFormat("API Server started on port {0}.", port);
        }catch(System.Exception e){
            Debug.LogError(e);
            HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
            status.status = HttpUtils.ConnectionStatus.Status.ERROR;
            status.message = e.Message;
            onStatus.Invoke(status);
        }
    }
    public void SendData(DataMessage dataMessage){
        this._dataService.SendToAll(dataMessage);
    }

    public void SendEvent(EventMessage message){
        this._eventsService.SendToAll(message);
    }

    protected void ProcessInput(APIEndpoint endpoint, string message, string sessionID){
        try{
            APIMessage apiMessage = JsonUtility.FromJson<APIMessage>(message);
            PluginSaveData authenticatedClient = APIManager.Instance.PluginDataFromSessionID(sessionID);
            if("InputRequest".Equals(apiMessage.messageType)){
                InputMessage inputRequest = JsonUtility.FromJson<InputMessage>(message);
                if(authenticatedClient != null && authenticatedClient.authenticated == true){
                    APIManager.Instance._heartrate = inputRequest.data.heartrate;
                    // echo the request back as confirmation
                    inputRequest.messageType = "InputResponse";
                    endpoint.SendToID(inputRequest, sessionID);
                }else{
                    ErrorMessage errorResponse = new ErrorMessage(
                        ErrorMessage.StatusCode.FORBIDDEN,
                        string.Format("Client is not authenticated.", apiMessage.messageType)
                    );
                    endpoint.SendToID(errorResponse, sessionID);
                }
            }else if("AuthenticationRequest".Equals(apiMessage.messageType)){
                AuthenticationMessage authRequest = JsonUtility.FromJson<AuthenticationMessage>(message);
                if(authRequest.data.token != null 
                && APIManager.Instance._tokenToSessionMap.ContainsKey(authRequest.data.token)){
                    // they send us a token that has been approved by the user
                    PluginSaveData existingPluginData = APIManager.Instance._tokenToSessionMap[authRequest.data.token];
                    APIManager.Instance._tokenToSessionMap.Remove(authRequest.data.token);
                    PluginSaveData pluginData = new PluginSaveData(
                        authRequest.data.token, 
                        sessionID, 
                        authRequest.data.pluginName != null ? authRequest.data.pluginName :existingPluginData.pluginName, 
                        authRequest.data.pluginAuthor != null ? authRequest.data.pluginAuthor :existingPluginData.pluginAuthor,
                        authRequest.data.pluginAbout != null ? authRequest.data.pluginAbout :existingPluginData.pluginAbout,
                        true); // flag this plugin as authenticated = true
                    APIManager.Instance._tokenToSessionMap.Add(authRequest.data.token, pluginData);
                    Debug.LogFormat("Authenticating session ID {0}", sessionID);

                    AuthenticationMessage authResponse = new AuthenticationMessage(
                        authRequest.data.pluginName, 
                        authRequest.data.pluginAuthor, 
                        authRequest.data.pluginAbout,
                        authRequest.data.token,
                        true);

                    endpoint.SendToID(authResponse, sessionID);
                    SaveDataManager.Instance.WriteTokenSaveData(APIManager.Instance.ToTokenSaveData());
                }else if(authRequest.data.pluginAuthor != null && authRequest.data.pluginName != null){
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
                                    sessionID, 
                                    authResponse.data.pluginName, 
                                    authResponse.data.pluginAuthor,
                                    authRequest.data.pluginAbout,
                                    false);
                                APIManager.Instance._tokenToSessionMap.Add(authResponse.data.token, data);
                                endpoint.SendToID(authResponse, sessionID);
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
                                endpoint.SendToID(errorResponse, sessionID);
                                UIManager.Instance.HidePopUp();
                            })
                    );
                }else{
                    // Malformed Authentication Request
                    ErrorMessage errorResponse = new ErrorMessage(
                        ErrorMessage.StatusCode.BAD_REQUEST,
                        "Message data must contain pluginName and pluginAuthor."
                    );
                    endpoint.SendToID(errorResponse, sessionID);
                }
            }else{
                // Unknown Message Type
                ErrorMessage errorMessage = new ErrorMessage(
                    ErrorMessage.StatusCode.BAD_REQUEST,
                    string.Format("Message type of {0} is not recognized.", apiMessage.messageType)
                );
                endpoint.SendToID(errorMessage, sessionID);
            }
        }catch(System.Exception ex){
            Debug.LogErrorFormat("Error parsing incoming socket message on path {0}: {1}, {2}",
            endpoint.Path, message, ex);
        }
    }

    #region Data Serialization

    private PluginSaveData PluginDataFromSessionID(string sessionID){
        foreach(PluginSaveData data in this._tokenToSessionMap.Values){
            if(data.sessionID.Equals(sessionID)){
                return data;
            }
        }
        return null;
    }

    public void RevokeTokenData(string token){
        if(this._tokenToSessionMap.ContainsKey(token)){
            this._tokenToSessionMap.Remove(token);
            SaveDataManager.Instance.WriteTokenSaveData(ToTokenSaveData());
        }
    }

    private TokenSaveData ToTokenSaveData(){
        TokenSaveData data = new TokenSaveData(this._tokenToSessionMap.Values);
        return data;
    }

    private void FromTokenSaveData(TokenSaveData data){
        foreach(PluginSaveData p in data.tokens){
            p.authenticated = false; // this is important!
            this._tokenToSessionMap.Add(p.token, p);
        }
    }

    [System.Serializable]
    public class TokenSaveData{
        public List<PluginSaveData> tokens = new List<PluginSaveData>();

        public TokenSaveData(){
            this.tokens = new List<PluginSaveData>();
        }
        public TokenSaveData(IEnumerable<PluginSaveData> data){
            this.tokens = new List<PluginSaveData>(data);
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [System.Serializable]
    public class PluginSaveData{
        public string token;
        public string sessionID;
        public string pluginName;
        public string pluginAuthor;
        public string pluginAbout;
        public bool authenticated = false;
        public PluginSaveData(string token, string sessionID, string pluginName, string pluginAuthor, string pluginAbout, bool authenticated){
            this.token = token;
            this.sessionID = sessionID;
            this.pluginName = pluginName;
            this.pluginAuthor = pluginAuthor;
            this.pluginAbout = pluginAbout != null && pluginAbout.Trim().Length > 512 ? pluginAbout.Trim().Substring(0, 512) : pluginAbout;
            this.authenticated = authenticated;
        }
    }

    #endregion

    #region Endpoints

    public class APIEndpoint {
        protected string _path  = "/input";
        public string Path { get { return this._path; } }
        public int ClientCount { get { 
            WebSocketServiceHost host;
            if(this._server != null 
            && this._server.WebSocketServices.TryGetServiceHost(this._path, out host)){
                return host.Sessions.Count;
            }
            return 0;
        } }
        protected long _messages = 0;
        public long MessageCount { get { return this._messages; }}

        private WebSocketServer _server = null;
        private APIService _service = null;

        public Statistics Stats { get { 
            return new Statistics(
                this.ClientCount, 
                this.MessageCount, 
                this._server != null ? "ws://localhost:" + this._server.Port + this._path : "Unavailable"); } }

        public APIEndpoint(string path, System.Action onOpen, System.Action<string, string> onMessage){
            
            this._path = path;
            this._service = new APIService(path, onOpen, 
            (a, b) => { 
                onMessage(a, b);                         
                this._messages = this._messages + 1;
            } );
        }

        public void Start(WebSocketServer server){
            this._server = server;
            this._server.RemoveWebSocketService(this._path);
            this._server.AddWebSocketService<APIService>(this._path, () => { return this._service; });
            this._messages = 0;
        }

        public bool SendToAll(APIMessage message){
            WebSocketServiceHost host;
            if(this._server != null 
            && this._server.WebSocketServices.TryGetServiceHost(this._path, out host)){
                host.Sessions.Broadcast(message.ToString());
                this._messages = this._messages + host.Sessions.Count;
                return true;
            }
            return false;
        }

        public bool SendToID(APIMessage message, string sessionID){
            WebSocketServiceHost host;
            if(this._server != null 
            && this._server.WebSocketServices.TryGetServiceHost(this._path, out host)){
                try{
                    host.Sessions.SendTo(sessionID, message.ToString());
                    this._messages = this._messages + 1;
                    return true;
                }catch(System.Exception e){
                    Debug.LogErrorFormat("Error sending message to session ID: {0}, {1}", sessionID, e);
                }
            }
            return false;
        }

        public struct Statistics {
            public int clients;
            public long messages;
            public string url;

            public Statistics(int clients, long messages, string url){
                this.clients = clients;
                this.messages = messages;
                this.url = url;
            }
        }
    }

    public class APIService : WebSocketService {

        private string _path;
        private System.Action _onOpen;
        private System.Action<string, string> _onMessage;

        public APIService(string path, System.Action onOpen, System.Action<string, string> onMessage){
            this._path = path;
            this._onOpen = onOpen;
            this._onMessage = onMessage;
        }

        protected override void OnOpen(){
            Debug.LogFormat("Connection established to {0} API...", this._path);
            MainThreadUtil.Run(() => { this._onOpen();});
        }
        protected override void OnMessage(MessageEventArgs e){
            MainThreadUtil.Run(() => { this._onMessage(e.Data, this.ID); });
        }

        protected override void OnClose(CloseEventArgs e){
           Debug.LogFormat("Closing {0} API...", this._path);
        }
    }

    #endregion
}
    
