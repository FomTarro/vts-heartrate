using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.InputTools;
using UI.Animation;

public class PopUp : MonoBehaviour
{

    [SerializeField]
    private Text _title = null;
    [SerializeField]
    private Text _content = null;

    [SerializeField]
    private ExtendedButton _buttonPrefab = null;
    [SerializeField]
    private RectTransform _buttonParent = null;

    [SerializeField]
    private ExtendedButton _closeButton = null;

    [SerializeField]
    private AnimatedFlash _flash = null;

    private bool _show = false;


    // Start is called before the first frame update
    void Start(){
        this._closeButton.onPointerUp.AddListener(() => { this.Hide(); });
        // this.Hide();
    }

    public void Show(string title, string body, params PopUpOption[] options){
        this._title.text = title;
        this._content.text = body;
        foreach(Transform child in this._buttonParent){
            Destroy(child.gameObject);
        }
        foreach(PopUpOption option in options){
            ExtendedButton button = Instantiate(
                this._buttonPrefab, 
                Vector3.zero, 
                Quaternion.identity, 
                this._buttonParent);
            button.GetComponentInChildren<Text>().text = option.text;
            button.onPointerUp.AddListener(() => {option.callback();});
        }
        this._show = true;
        this._flash.SetAlpha(0);
        this.gameObject.SetActive(true);
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
        public Color color;
        public System.Action callback;

        public PopUpOption(string text, Color color, System.Action callback){
            this.text = text;
            this.color = color;
            this.callback = callback;
        }
    }
}
