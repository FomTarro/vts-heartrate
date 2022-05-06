using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Localization;

public class LocalizedDropdownItem : MonoBehaviour
{
    [SerializeField]
    private bool _textAsKey = true;
    [SerializeField]
    private LocalizedText _localizedText = null;
    [SerializeField]
    private TMP_Text _displayText = null;
    [SerializeField]
    private Text _text = null;

    private void Start(){
        if(this._textAsKey){
            this._localizedText.ChangeKey(this._text.text);
        }
    }

    private void Update(){
        if(this._textAsKey && !this._text.text.Equals(this._localizedText.Key)){
            this._localizedText.ChangeKey(this._text.text);
        }
    }
}
