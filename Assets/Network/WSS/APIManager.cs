﻿using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using VTS.Networking.Impl;
using System.IO;

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

    private Dictionary<string, PluginData> _tokenToSessionMap = new Dictionary<string, PluginData>();
    public List<PluginData> ApprovedPlugins { get { return new List<PluginData>(this._tokenToSessionMap.Values); } }
    private string GLOBAL_SAVE_PATH = "";

    public override void Initialize()
    {
        this.GLOBAL_SAVE_PATH = Path.Combine(Application.persistentDataPath, "plugins.json");
        LoadTokenData();
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

    private PluginData PluginDataFromSessionID(string sessionID){
        foreach(PluginData data in this._tokenToSessionMap.Values){
            if(data.sessionID.Equals(sessionID)){
                return data;
            }
        }
        return null;
    }

    public void RevokeTokenData(string token){
        if(this._tokenToSessionMap.ContainsKey(token)){
            this._tokenToSessionMap.Remove(token);
            SaveTokenData();
        }
    }

    private void SaveTokenData(){
        TokenSaveData data = new TokenSaveData(this._tokenToSessionMap.Values);
        File.WriteAllText(this.GLOBAL_SAVE_PATH, data.ToString());
    }

    private void LoadTokenData(){
        if(File.Exists(this.GLOBAL_SAVE_PATH)){
            string content = File.ReadAllText(this.GLOBAL_SAVE_PATH);
            TokenSaveData data = JsonUtility.FromJson<TokenSaveData>(content);
            foreach(PluginData p in data.tokens){
                this._tokenToSessionMap.Add(p.token, p);
            }
        }
    }

    [System.Serializable]
    public class TokenSaveData{
        public List<PluginData> tokens = new List<PluginData>();
        public TokenSaveData(IEnumerable<PluginData> data){
            this.tokens = new List<PluginData>(data);
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [System.Serializable]
    public class PluginData{
        public string token;
        public string sessionID;
        public string pluginName;
        public string pluginAuthor;
        public PluginData(string token, string sessionID, string pluginName, string pluginAuthor){
            this.token = token;
            this.sessionID = sessionID;
            this.pluginName = pluginName;
            this.pluginAuthor = pluginAuthor;
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
                PluginData client = APIManager.Instance.PluginDataFromSessionID(sessionID);
                if("InputRequest".Equals(apiMessage.messageType)){
                    if(client != null){
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
                        PluginData data = new PluginData(authRequest.data.token, sessionID, authRequest.data.pluginName, authRequest.data.pluginAuthor);
                        APIManager.Instance._tokenToSessionMap.Add(authRequest.data.token, data);
                        Debug.LogFormat("Authenticating session ID {0}", sessionID);

                        AuthenticationMessage authResponse = new AuthenticationMessage();
                        authResponse.messageType = "AuthenticationResponse";
                        authResponse.data.token = authRequest.data.token;
                        authResponse.data.pluginAuthor = authRequest.data.pluginAuthor;
                        authResponse.data.pluginName = authRequest.data.pluginName;
                        authResponse.data.authenticated = true;
                        this.SendToID(JsonUtility.ToJson(authResponse), sessionID);
                        APIManager.Instance.SaveTokenData();
                    }else if(authRequest.data.pluginAuthor != null && authRequest.data.pluginName != null){
                        // New Token Request
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
                                    PluginData data = new PluginData(authResponse.data.token, sessionID, authResponse.data.pluginName, authResponse.data.pluginAuthor);
                                    APIManager.Instance._tokenToSessionMap.Add(authResponse.data.token, data);
                                    this.SendToID(JsonUtility.ToJson(authResponse), sessionID);
                                    APIManager.Instance.SaveTokenData();
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
                        // Malformed Authentication Request
                        ErrorMessage error = new ErrorMessage();
                        error.data.message = "Message data must contain pluginName and pluginAuthor.";
                        error.data.errorCode = ErrorMessage.StatusCode.BAD_REQUEST;
                        this.SendToID(JsonUtility.ToJson(error), sessionID);
                    }
                }else{
                    // Unknown Message Type
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

    public class DataService : WebSocketService {
        protected override void OnOpen(){
            Debug.Log("Connection established to Data API...");
        }
    }

    public class EventsService : WebSocketService {
        protected override void OnOpen(){
            Debug.Log("Connection established to Event API...");
        }
    }

    public class InputService : WebSocketService {

        protected override void OnOpen(){
            Debug.Log("Connection established to Input API...");
        }
        protected override void OnMessage(MessageEventArgs e){
            MainThreadUtil.Run(() => { APIManager.Instance.InputEndpoint.Receive(e.Data, this.ID); });
        }
    }
}
    
