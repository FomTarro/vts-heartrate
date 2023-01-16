﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

public class WebServer : MonoBehaviour, IServer {

	private int _port = 9000;
	public int Port => this._port;

	private WebSocketSharp.Server.HttpServer _server = null;
	private IEndpoint[] _endpoints = new IEndpoint[0];
    [SerializeField]
    private string[] _suffixes = new string[] { "QuickServer" };
	private ConcurrentQueue<DataWrapper> _requests = new ConcurrentQueue<DataWrapper>();

	[SerializeField]
	// private string[] _prefixes = new string[] { "http://*:8080/QuickServer/" };

	private HttpListener _listener;

	private LinkedList<HttpListenerContext> _waitingContexts = new LinkedList<HttpListenerContext>();

	private Thread _listenerThread;

	private bool _closeThreadAndContexts = false;


	public void SetPort(int port) {
		this._port = port;
	}


	public void StartServer() {
        StopServer();
		this._endpoints = GetComponents<IEndpoint>();
		this._closeThreadAndContexts = false;
		this._listenerThread = new Thread(ListenThread);
		this._listenerThread.Start();
		StartCoroutine(HandleRequests());
	}

	public void StopServer() {
		this._closeThreadAndContexts = true;
		if (this._listenerThread != null) {
			this._listener.Close();
			this._listenerThread.Abort();
		}
	}

	void OnDestroy() {
        StopServer();
	}

	void OnApplicationQuit() {
        StopServer();
	}

	private void ListenThread() {
		try {
			this._listener = new HttpListener();
			string host = string.Format("http://*:{0}/", this._port);
			foreach(string suffix in this._suffixes){
				this._listener.Prefixes.Add(string.Format("{0}{1}/", host, suffix));
			}

			this._listener.Start();
			while (!this._closeThreadAndContexts) {
				HttpListenerContext context = this._listener.GetContext();
				//Debug.LogFormat("Recieved request from {0}.", context.Request.RemoteEndPoint.ToString());
				context.Response.StatusCode = 200;
				lock (this._waitingContexts) {
					this._waitingContexts.AddLast(context);
				}
			}
		}
		catch (Exception e) {
			if (typeof(ThreadAbortException) == e.GetType()) {
				Debug.Log("HTTP Server: aborting listener thread.");
			}
			else {
				Debug.LogErrorFormat("HTTP Server error at {0}.", e.StackTrace);
				Debug.LogError(e.Message, this);
				// Debug.LogError(this._listenerThread.ThreadState);
			}
		}
	}

	private IEnumerator HandleRequests() {
		while (true) {
			HttpListenerContext nextContext = null;
			lock (this._waitingContexts) {
				if (this._waitingContexts.Count > 0) {
					nextContext = this._waitingContexts.First.Value;
					this._waitingContexts.RemoveFirst();
				}
			}

			if (nextContext != null) {
				bool match = false;
				//Debug.LogFormat("Processing request for {0}.", nextContext.Request.RemoteEndPoint.ToString());
				foreach (IEndpoint endpoint in this._endpoints) {

					if (nextContext.Request.Url.ToString().EndsWith(endpoint.Path)) {
						match = true;
						string body = "";
						using (var reader = new StreamReader(nextContext.Request.InputStream, nextContext.Request.ContentEncoding)) {
							body = reader.ReadToEnd();
						}
						HttpRequestArgs args = new HttpRequestArgs(nextContext.Request.Url, body);
						DataWrapper wrapper = new DataWrapper(args, nextContext, endpoint);
						this._requests.Enqueue(wrapper);
					}
				}
				if (!match) {
					nextContext.Response.StatusCode = 404;
					nextContext.Response.Close();
				}

				// Thread thread = new Thread(new ParameterizedThreadStart(FinishRequest));
				// thread.Start(nextContext);
			}
			else
				yield return null;
		}
	}

	private void Update() {
		while (this._requests.Count > 0) {
			DataWrapper request;
			if (this._requests.TryDequeue(out request)) {
				try {
					var response = request.endpoint.ProcessRequest(request.request);
					request.context.Response.StatusCode = response.Status;
					byte[] bytes = request.context.Request.ContentEncoding.GetBytes(response.Body);
					request.context.Response.OutputStream.Write(bytes, 0, bytes.Length);
					request.context.Response.Close();

				}
				catch (System.Exception e) {
					Debug.LogError(string.Format("HTTP Server error: {0}", e));
					request.context.Response.StatusCode = 500;
					request.context.Response.Close();
				}
			}
		}
	}

	private void FinishRequest(object arg) {
		/*
        HttpListenerContext context = (HttpListenerContext)arg;
        Debug.LogFormat("Request for {0} finished.", context.Request.RemoteEndPoint.ToString());

        // TODO: Commented out for now, as we have rules dictate their own closures
        context.Response.Close();
        */
	}

	private class DataWrapper {
		public HttpRequestArgs request;
		public HttpListenerContext context;
		public IEndpoint endpoint;
		public DataWrapper(HttpRequestArgs request, HttpListenerContext context, IEndpoint endpoint) {
			this.request = request;
			this.context = context;
			this.endpoint = endpoint;
		}
	}

	private class HttpRequestArgs : IRequestArgs {
		private Uri _url;
		public Uri Url => this._url;
		private string _body;
		public string Body => this._body;

		public HttpRequestArgs(Uri url, string body) {
			this._url = url;
			this._body = body;
		}
	}
}