using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class WebsocketServer : MonoBehaviour, IServer {

	private int _port = 9000;
	public int Port => this._port;

	private WebSocketSharp.Server.WebSocketServer _server = null;
	private IEndpoint[] _endpoints = new IEndpoint[0];
	protected ConcurrentQueue<DataWrapper> _requests = new ConcurrentQueue<DataWrapper>();

	public void SetPort(int port) {
		this._port = port;
	}

	public void StartServer() {
		StopServer();
		this._server = new WebSocketSharp.Server.WebSocketServer(this._port);
		this._endpoints = GetComponents<IEndpoint>();
		foreach (IEndpoint endpoint in this._endpoints) {
			this._server.AddWebSocketService<EndpointService>(endpoint.Path, () => { return new EndpointService(endpoint, this); });
		}
		this._server.Start();
	}

	public void StopServer() {
		if (this._server != null) {
			this._server.Stop();
		}
	}

	private void Update() {
		while (this._requests.Count > 0) {
			DataWrapper request;
			if (this._requests.TryDequeue(out request)) {
				try {
					var response = request.endpoint.ProcessRequest(request.request);
					// TODO: how do we determine if we need to send this response to just our client, or all connected clients, or no one at all?
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

		public EndpointService(IEndpoint endpoint, WebsocketServer server) {
			this._endpoint = endpoint;
			this._server = server;
		}

		protected override void OnOpen() {
			Debug.Log(string.Format("Connection established to Socket Service {0}...", this._endpoint.Path));
		}
		protected override void OnMessage(MessageEventArgs e) {
			// MainThreadUtil.Run(() => { this._onMessage(e.Data, this.ID); });
			// TODO: do we need URI here? I don't think so actually;
			WebsocketRequestArgs args = new WebsocketRequestArgs(null, e.Data);
			DataWrapper wrapper = new DataWrapper(args, this, this.ID);
			this._server._requests.Enqueue(wrapper);
		}

		protected override void OnClose(CloseEventArgs e) {
			Debug.LogFormat("Closing {0} Socket Service...", this._endpoint.Path);
		}

		public IResponseArgs ProcessRequest(IRequestArgs request) {
			return this._endpoint.ProcessRequest(request);
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
