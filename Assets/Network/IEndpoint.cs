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
    long Status { get; }
}
