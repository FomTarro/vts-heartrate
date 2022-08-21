using UnityEngine;
using TMPro;

public class ProfileSpacer : MonoBehaviour {
    
    [SerializeField]
    private TMP_Text _text = null;

    public void Configure(string title){
        this._text.text = title;
    }

}
