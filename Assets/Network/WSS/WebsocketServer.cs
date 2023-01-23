using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;

public class WebsocketServer : MonoBehaviour, IServer {

	[SerializeField]
	private int _port = 9000;
	public int Port => this._port;

	private WebSocketSharp.Server.WebSocketServer _server = null;
	private IEndpoint[] _endpoints = new IEndpoint[0];
	protected ConcurrentQueue<DataWrapper> _requests = new ConcurrentQueue<DataWrapper>();

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

	public class EndpointService : WebSocketSharp.Server.WebSocketBehavior, IEndpoint {
		public string Path => this._endpoint.Path;
		private IEndpoint _endpoint;
		private WebsocketServer _server;
		private long _messagesIn = 0;
		public long MessagesIn => this._messagesIn;
		private long _messagesOut = 0;
		public long MessagesOut => this._messagesOut;

		public EndpointService(IEndpoint endpoint, WebsocketServer server) {
			this._endpoint = endpoint;
			this._server = server;
		}

		protected override void OnOpen() {
			Debug.Log(string.Format("Connection established to Websocket Service: {0}", this._endpoint.Path));
		}

		protected override void OnMessage(MessageEventArgs e) {
			// TODO: do we need URI here? I don't think so actually;
			WebsocketRequestArgs args = new WebsocketRequestArgs(null, e.Data);
			DataWrapper wrapper = new DataWrapper(args, this, this.ID);
			this._messagesIn = this._messagesIn + 1;
			this._server._requests.Enqueue(wrapper);
		}

		protected override void OnClose(CloseEventArgs e) {
			Debug.Log(string.Format("Closing Websocket Service {0}...", this._endpoint.Path));
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
		public string id;
		public DataWrapper(IRequestArgs request, EndpointService endpoint, string id) {
			this.request = request;
			this.endpoint = endpoint;
			this.id = id;
		}
	}

	protected class WebsocketRequestArgs : IRequestArgs {
		private Uri _url;
		public Uri Url => this._url;
		private string _body;
		public string Body => this._body;

		public WebsocketRequestArgs(Uri url, string body) {
			this._url = url;
			this._body = body;
		}
	}
}
