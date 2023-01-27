using UnityEngine;
using UnityEngine.Events;

public class UnityEventEndpoint : BaseUnityEndpoint {
	
	[System.Serializable]
	public class WebServerPostEvent : UnityEvent<string> { }
	[SerializeField]
	public WebServerPostEvent callback;

	[SerializeField]
	private ResponseAudience _audience = ResponseAudience.REQUESTOR;

	public override IResponseArgs ProcessRequest(IRequestArgs request) {
		string body = request.Body;
		try{
			callback.Invoke(body);
			return new UnityEventResponse(200, JsonUtility.ToJson(new ResponseMessage()), this._audience);
		}catch(System.Exception e){
			return new UnityEventResponse(500, e.Message, this._audience);
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

		private ResponseAudience _audience;
		public ResponseAudience Audience => this._audience;

		public UnityEventResponse (int status, string body, ResponseAudience audience){
			this._status = status;
			this._body = body;
			this._audience = audience;
		}
	}
}
