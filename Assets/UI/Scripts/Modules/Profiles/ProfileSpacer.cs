using UnityEngine;
using TMPro;

public class ProfileSpacer : MonoBehaviour {
    
    [SerializeField]
    private TMP_Text _text = null;
    [SerializeField]
    private RectTransform _content = null;
    public RectTransform Content { get { return this._content; } }
    [SerializeField]
    private CollapseButton _collapse = null;

    public void Configure(string title){
        this._text.text = title;
    }

    public void ToggleCollapse(bool state){
        this._collapse.ToggleCollapse(state);
    }

}
