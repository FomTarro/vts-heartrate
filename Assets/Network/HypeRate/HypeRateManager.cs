using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTS.Networking;
using VTS.Networking.Impl;

public class HypeRateManager : Singleton<HypeRateManager>
{
    private IWebSocket _socket = new WebSocketSharpImpl();
    private const string HYPERATE_SOCKET_URL = @"wss://app.hyperate.io/socket/websocket?token={0}";
    private string _hyperateID = null;
    public string HypeRateID { get { return this._hyperateID; } }
    private string KEEP_ALIVE_MESSAGE;

    [SerializeField]
    private int _heartrate = 0;
    public int Heartrate { get { return this._heartrate; } }

    public override void Initialize()
    {
        HypeRateMessage message = new HypeRateMessage("phoenix");
        message.@event = "heartbeat";
        this.KEEP_ALIVE_MESSAGE = JsonUtility.ToJson(message);
    }

    public void SetHyperateID(string id){
        this._hyperateID = id;
    }

    public void Connect(System.Action<HttpUtils.ConnectionStatus> onStatus){
        Disconnect(onStatus);
        HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
        status.status = HttpUtils.ConnectionStatus.Status.CONNECTING;
        onStatus.Invoke(status);
        this._socket.Start(string.Format(HYPERATE_SOCKET_URL, HypeRateCredentials.API_KEY),
        () => {
            Debug.Log("Connected to HypeRate socket");
            string send = JsonUtility.ToJson(new HypeRateMessage("hr:"+_hyperateID));
            this._socket.Send(send);
            // this._socket.Send("{\"topic\": \"hr:"+_hyperateID+"\", \"event\": \"phx_join\", \"payload\": {}, \"ref\": 0}");
            status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
            onStatus.Invoke(status);
        },
        () => {
            Debug.Log("Disconnected from the HypeRate socket");
            status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
            onStatus.Invoke(status);
        },
        () => {
            Debug.LogError("An error occured while connecting to the HypeRate socket...");
            status.status = HttpUtils.ConnectionStatus.Status.ERROR;
            onStatus.Invoke(status);
            // Connect(onStatus);
        });
    }

    public void Disconnect(System.Action<HttpUtils.ConnectionStatus> onStatus){
        if(this._socket.IsConnecting() || this._socket.IsConnectionOpen()){
            this._socket.Stop();
        }
        HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
        status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
        onStatus.Invoke(status);
    }

    public void FixedUpdate(){

        if(this._socket.IsConnectionOpen()){
            // Debug.Log(this._socket.GetNextResponse());
            string response = this._socket.GetNextResponse();
            HypeRateMessage message = JsonUtility.FromJson<HypeRateMessage>(response);
            if(message != null){
                if(message.@event.Equals("hr_update")){
                    this._heartrate = message.payload.hr;
                }else if(message.@event.Equals("phx_reply")){
                    // {"event":"phx_reply","payload":{"response":{},"status":"ok"},"ref":0,"topic":"phoenix"}
                    if(message.payload.status.Equals("ok")){
                        // all good
                    }
                    else{
                        Debug.LogError("Error from HypeRate socket with status: " + message.payload.status);
                    }
                }else{
                    // error of some kind
                    Debug.LogError(response);
                }
            }
            // {\"topic\": \"phoenix\",\"event\": \"heartbeat\",\"payload\": {},\"ref\": 0}
            this._socket.Send(KEEP_ALIVE_MESSAGE);
        }
    }

    [System.Serializable]
    public class HypeRateMessage{
        public string topic = "";
        public string @event = "phx_join"; // lmao don't name your variables like this in your API schema, please!
        public HypeRatePayload payload = new HypeRatePayload();
        public int @ref = 0;

        public HypeRateMessage(string topic){
            this.topic = topic;
        }

        [System.Serializable]
        public class HypeRatePayload{
            public int hr;
            public string response;
            public string status;

        }
    }

}
