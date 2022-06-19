using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
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

    // input api
    private const string INPUT_PATH = "/input";
    protected int _inputClients = 0;
    public int InputClientCount { get { return this._inputClients; }}
    protected long _inputMessages = 0;
    public long InputMessages { get { return this._inputMessages; }}

    private int _heartrate = 0;
    public int Heartrate { get { return this._heartrate; } }

    public int TotalClientCount { get { return this._eventClients + this._dataClients + this._inputClients; }}

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
            this._eventMessages = 0;
            this._server.AddWebSocketService<InputService>(INPUT_PATH);
            this._inputMessages = 0;
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
                this._eventClients = eventHost.Sessions.Count;
            }
            WebSocketServiceHost inputHost;
            if(this._server.WebSocketServices.TryGetServiceHost(INPUT_PATH, out inputHost)){
                this._inputClients = inputHost.Sessions.Count;
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

    public void SendEvent(EventMessage message){
        WebSocketServiceHost eventHost;
        if(this._server != null 
        && this._server.WebSocketServices.TryGetServiceHost(EVENTS_PATH, out eventHost)){
            eventHost.Sessions.Broadcast(JsonUtility.ToJson(message));
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

    public class InputService : WebSocketService {
        protected override void OnMessage(MessageEventArgs e)
        {
            try{
                InputMessage message = JsonUtility.FromJson<InputMessage>(e.Data);
                Debug.Log(message.data.heartrate);
                APIManager.Instance._heartrate = message.data.heartrate;
                APIManager.Instance._inputMessages = APIManager.Instance._inputMessages + 1;
            }catch(System.Exception ex){
                Debug.LogErrorFormat("Error parsing Input socket message: {0}, {1}",
                e.Data, ex);
            }
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
}
    
