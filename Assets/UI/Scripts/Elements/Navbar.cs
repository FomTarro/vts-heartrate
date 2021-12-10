using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Animation;

public class Navbar : MonoBehaviour
{
    [SerializeField]
    private Text _heartRate = null;
    [SerializeField]
    private AnimatedScale _heartRateAnimation = null;

    // [SerializeField]
    // private InputField _minField = null;
    // [SerializeField]
    // private InputField _maxField = null;

    [Header("Tabs")]
    [SerializeField]
    private List<TabMapper> _tabs = new List<TabMapper>();

    [SerializeField]
    private ScrollRect _scroll = null;

    private RectTransform _selected = null;


    // Start is called before the first frame update
    void Start()
    {
        GoTo(Tabs.HEARTRATE_INPUTS);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this._heartRateAnimation.scaleTime = 60f/((float)HeartrateManager.Instance.Plugin.HeartRate);
        this._heartRate.text = HeartrateManager.Instance.Plugin.HeartRate.ToString();
        this._scroll.content.sizeDelta = new Vector2(this._scroll.content.sizeDelta.x, this._selected.rect.height);
    }

    public void GoTo(int tab){
        Tabs value = (Tabs)tab;
        GoTo(value);
    }

    private void GoTo(Tabs tab){
        foreach(TabMapper entry in this._tabs){
            entry.element.gameObject.SetActive(entry.tab == tab);
            if(entry.tab == tab){
                _selected = entry.element;
            }
        }
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
    }
}
