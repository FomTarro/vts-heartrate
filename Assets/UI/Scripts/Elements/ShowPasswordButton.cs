using UnityEngine;
using TMPro;
using UI.InputTools;
 
[RequireComponent(typeof(ExtendedButton))]
public class ShowPasswordButton : MonoBehaviour
{
    private ExtendedButton _button = null;
    [SerializeField]
    private TMP_InputField _input = null;

    private bool _toggle = false;

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerDown.AddListener(Toggle);
    }

    private void Toggle(){
        if(this._toggle){
            HidePassword();
        }else{
            ShowPassword();
        }
        this._toggle = !this._toggle;
    }

    private void ShowPassword(){
        this._input.contentType = TMP_InputField.ContentType.Standard;
        this._input.DeactivateInputField();
        this._input.ActivateInputField();
    }

    private void HidePassword(){
        this._input.contentType = TMP_InputField.ContentType.Password;
        this._input.DeactivateInputField();
        this._input.ActivateInputField();
    }
}
