using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventRule : WebServerRule {
	[System.Serializable]
	public class WebServerPostEvent : UnityEvent<string> { }

	[SerializeField]
	public WebServerPostEvent callback;

	protected override IEnumerator OnRequest(HttpListenerContext context) {
		var request = context.Request;
		string body = "";
		if (request.HttpMethod.ToUpper().Equals("POST")) {
			using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) {
				body = reader.ReadToEnd();
			}
		}
		callback.Invoke(body);
		context.Response.Close();
		yield return null;
	}

}
