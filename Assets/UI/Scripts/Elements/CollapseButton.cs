using UnityEngine;
using UnityEngine.UI;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class CollapseButton : MonoBehaviour
{
    private ExtendedButton _button = null;
    [SerializeField]
    private ExtendedButton _title = null;
    [SerializeField]
    private GameObject _content = null;
    [SerializeField]
    private TMPro.TMP_Text _text = null;
    [SerializeField]
    private Image _image = null;
    [SerializeField]
    private bool _initialState = true;

    private const string COLLAPSED_GLYPH = "🠶";
    private const string EXPANDED_GLYPH = "🠷";

    // Start is called before the first frame update
    void Start(){
        this._button = GetComponent<ExtendedButton>();
        if(this._content != null){
            this._button.onPointerUp.AddListener(() => { ToggleCollapse(!_content.gameObject.activeSelf); });
            if(this._title != null){
                this._title.onPointerUp.AddListener(() => { ToggleCollapse(!_content.gameObject.activeSelf); });
            }
        }
    }
    private void OnEnable(){
        this._text.text = this._initialState ? EXPANDED_GLYPH : COLLAPSED_GLYPH;
        ToggleCollapse(this._initialState);
    }

    private void ToggleCollapse(bool state){
        if(this._content != null){
            this._content.gameObject.SetActive(state);
            this._text.text = state ? EXPANDED_GLYPH : COLLAPSED_GLYPH;
            this._image.rectTransform.eulerAngles = new Vector3(0, 0, state ? 0 : 90);
            UIManager.Instance.ResizeContent();
        }
    }
}
