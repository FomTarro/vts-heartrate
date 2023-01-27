public interface IEndpoint {
	string Path { get; }
	IResponseArgs ProcessRequest(IRequestArgs request);
}

public interface IRequestArgs {
	IEndpoint Endpoint { get; }
	string Body { get; }
	string ClientID { get; }
}

public interface IResponseArgs {
	string Body { get; }
	int Status { get; }
}

