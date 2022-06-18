﻿using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Server;

public class APIManager : Singleton<APIManager>
{
    private WebSocketServer _server = null;


    // data api
    private const string DATA_PATH = "/data";
    protected int _dataClients = 0;
    public int DataClientCount { get { return this._dataClients; }}
    protected long _dataMessages = 0;
    public long DataMessages { get { return this._dataMessages; }}

    // event api
    private const string EVENTS_PATH = "/events";
    protected int _eventClients = 0;
    public int EventClientCount { get { return this._eventClients; }}
    protected long _eventMessages = 0;
    public long EventMessages { get { return this._eventMessages; }}

    public int TotalClientCount { get { return this._eventClients + this._dataClients; }}

    private int _port;
    public int Port { get { return this._port; } }

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
            this._server.AddWebSocketService<DataService>(DATA_PATH);
            this._dataMessages = 0;
            this._server.AddWebSocketService<EventService>(EVENTS_PATH);
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

    private void Update(){
        if(this._server != null){
            WebSocketServiceHost dataHost;
            if(this._server.WebSocketServices.TryGetServiceHost(DATA_PATH, out dataHost)){
                this._dataClients = dataHost.Sessions.Count;
            }
            WebSocketServiceHost eventHost;
            if(this._server.WebSocketServices.TryGetServiceHost(EVENTS_PATH, out eventHost)){
                
            }
        }
    }

    public void SendData(){
        WebSocketServiceHost dataHost;
        if(this._server != null 
        && this._server.WebSocketServices.TryGetServiceHost(DATA_PATH, out dataHost)){
            DataMessage data = new DataMessage();
            data.data.heartrate = HeartrateManager.Instance.Plugin.HeartRate;
            Dictionary<string, float> paramMap = HeartrateManager.Instance.Plugin.ParameterMap;
            data.data.vts_heartrate_bpm = GetValueFromDictionary(paramMap, "VTS_Heartrate_BPM");
            data.data.vts_heartrate_pulse = GetValueFromDictionary(paramMap, "VTS_Heartrate_Pulse");
            data.data.vts_heartrate_breath = GetValueFromDictionary(paramMap, "VTS_Heartrate_Breath");
            data.data.vts_heartrate_linear = GetValueFromDictionary(paramMap, "VTS_Heartrate_Linear");
            dataHost.Sessions.Broadcast(JsonUtility.ToJson(data));
            this._dataMessages = this._dataMessages + dataHost.Sessions.Count;
        }
    }

    public void SendEvent(object eventInfo){
        WebSocketServiceHost eventHost;
        if(this._server.WebSocketServices.TryGetServiceHost(EVENTS_PATH, out eventHost)){
            eventHost.Sessions.Broadcast(JsonUtility.ToJson(""));
            this._eventMessages = this._eventMessages + eventHost.Sessions.Count;
        }
    }

    private float GetValueFromDictionary(Dictionary<string, float> dict, string key){
        return dict.ContainsKey(key) ? dict[key] : 0.0f;
    }

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

    public class EventService : WebSocketService {
        protected override void OnOpen(){
            Debug.Log("Connection established to Event API...");
        }
    }
}
    
