using UnityEngine;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class OpenURLButton : MonoBehaviour
{
    private ExtendedButton _button = null;
    [SerializeField]
    private string _url;

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerUp.AddListener(() => {Application.OpenURL(this._url);});
    }
}

