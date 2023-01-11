using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class WebsocketServer : MonoBehaviour, IServer {

	private int _port = 9000;
	public int Port => this._port;

	private WebSocketSharp.Server.WebSocketServer _server = null;
	private IEndpoint[] _endpoints = new IEndpoint[0];

	public void SetPort(int port) {
		this._port = port;
	}

	public void StartServer() {
		StopServer();
		this._server = new WebSocketSharp.Server.WebSocketServer(this._port);
		this._endpoints = GetComponents<IEndpoint>();
		foreach (IEndpoint endpoint in this._endpoints) {
			this._server.AddWebSocketService<EndpointService>(endpoint.Path, () => { return new EndpointService(endpoint); });
		}
		this._server.Start();
	}

	public void StopServer() {
		if (this._server != null) {
			this._server.Stop();
		}
	}

	public class EndpointService : WebSocketSharp.Server.WebSocketBehavior, IEndpoint {
		public string Path => this._endpoint.Path;

		private IEndpoint _endpoint;
		public EndpointService(IEndpoint endpoint) {
			this._endpoint = endpoint;
		}

		protected override void OnOpen() {
			Debug.Log(string.Format("Connection established to Socket Service {0}...", this._endpoint.Path));
		}
		protected override void OnMessage(MessageEventArgs e) {
			// MainThreadUtil.Run(() => { this._onMessage(e.Data, this.ID); });
            // TODO: do we need URI here? I don't think so actually;
            WebsocketRequestArgs args = new WebsocketRequestArgs(null, e.Data, this.ID);

		}

		protected override void OnClose(CloseEventArgs e) {
			Debug.LogFormat("Closing {0} Socket Service...", this._endpoint.Path);
		}

		public IResponseArgs ProcessRequest(IRequestArgs request) {
			return this._endpoint.ProcessRequest(request);
		}
	}

	private class WebsocketRequestArgs : IRequestArgs {
		private Uri _url;
		public Uri Url => this._url;
		private string _body;
		public string Body => this._body;
        private string _id;
        public string ID => this._id;

		public WebsocketRequestArgs(Uri url, string body, string id) {
			this._url = url;
			this._body = body;
            this._id = id;
		}
	}
}
