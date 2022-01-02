using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Tabs")]
    [SerializeField]
    private List<TabMapper> _tabs = new List<TabMapper>();

    [SerializeField]
    private ScrollRect _scroll = null;

    private RectTransform _selected = null;

    [SerializeField]
    private PopUp _popUp = null; 


    // Start is called before the first frame update
    void Start()
    {
        GoTo(Tabs.HEARTRATE_INPUTS);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this._scroll.content.sizeDelta = new Vector2(this._scroll.content.sizeDelta.x, this._selected.rect.height);
    }

    public override void Initialize()
    {
        
    }

    public void GoTo(Tabs tab){
        foreach(TabMapper entry in this._tabs){
            entry.element.gameObject.SetActive(entry.tab == tab);
            if(entry.tab == tab){
                _selected = entry.element;
            }
        }
    }

    public void ShowPopUp(string title, string body, params PopUp.PopUpOption[] options){
        this._popUp.Show(title, body, options);
    }

    public void HidePopUp(){
        this._popUp.Hide();
    }

    [System.Serializable]
    public class TabMapper {
        public Tabs tab;
        public RectTransform element;
    }

    [System.Serializable]
    public enum Tabs : int {
        HEARTRATE_INPUTS = 1,
        COLOR_INPUTS = 2,
        DEBUG_LOGS = 3,
        SETTINGS = 4,
    }
}
