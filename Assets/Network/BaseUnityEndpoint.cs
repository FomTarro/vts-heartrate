using UnityEngine;

public abstract class BaseUnityEndpoint : MonoBehaviour, IEndpoint {
    
	[SerializeField]
	protected string _path;
	public string Path => this._path;

	public abstract IResponseArgs ProcessRequest(IRequestArgs request);
}
