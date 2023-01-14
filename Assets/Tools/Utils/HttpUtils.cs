using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;

public static class HttpUtils {
	public static IEnumerator GetRequest(string url, Action<HttpError> onError, Action<string> onSuccess, string bearer) {
		UnityWebRequest webRequest = UnityWebRequest.Get(url);
		webRequest.SetRequestHeader("Content-Type", "application/json");
		if (bearer != null) {
			webRequest.SetRequestHeader("Authorization", string.Format("Bearer {0}", bearer));
		}
		using (webRequest) {
			// Request and wait for the desired page.
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError || webRequest.isHttpError) {
				string errorMessage = string.Format("Error making GET request to URL: {0} : {1}", url, webRequest.error);
				HttpError error = new HttpError(webRequest.responseCode, webRequest.error);
				onError.Invoke(error);
			}
			else {
				// Debug.Log("Received: " + webRequest.downloadHandler.text);
				try {
					onSuccess.Invoke(webRequest.downloadHandler.text);
				}
				catch (System.Exception e) {
					HttpError error = new HttpError(500, e.ToString());
					onError.Invoke(error);
				}
			}
		}
	}

	/// <summary>
	/// Returns the locak IPv4 address of this machine.
	/// </summary>
	/// <returns></returns>
	public static IPAddress GetLocalIPAddress() {

		using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
			socket.Connect("8.8.8.8", 65530);
			IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
			return endPoint.Address;
		}
		throw new Exception("No network adapters with an IPv4 address in the system!");
	}

	public static int ValidatePortValue(int value, int defaultPort){
        int port = value;
        if (port <= 0 || port > 65535){
            port = defaultPort;
        }
        return port;
    }

	public class HttpError {
		public long statusCode;
		public string message;

		public HttpError(long statusCode, string message) {
			this.statusCode = statusCode;
			this.message = message;
			// Debug.LogError(this.message);
			// LoggingManager.Instance.Log(this.message);
		}
	}

	public class ConnectionStatus {
		public string message;

		public Status status;

		public enum Status {
			CONNECTING = 0,
			CONNECTED = 1,
			ERROR = 2,
			DISCONNECTED = 3,
		}
	}
}