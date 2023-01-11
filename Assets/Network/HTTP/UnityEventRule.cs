using UnityEngine;
using UnityEngine.Events;

public class UnityEventRule : MonoBehaviour, IEndpoint {
	[System.Serializable]
	public class WebServerPostEvent : UnityEvent<string> { }
	[SerializeField]
	public WebServerPostEvent callback;

	[SerializeField]
	private string _path;
	public string Path => this._path;

	public IResponseArgs ProcessRequest(IRequestArgs request) {
		string body = request.Body;
		try{
			callback.Invoke(body);
			return new UnityEventResponse(200, "");
		}catch(System.Exception e){
			return new UnityEventResponse(500, e.Message);
		}
	}

	public class UnityEventResponse : IResponseArgs {
		private string _body = "";
		public string Body => this._body;

		public long _status = 200;
		public long Status => this._status;

		public UnityEventResponse (long status, string body){
			this._status = status;
			this._body = body;
		}
	}
}
