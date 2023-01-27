public class InputEndpoint : BaseUnityEndpoint {

	public override IResponseArgs ProcessRequest(IRequestArgs request) {
		APIManager.Instance.ProcessInput(request.Body, request.ClientID);
        return new InputResponse(200, "");
	}

    [System.Serializable]
	public class InputResponse : IResponseArgs {
		private string _body = "";
		public string Body => this._body;

		private int _status = 200;
		public int Status => this._status;

		public InputResponse (int status, string body){
			this._status = status;
			this._body = body;
		}
	}
}
