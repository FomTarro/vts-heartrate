using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ExampleRule : WebServerRule {
	protected override IEnumerator OnRequest(HttpListenerContext context) {
        Debug.Log("Ping!");
		context.Response.StatusCode = 200;
		context.Response.Close();
		yield return null;
	}
}
