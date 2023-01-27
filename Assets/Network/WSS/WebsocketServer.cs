using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;

public class WebSocketServer : MonoBehaviour, IServer {

	[SerializeField]
	private int _port = 9000;
	public int Port => this._port;

	private WebSocketSharp.Server.WebSocketServer _server = null;
	private IEndpoint[] _endpoints = new IEndpoint[0];
	protected ConcurrentQueue<DataWrapper> _requests = new ConcurrentQueue<DataWrapper>();
	public List<string> Paths => new List<string>(this._server.WebSocketServices.Paths);

	public void SetPort(int port) {
		this._port = port;
	}

	private void Start(){
		StartServer();
	}

	public void StartServer() {
		StopServer();
		Debug.Log(string.Format("Websocket Server starting on port: {0}...", this._port));
		this._server = new WebSocketSharp.Server.WebSocketServer(this._port);
		this._endpoints = GetComponents<IEndpoint>();
		foreach (IEndpoint endpoint in this._endpoints) {
			this._server.AddWebSocketService<EndpointService>(endpoint.Path, () => { return new EndpointService(endpoint, this); });
			Debug.Log(string.Format("Websocket Server registering endpoint: {0}", endpoint.Path));
		}
		this._server.Start();
		Debug.Log(string.Format("Websocket Server started on port: {0}!", this._port));
	}

	public void StopServer() {
		if (this._server != null) {
			// TODO: clear request queue;
			this._server.Stop();
			Debug.Log(string.Format("Websocket Server stopping on port: {0}", this._port));
		}
	}

	private void Update() {
		while (this._requests.Count > 0) {
			DataWrapper request;
			if (this._requests.TryDequeue(out request)) {
				try {
					var response = request.endpoint.ProcessRequest(request.request);
					if (response.Audience == ResponseAudience.ALL) {
						request.endpoint.SendToAll(response.Body);
					} else if (response.Audience == ResponseAudience.REQUESTOR) {
						request.endpoint.SendToClient(response.Body);
					}
				}
				catch (System.Exception e) {
					Debug.LogError(string.Format("Websocket Server error: {0}", e));
				}
			}
		}
	}

	public bool SendToClient(string body, IEndpoint endpoint, string sessionID){
		try{
			WebSocketSharp.Server.WebSocketServiceHost host;
			if(this._server.WebSocketServices.TryGetServiceHost(endpoint.Path, out host)){
				host.Sessions.SendTo(body, sessionID);
				return true;
			}
		}catch(Exception e){
			Debug.LogError(string.Format("Error sending data to Websocket Client {0} with exception: {1}", sessionID, e));
		}
		return false;
	}

	public bool SendToAll(string body, IEndpoint endpoint){
		try{
			WebSocketSharp.Server.WebSocketServiceHost host;
			if(this._server.WebSocketServices.TryGetServiceHost(endpoint.Path, out host)){
				host.Sessions.Broadcast(body);
				return true;
			}
		}catch(Exception e){
			Debug.LogError(string.Format("Error sending data to all Websocket lients with exception: {0}", e));
		}
		return false;
	}

	public class EndpointService : WebSocketSharp.Server.WebSocketBehavior, IEndpoint {
		public string Path => this._endpoint.Path;
		private IEndpoint _endpoint;
		private WebSocketServer _server;
		private long _messagesIn = 0;
		public long MessagesIn => this._messagesIn;
		private long _messagesOut = 0;
		public long MessagesOut => this._messagesOut;

		public EndpointService(IEndpoint endpoint, WebSocketServer server) {
			this._endpoint = endpoint;
			this._server = server;
		}

		protected override void OnOpen() {
			Debug.Log(string.Format("Connection established to Websocket Service: {0}", this._endpoint.Path));
		}

		protected override void OnMessage(MessageEventArgs e) {
			// TODO: do we need URI here? I don't think so actually;
			WebsocketRequestArgs args = new WebsocketRequestArgs(null, e.Data, this.ID);
			DataWrapper wrapper = new DataWrapper(args, this);
			this._messagesIn = this._messagesIn + 1;
			this._server._requests.Enqueue(wrapper);
		}

		protected override void OnClose(CloseEventArgs e) {
			Debug.Log(string.Format("Disconnecting from Websocket Service {0}...", this._endpoint.Path));
		}

		public IResponseArgs ProcessRequest(IRequestArgs request) {
			return this._endpoint.ProcessRequest(request);
		}

		public bool SendToAll(string body) {
			try {
				this.Sessions.Broadcast(body);
				this._messagesOut = this._messagesOut + this.Sessions.Count;
				return true;
			}
			catch (System.Exception e) {
				Debug.LogError(string.Format("Websocket Server send error: {0}", e));
			}
			return false;
		}

		public bool SendToClient(string body) {
			try {
				this.SendAsync(body, (success) => { });
				this._messagesOut = this._messagesOut + 1;
				return true;
			}
			catch (System.Exception e) {
				Debug.LogError(string.Format("Websocket Server send error: {0}", e));
			}
			return false;
		}
	}

	protected class DataWrapper {
		public IRequestArgs request;
		public EndpointService endpoint;
		public DataWrapper(IRequestArgs request, EndpointService endpoint) {
			this.request = request;
			this.endpoint = endpoint;
		}
	}

	protected class WebsocketRequestArgs : IRequestArgs {
		private Uri _url;
		public Uri Url => this._url;
		private string _body;
		public string Body => this._body;
		private string _id;
		public string ClientID => this._id;

		public WebsocketRequestArgs(Uri url, string body, string id) {
			this._url = url;
			this._body = body;
			this._id = id;
		}
	}
}
