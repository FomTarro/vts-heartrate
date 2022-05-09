using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Localization;

public class DropdownItemLabel : MonoBehaviour
{
    [SerializeField]
    private bool _textAsKey = true;
    [SerializeField]
    private LocalizedText _localizedText = null;
    [SerializeField]
    private TMP_Text _displayText = null;
    [SerializeField]
    private TMP_Text _backingText = null;

    private void OnValidate(){
        this._localizedText.enabled = this._textAsKey;
    }

    private void Start(){ 
        if(this._textAsKey){
            this._localizedText.ChangeKey(this._backingText.text);
        }
    }

    private void Update(){
        if(this._textAsKey && !this._backingText.text.Equals(this._localizedText.Key)){
            this._localizedText.ChangeKey(this._backingText.text);
        }else if(!this._textAsKey && !this._backingText.text.Equals(this._displayText.text)){
            this._displayText.text = this._backingText.text;
        }
    }
}
