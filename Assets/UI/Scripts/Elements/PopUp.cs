using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.InputTools;
using UI.Animation;
using TMPro;

public class PopUp : MonoBehaviour
{

    [SerializeField]
    private TMP_Text _title = null;
    [SerializeField]
    private TMP_Text _content = null;
    [SerializeField]
    private RectTransform _scroll = null;

    [SerializeField]
    private ExtendedButton _buttonPrefab = null;
    [SerializeField]
    private RectTransform _buttonParent = null;
    [SerializeField]
    private GridLayoutGroup _buttonLayout = null;

    [SerializeField]
    private ExtendedButton _closeButton = null;

    [SerializeField]
    private AnimatedFlash _flash = null;

    [SerializeField]
    private RectTransform _layoutGroup = null;

    private bool _show = false;


    // Start is called before the first frame update
    void Start(){
        this._closeButton.onPointerUp.AddListener(() => { this.Hide(); });
        // this.Hide();
    }

    public void Show(string titleKey, string bodyKey, params PopUpOption[] options){
        this._title.text = Localization.LocalizationManager.Instance.GetString(titleKey);
        if(bodyKey != null && bodyKey.Length >= 0){
            this._content.text = Localization.LocalizationManager.Instance.GetString(bodyKey);
        }else{
            this._content.text = "";
        }
        foreach(Transform child in this._buttonParent){
            Destroy(child.gameObject);
        }
        foreach(PopUpOption option in options){
            ExtendedButton button = Instantiate(
                this._buttonPrefab, 
                Vector3.zero, 
                Quaternion.identity, 
                this._buttonParent);
            button.GetComponent<Image>().color = ColorUtils.ColorPresetToUnityColor(option.color);
            button.GetComponentInChildren<TMP_Text>().text = Localization.LocalizationManager.Instance.GetString(option.text);
            button.onPointerUp.AddListener(() => {option.callback();});
        }
        this._buttonParent.gameObject.SetActive(options.Length > 0);
        this._buttonLayout.cellSize = new Vector2(options.Length > 1 ? 160 : 330, 30);
        this._show = true;
        this._flash.SetAlpha(0);
        this.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this._layoutGroup);
        this._scroll.sizeDelta = new Vector2(this._scroll.sizeDelta.x, Mathf.Min(400, this._content.preferredHeight));
        this._flash.StartAnimation(ShouldShow, 
        () => {},
        () => {},
        () => {this.gameObject.SetActive(false);});
    }

    private bool ShouldShow(){
        return !this._show;
    }

    public void Hide(){
        this._show = false;
    }

    public struct PopUpOption{
        public string text;
        public ColorUtils.ColorPreset color;
        public System.Action callback;

        public PopUpOption(string text, ColorUtils.ColorPreset color, System.Action callback){
            this.text = text;
            this.color = color;
            this.callback = callback;
        }

    }
}
