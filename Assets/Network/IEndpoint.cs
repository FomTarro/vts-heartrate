using System;

public interface IEndpoint {
	string Path { get; }
	IResponseArgs ProcessRequest(IRequestArgs request);
}

public interface IRequestArgs {
	Uri Url { get; }
	string Body { get; }
}

public interface IResponseArgs {
    string Body { get; }
    int Status { get; }
	ResponseAudience Audience { get; }
}

public enum ResponseAudience {
	NONE,
	REQUESTOR,
	ALL,
}
