using UnityEngine;
using UnityEngine.Events;

public class UnityEventEndpoint : BaseUnityEndpoint {
	
	[System.Serializable]
	public class WebServerPostEvent : UnityEvent<string> { }
	[SerializeField]
	public WebServerPostEvent callback;

	public override IResponseArgs ProcessRequest(IRequestArgs request) {
		string body = request.Body;
		try{
			callback.Invoke(body);
			return new UnityEventResponse(200, JsonUtility.ToJson(new ResponseMessage()));
		}catch(System.Exception e){
			return new UnityEventResponse(500, e.Message);
		}
	}

	[System.Serializable]
	public class ResponseMessage {
		public string message = "OK!";
	}

	[System.Serializable]
	public class UnityEventResponse : IResponseArgs {
		private string _body = "";
		public string Body => this._body;

		private int _status = 200;
		public int Status => this._status;


		public UnityEventResponse (int status, string body){
			this._status = status;
			this._body = body;
		}
	}
}
