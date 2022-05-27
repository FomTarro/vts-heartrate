using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Server;

public class APIManager : Singleton<APIManager>
{
    private WebSocketServer _server = null;

    private const string DATA_PATH = "/data";
    private const string EVENTS_PATH = "/events";

    public override void Initialize()
    {
        StartOnPort(8090);
    }


    public void StartOnPort(int port){
        if(this._server != null){
            this._server.Stop();
        }
        try{
            this._server = new WebSocketServer(port, false);
            this._server.AddWebSocketService<DataService>(DATA_PATH);
            this._server.AddWebSocketService<EventService>(EVENTS_PATH);
            this._server.Start();
        }catch(System.Exception e){
            Debug.LogError(e);
        }
    }

    public void SendData(){
        WebSocketServiceHost dataHost;
        if(this._server.WebSocketServices.TryGetServiceHost(DATA_PATH, out dataHost)){
            DataMessage data = new DataMessage();
            data.heartrate = HeartrateManager.Instance.Plugin.HeartRate;
            Dictionary<string, float> paramMap = HeartrateManager.Instance.Plugin.ParameterMap;
            data.parameters.vts_heartrate_bpm = GetValueFromDictionary(paramMap, "VTS_Heartrate_BPM");
            data.parameters.vts_heartrate_pulse = GetValueFromDictionary(paramMap, "VTS_Heartrate_Pulse");
            data.parameters.vts_heartrate_breath = GetValueFromDictionary(paramMap, "VTS_Heartrate_Breath");
            data.parameters.vts_heartrate_linear = GetValueFromDictionary(paramMap, "VTS_Heartrate_Linear");
            dataHost.Sessions.Broadcast(JsonUtility.ToJson(data));
        }
    }

    public void SendEvent(object eventInfo){
        WebSocketServiceHost eventHost;
        if(this._server.WebSocketServices.TryGetServiceHost(EVENTS_PATH, out eventHost)){
        
        }
    }

    private float GetValueFromDictionary(Dictionary<string, float> dict, string key){
        return dict.ContainsKey(key) ? dict[key] : 0.0f;
    }

    [System.Serializable]
    public class APIMessage {
        public long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public class DataService : WebSocketService {
        protected override void OnOpen(){
            Debug.Log("Connection established to Data API...");
        }
    }

    public class DataMessage : APIMessage {
        public int heartrate;
        public Parameters parameters = new Parameters();
    }

    [System.Serializable]
    public class Parameters{
        public float vts_heartrate_bpm;
        public float vts_heartrate_pulse;
        public float vts_heartrate_breath;
        public float vts_heartrate_linear;
    }

    public class EventService : WebSocketService {
        protected override void OnOpen(){
            Debug.Log("Connection established to Event API...");
        }
    }
}
    
