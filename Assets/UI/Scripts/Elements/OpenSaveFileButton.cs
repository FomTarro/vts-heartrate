using UnityEngine;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class OpenSaveFileButton : MonoBehaviour
{
    private ExtendedButton _button = null;

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerUp.AddListener(() => {Application.OpenURL(SaveDataManager.Instance.SaveDirectory);});
    }
}
