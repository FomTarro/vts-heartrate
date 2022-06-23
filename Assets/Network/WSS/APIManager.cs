using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using VTS.Networking.Impl;

public class APIManager : Singleton<APIManager>
{
    private WebSocketServer _server = null;

    // data api
    private APIEndpoint _dataService = new APIEndpoint("/data");
    public APIEndpoint DataEndpoint { get { return this._dataService; } }
    // event api
    private APIEndpoint _eventsService = new APIEndpoint("/events");
    public APIEndpoint EventsEndpoint { get { return this._eventsService; } }
    // input api
    private APIEndpoint _inputService = new APIEndpoint("/input");
    public APIEndpoint InputEndpoint { get { return this._inputService; } }

    private int _heartrate = 0;
    public int Heartrate { get { return this._heartrate; } }

    private int _port;
    public int Port { get { return this._port; } }

    private Dictionary<string, string> _tokenToSessionMap = new Dictionary<string, string>();

    public override void Initialize()
    {
        
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
            this._dataService.Start<DataService>(this._server);
            this._eventsService.Start<EventsService>(this._server);
            this._inputService.Start<InputService>(this._server);
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
    public void SendData(){
        DataMessage data = new DataMessage();
        data.data.heartrate = HeartrateManager.Instance.Plugin.HeartRate;
        Dictionary<string, float> paramMap = HeartrateManager.Instance.Plugin.ParameterMap;
        data.data.vts_heartrate_bpm = GetValueFromDictionary(paramMap, "VTS_Heartrate_BPM");
        data.data.vts_heartrate_pulse = GetValueFromDictionary(paramMap, "VTS_Heartrate_Pulse");
        data.data.vts_heartrate_breath = GetValueFromDictionary(paramMap, "VTS_Heartrate_Breath");
        data.data.vts_heartrate_linear = GetValueFromDictionary(paramMap, "VTS_Heartrate_Linear");
        this._dataService.SendToAll(JsonUtility.ToJson(data));
    }

    public void SendEvent(EventMessage message){
        this._eventsService.SendToAll(JsonUtility.ToJson(message));
    }

    private float GetValueFromDictionary(Dictionary<string, float> dict, string key){
        return dict.ContainsKey(key) ? dict[key] : 0.0f;
    }

    private void SaveTokenData(){
        // TokenSaveData data = new TokenSaveData();
    }

    private void LoadTokenData(){

    }

    [System.Serializable]
    public class TokenSaveData{
        public List<string> tokens = new List<string>();
        public TokenSaveData(IEnumerable<string> tokens){

        }
    }

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

        public Statistics Stats { get { 
            return new Statistics(
            this.ClientCount, 
            this.MessageCount, 
            this._server != null ? "ws://localhost:" + this._server.Port + this._path : "Unavailable"); } }

        public APIEndpoint(string path){
            this._path = path;
        }

        public void Start<T>(WebSocketServer server) where T : WebSocketService, new(){
            this._server = server;
            this._server.RemoveWebSocketService(this._path);
            this._server.AddWebSocketService<T>(this._path);
            this._messages = 0;
        }

        public bool SendToAll(string message){
            WebSocketServiceHost host;
            if(this._server != null 
            && this._server.WebSocketServices.TryGetServiceHost(this._path, out host)){
                host.Sessions.Broadcast(JsonUtility.ToJson(message));
                this._messages = this._messages + host.Sessions.Count;
                return true;
            }
            return false;
        }

        public bool SendToID(string message, string sessionID){
            WebSocketServiceHost host;
            if(this._server != null 
            && this._server.WebSocketServices.TryGetServiceHost(this._path, out host)){
                try{
                    host.Sessions.SendTo(sessionID, message);
                    this._messages = this._messages + 1;
                    return true;
                }catch(System.Exception e){
                    Debug.LogErrorFormat("Error sending message to session ID: {0}, {1}", sessionID, e);
                }
            }
            return false;
        }

        public void Receive(string message, string sessionID){
            try{
                APIMessage apiMessage = JsonUtility.FromJson<APIMessage>(message);
                if("InputRequest".Equals(apiMessage.messageType)){
                    if(APIManager.Instance._tokenToSessionMap.ContainsValue(sessionID)){
                        InputMessage input = JsonUtility.FromJson<InputMessage>(message);
                        Debug.Log(input.data.heartrate);
                        APIManager.Instance._heartrate = input.data.heartrate;
                    }else{
                        ErrorMessage error = new ErrorMessage();
                        error.data.message = string.Format("Client is not authenticated.", apiMessage.messageType);
                        error.data.errorCode = ErrorMessage.StatusCode.FORBIDDEN;
                        this.SendToID(JsonUtility.ToJson(error), sessionID);
                    }
                }else if("AuthenticationRequest".Equals(apiMessage.messageType)){
                    AuthenticationMessage authRequest = JsonUtility.FromJson<AuthenticationMessage>(message);
                    if(authRequest.data.token != null 
                    && APIManager.Instance._tokenToSessionMap.ContainsKey(authRequest.data.token)){
                        // they send us a token
                        APIManager.Instance._tokenToSessionMap.Remove(authRequest.data.token);
                        APIManager.Instance._tokenToSessionMap.Add(authRequest.data.token, sessionID);
                        Debug.LogFormat("Authenticating session ID {0}", sessionID);

                        AuthenticationMessage authResponse = new AuthenticationMessage();
                        authResponse.messageType = "AuthenticationResponse";
                        authResponse.data.token = authRequest.data.token;
                        authResponse.data.pluginAuthor = authRequest.data.pluginAuthor;
                        authResponse.data.pluginName = authRequest.data.pluginName;
                        authResponse.data.authenticated = true;

                        // TODO: write authentication file
                    }else if(authRequest.data.pluginAuthor != null && authRequest.data.pluginName != null){
                        // they are requesting a new token
                        Dictionary<string, string> strings = new Dictionary<string, string>();
                        strings.Add("settings_api_server_approve_plugin_tooltip_populated", 
                            string.Format(Localization.LocalizationManager.Instance.GetString("settings_api_server_approve_plugin_tooltip"), 
                                authRequest.data.pluginName, 
                                authRequest.data.pluginAuthor));
                        Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
                        UIManager.Instance.ShowPopUp(
                            "settings_api_server_approve_plugin_title",
                            "settings_api_server_approve_plugin_tooltip_populated",
                            new PopUp.PopUpOption(
                                "settings_api_server_button_approve",
                                true,
                                () => {
                                    AuthenticationMessage authResponse = new AuthenticationMessage();
                                    authResponse.messageType = "AuthenticationResponse";
                                    authResponse.data.token = System.Guid.NewGuid().ToString();
                                    authResponse.data.pluginAuthor = authRequest.data.pluginAuthor;
                                    authResponse.data.pluginName = authRequest.data.pluginName;
                                    APIManager.Instance._tokenToSessionMap.Add(authResponse.data.token, sessionID);
                                    this.SendToID(JsonUtility.ToJson(authResponse), sessionID);
                                    UIManager.Instance.HidePopUp();
                                }),
                            new PopUp.PopUpOption(
                                "settings_api_server_button_deny",
                                false,
                                () => { 
                                    ErrorMessage error = new ErrorMessage();
                                    error.data.message = "User had denied this authentication request.";
                                    error.data.errorCode = ErrorMessage.StatusCode.FORBIDDEN;
                                    this.SendToID(JsonUtility.ToJson(error), sessionID);
                                    UIManager.Instance.HidePopUp();
                                })
                        );
                    }else{
                        ErrorMessage error = new ErrorMessage();
                        error.data.message = "Message data must contain pluginName and pluginAuthor.";
                        error.data.errorCode = ErrorMessage.StatusCode.BAD_REQUEST;
                        this.SendToID(JsonUtility.ToJson(error), sessionID);
                    }
                }else{
                    ErrorMessage error = new ErrorMessage();
                    error.data.message = string.Format("Message type of {0} is not recognized.", apiMessage.messageType);
                    error.data.errorCode = ErrorMessage.StatusCode.BAD_REQUEST;
                    this.SendToID(JsonUtility.ToJson(error), sessionID);
                }
                this._messages = this._messages + 1;
            }catch(System.Exception ex){
                Debug.LogErrorFormat("Error parsing incoming socket message on path {0}: {1}, {2}",
                this._path, message, ex);
            }
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

    #region Data Classes

    [System.Serializable]
    public class APIMessage {
        public string apiVersion = "1.0";
        public string messageType = "APIMessage";
        public long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public class DataService : WebSocketService {
        protected override void OnOpen(){
            Debug.Log("Connection established to Data API...");
        }
    }

    public class DataMessage : APIMessage {

        public Data data = new Data();
        public DataMessage(){
            this.messageType = "DataResponse";
            this.data = new Data();
        }

        [System.Serializable]
        public class Data{
            public float heartrate;
            public float vts_heartrate_bpm;
            public float vts_heartrate_pulse;
            public float vts_heartrate_breath;
            public float vts_heartrate_linear;
        }
    }

    #endregion

    #region Event Classes
    public class EventsService : WebSocketService {
        protected override void OnOpen(){
            Debug.Log("Connection established to Event API...");
        }
    }

    public abstract class EventMessage : APIMessage {

    }

    public class ExpressionEventMessage : EventMessage {

        public Data data = new Data();
        public ExpressionEventMessage(
            int threshold, 
            string expression,
            ExpressionModule.TriggerBehavior behavior,
            bool activated){
            this.messageType = "ExpressionEventResponse";
            this.data = new Data();
            this.data.threshold = threshold;
            this.data.expression = expression;
            this.data.behavior = MapBehavior(behavior);
            this.data.activated = activated;
        }

        private ExpressionTriggerBehavior MapBehavior(ExpressionModule.TriggerBehavior behavior){
            switch(behavior){
                case ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE:
                    return ExpressionTriggerBehavior.ACTIVATE_ABOVE;
                case ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW:
                    return ExpressionTriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW;
                case ExpressionModule.TriggerBehavior.ACTIVATE_BELOW:
                    return ExpressionTriggerBehavior.ACTIVATE_BELOW;
                case ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE:
                    return ExpressionTriggerBehavior.DEACTIVATE_ABOVE;
                case ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW:
                    return ExpressionTriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW;
                case ExpressionModule.TriggerBehavior.DEACTIVATE_BELOW:
                    return ExpressionTriggerBehavior.DEACTIVATE_BELOW;
            }
            return ExpressionTriggerBehavior.UNKNOWN;
        }

        [System.Serializable]
        public class Data {
            public int threshold;
            public string expression;
            public ExpressionTriggerBehavior behavior;
            public bool activated;
        }

        [System.Serializable]
        public enum ExpressionTriggerBehavior : int {   
            UNKNOWN = -1,     
            ACTIVATE_ABOVE_DEACTIVATE_BELOW = 0,
            DEACTIVATE_ABOVE_ACTIVATE_BELOW = 1,
            ACTIVATE_ABOVE = 2,
            DEACTIVATE_ABOVE = 3,
            ACTIVATE_BELOW = 4,
            DEACTIVATE_BELOW = 5,
        }
    }

    public class HotkeyEventMessage : EventMessage {
        public Data data = new Data();
        public HotkeyEventMessage(
            int threshold, 
            string hotkey,
            HotkeyModule.TriggerBehavior behavior
            ){
            this.messageType = "HotkeyEventResponse";
            this.data = new Data();
            this.data.threshold = threshold;
            this.data.hotkey = hotkey;
            this.data.behavior = MapBehavior(behavior);
        }

        private HotkeyTriggerBehavior MapBehavior(HotkeyModule.TriggerBehavior behavior){
            switch(behavior){
                case HotkeyModule.TriggerBehavior.ACTIVATE_ABOVE:
                    return HotkeyTriggerBehavior.ACTIVATE_ABOVE;
                case HotkeyModule.TriggerBehavior.ACTIVATE_BELOW:
                    return HotkeyTriggerBehavior.ACTIVATE_BELOW;
                case HotkeyModule.TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW:
                    return HotkeyTriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW;
            }
            return HotkeyTriggerBehavior.UNKNOWN;
        }

        [System.Serializable]
        public class Data {
            public int threshold;
            public string hotkey;
            public HotkeyTriggerBehavior behavior;
        }

        [System.Serializable]
        public enum HotkeyTriggerBehavior : int {   
            UNKNOWN = -1,     
            ACTIVATE_ABOVE_ACTIVATE_BELOW = 0,
            ACTIVATE_ABOVE = 1,
            ACTIVATE_BELOW = 2,
        }

    }

    #endregion

    #region Input Classes
    public class InputService : WebSocketService {

        protected override void OnOpen(){
            Debug.Log("Connection established to Input API...");
        }
        protected override void OnMessage(MessageEventArgs e){
            MainThreadUtil.Run(() => { APIManager.Instance.InputEndpoint.Receive(e.Data, this.ID); });
        }
    }

    public class InputMessage : APIMessage {
        public Data data = new Data();
        public InputMessage(){
            this.messageType = "InputResponse";
            this.data = new Data();
        }

        [System.Serializable]
        public class Data{
            public int heartrate;
        }
    }

    public class AuthenticationMessage : APIMessage {
        public Data data = new Data();
        public AuthenticationMessage(){
            this.messageType = "AuthenticationResponse";
            this.data = new Data();
        }

        [System.Serializable]
        public class Data{
            public string pluginName;
            public string pluginAuthor;
            public string token;
            public bool authenticated = false;
        }
    }

    public class ErrorMessage : APIMessage {
        public Data data = new Data();
        public ErrorMessage(){
            this.messageType = "ErrorResponse";
            this.data = new Data();
        }

        [System.Serializable]
        public class Data{
            public StatusCode errorCode;
            public string message;
        }

        [System.Serializable]
        public enum StatusCode : int {
            OK = 200,
            BAD_REQUEST = 400,
            FORBIDDEN = 403,
            SERVER_ERROR = 500,
        }
    }

    #endregion
}
    
