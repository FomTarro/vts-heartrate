using UnityEngine;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class ForceReloadModelButton : MonoBehaviour {
    private ExtendedButton _button = null;

    // Start is called before the first frame update
    void Start(){
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerUp.AddListener(() => { HeartrateManager.Instance.Plugin.Connect(); });
    }
}
