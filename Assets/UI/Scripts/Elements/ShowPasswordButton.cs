using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowPasswordButton : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _input = null;

    private void OnValidate(){
        this._input = GetComponentInParent<TMP_InputField>();
        if(this._input != null && this._input.contentType == TMP_InputField.ContentType.Password){
            this.gameObject.SetActive(true);
            // this._input.textViewport.offsetMax = new Vector2(-30;
        }else{
            this.gameObject.SetActive(false);
        }
    }

    public void ShowPassword(){
        this._input.contentType = TMP_InputField.ContentType.Standard;
        this._input.DeactivateInputField();
        this._input.ActivateInputField();
    }

    public void HidePassword(){
        this._input.contentType = TMP_InputField.ContentType.Password;
        this._input.DeactivateInputField();
        this._input.ActivateInputField();
    }
}
