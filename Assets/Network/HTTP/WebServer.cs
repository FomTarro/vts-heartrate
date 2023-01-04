using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

public class WebServer : MonoBehaviour {

	[SerializeField]
	private int _port = 9000;

	public void SetPort(int port){
		this._port = port;
		StartThread();
	}

#if UNITY_STANDALONE || UNITY_EDITOR

	// Use this for initialization
	private void Start() {
		this._rules = GetComponents<WebServerRule>();
		StartCoroutine(HandleRequests());
	}

	private void OnDestroy() {
        AbortThread();
	}

	private void OnApplicationQuit() {
        AbortThread();
	}

	private void StartThread(){
		AbortThread();
		this._closeThreadAndContexts = false;
		this._listenerThread = new Thread(ListenThread);
		this._listenerThread.Start();
	}

    private void AbortThread(){
        this._closeThreadAndContexts = true;
		if (this._listenerThread != null) {
			this._listener.Close();
			this._listenerThread.Abort();
		}
    }

	private void ListenThread() {
		try {
			this._listener = new HttpListener();

			string host = string.Format("http://*:{0}/", this._port);
			foreach(string suffix in this._suffixes){
				this._listener.Prefixes.Add(string.Format("{0}{1}", host, suffix));
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
				Debug.Log(string.Format(this.name, "{0} is aborting listener thread."));
			}
			else {
				Debug.LogError(string.Format("Web server error at {0}.", e.StackTrace));
				Debug.LogError(e.Message, this);
				Debug.LogError(this._listenerThread.ThreadState);
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
				//Debug.LogFormat("Processing request for {0}.", nextContext.Request.RemoteEndPoint.ToString());
				foreach (WebServerRule rule in _rules) {
					bool isMatch = false;
					IEnumerator e = rule.ProcessRequest(nextContext, x => isMatch = x);
					do {
						yield return null;
					}
					while (e.MoveNext());
					if (isMatch && rule.BlockOnMatch){
						break;
                    }
				}

				Thread thread = new Thread(new ParameterizedThreadStart(FinishRequest));
				thread.Start(nextContext);
			}
			else
				yield return null;
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

#endif

	[SerializeField]
	private string[] _suffixes = new string[] { "QuickServer" };
	private long _workerThreadIdGenerator;
	private HttpListener _listener;
	private LinkedList<HttpListenerContext> _waitingContexts = new LinkedList<HttpListenerContext>();
	private Thread _listenerThread;
	private WebServerRule[] _rules;
	private bool _closeThreadAndContexts = false;
}
