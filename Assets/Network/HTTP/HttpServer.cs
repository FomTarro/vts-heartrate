using System.Collections.Concurrent;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using System;

public class HttpServer : MonoBehaviour, IServer {

	private int _port = 9000;
	public int Port => this._port;

	private WebSocketSharp.Server.HttpServer _server = null;
	private IEndpoint[] _endpoints = new IEndpoint[0];
	private ConcurrentQueue<DataWrapper> _requests = new ConcurrentQueue<DataWrapper>();

	public void SetPort(int port) {
		this._port = port;
	}

	public void StartServer() {
		StopServer();
		this._server = new WebSocketSharp.Server.HttpServer(this._port);
		this._endpoints = GetComponents<IEndpoint>();
		this._server.OnGet += (a, b) => {
			EnqueueResponse(b);
		};
		this._server.OnPost += (a, b) => {
			EnqueueResponse(b);
		};
		this._server.Start();
	}

	public void StopServer() {
		if (this._server != null) {
			this._server.Stop();
		}
	}

	private void OnApplicationQuit() {
		StopServer();
	}

	private void EnqueueResponse(WebSocketSharp.Server.HttpRequestEventArgs args){
		string body;
		using (var reader = new StreamReader(args.Request.InputStream, args.Request.ContentEncoding)) {
			body = reader.ReadToEnd();
		}
		this._requests.Enqueue(new DataWrapper(body, args));
	}

	private void Update(){
		while(this._requests.Count > 0){
			DataWrapper request;
			if(this._requests.TryDequeue(out request)){
				DispatchToEndpoint(request);
			}
		}
	}

	private void DispatchToEndpoint(DataWrapper data) {
		HttpRequestArgs request = new HttpRequestArgs(data.args.Request.Url, data.body);
		if(request.Url.ToString().StartsWith("http")){
			foreach(IEndpoint endpoint in this._endpoints){
				if(request.Url.ToString().EndsWith(endpoint.Path)){
					IResponseArgs response = endpoint.ProcessRequest(request);
					data.args.Response.WriteContent(data.args.Request.ContentEncoding.GetBytes(response.Body));
					data.args.Response.Close();
				}
			}
		}
	}

	private class DataWrapper{
		public string body;
		public WebSocketSharp.Server.HttpRequestEventArgs args;
		public DataWrapper(string body, WebSocketSharp.Server.HttpRequestEventArgs args){
			this.body = body;
			this.args = args;
		}
	}

	private class HttpRequestArgs : IRequestArgs {
		private Uri _url;
		public Uri Url => this._url;
		private string _body;
		public string Body => this._body;

		public HttpRequestArgs(Uri url, string body){
			this._url = url;
			this._body = body;
		}
	}
}
