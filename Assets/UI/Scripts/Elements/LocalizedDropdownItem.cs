using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Localization;

public class LocalizedDropdownItem : MonoBehaviour
{
    [SerializeField]
    private LocalizedText _localizedText = null;
    [SerializeField]
    private Text _text = null;

    private void Start(){
        this._localizedText.ChangeKey(this._text.text);
    }
}
