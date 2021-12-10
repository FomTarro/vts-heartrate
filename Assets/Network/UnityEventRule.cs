using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventRule : WebServerRule
{
    [SerializeField]
    public UnityEvent callback;
    protected override IEnumerator OnRequest(HttpListenerContext context)
    {
        callback.Invoke();
        context.Response.Close();
        yield return null;
    }

}
