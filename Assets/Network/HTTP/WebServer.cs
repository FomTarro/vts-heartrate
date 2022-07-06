using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System;

public class WebServer : MonoBehaviour
{

#if UNITY_STANDALONE || UNITY_EDITOR

    // Use this for initialization
    void Start ()
    {
        rules = GetComponents<WebServerRule>();
        closeThreadAndContexts = false;
        listenerThread = new Thread(ListenThread);
        listenerThread.Start();
        StartCoroutine(HandleRequests());
	}

    void OnDestroy()
    {
        closeThreadAndContexts = true;
        if (listenerThread != null)
        {
            listener.Close();
            listenerThread.Abort();
        }
    }

    void OnApplicationQuit()
    {
        closeThreadAndContexts = true;
        if (listenerThread != null)
        {
            listenerThread.Abort();
            listener.Close();
        }
    }

    private void ListenThread()
    {
        try
        {
            listener = new HttpListener();

            foreach (string prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }

            listener.Start();

            while (!closeThreadAndContexts)
            {
                HttpListenerContext context = listener.GetContext();

                //Debug.LogFormat("Recieved request from {0}.", context.Request.RemoteEndPoint.ToString());

                context.Response.StatusCode = 200;
                lock (waitingContexts)
                {
                    waitingContexts.AddLast(context);
                }
            }
        }
        catch(Exception e)
        {
            if (typeof(ThreadAbortException) == e.GetType())
            {
                Debug.Log("[<b>Web Server</b>] Aborting listener thread.");
            }
            else
            {
                Debug.LogErrorFormat("Web server error at {0}.", e.StackTrace);
                Debug.LogError(e.Message, this);
                Debug.LogError(listenerThread.ThreadState);
            }
        }
    }

    private IEnumerator HandleRequests()
    {
        while (true)
        {
            HttpListenerContext nextContext = null;
            lock(waitingContexts)
            {
                if(waitingContexts.Count > 0)
                {
                    nextContext = waitingContexts.First.Value;
                    waitingContexts.RemoveFirst();
                }
            }

            if (nextContext != null)
            {
                //Debug.LogFormat("Processing request for {0}.", nextContext.Request.RemoteEndPoint.ToString());
                foreach(WebServerRule rule in rules)
                {
                    bool isMatch = false;
                    IEnumerator e = rule.ProcessRequest(nextContext, x => isMatch = x);
                    do
                    {
                        yield return null;
                    }
                    while (e.MoveNext());

                    if (isMatch && rule.BlockOnMatch)
                        break;
                }

                Thread thread = new Thread(new ParameterizedThreadStart(FinishRequest));
                thread.Start(nextContext);
            }
            else
                yield return null;
        }
    }

    private void FinishRequest(object arg)
    {
        /*
        HttpListenerContext context = (HttpListenerContext)arg;
        Debug.LogFormat("Request for {0} finished.", context.Request.RemoteEndPoint.ToString());

        // TODO: Commented out for now, as we have rules dictate their own closures
        context.Response.Close();
        */
    }

#endif

    [SerializeField]
    private string[] prefixes = new string[] { "http://*:8080/QuickServer/" };

    private long workerThreadIdGenerator;

    private HttpListener listener;

    private LinkedList<HttpListenerContext> waitingContexts = new LinkedList<HttpListenerContext>();

    private Thread listenerThread;

    private WebServerRule[] rules;

    private bool closeThreadAndContexts = false;
}
