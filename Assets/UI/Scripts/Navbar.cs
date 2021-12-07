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

    [SerializeField]
    private InputField _minField = null;
    [SerializeField]
    private InputField _maxField = null;

    [Header("Tabs")]
    [SerializeField]
    private List<TabMapper> _tabs = new List<TabMapper>();


    // Start is called before the first frame update
    void Start()
    {
        GoTo(Tabs.HEARTRATE_INPUTS);
        this._minField.onEndEdit.AddListener(SetMinRate);
        this._maxField.onEndEdit.AddListener(SetMaxRate);
    }

    // Update is called once per frame
    void Update()
    {
        this._heartRateAnimation.scaleTime = 60f/((float)HeartrateManager.Instance.Plugin.HeartRate);
        this._heartRate.text = HeartrateManager.Instance.Plugin.HeartRate.ToString();
    }

    public void SetMinRate(string s){
        int rate = RateToInt(s);
        HeartrateManager.Instance.Plugin.SetMinRate(rate);
        this._minField.text = rate.ToString();
    }

    public void SetMaxRate(string s){
        int rate = RateToInt(s);
        HeartrateManager.Instance.Plugin.SetMaxRate(rate);
        this._maxField.text = rate.ToString();
    }

    private int RateToInt(string rate){
        try{
            return int.Parse(rate);
        }catch(System.Exception e){
            Debug.LogWarning(e);
        }
        return 0;
    }

    public void GoTo(int tab){
        Tabs value = (Tabs)tab;
        GoTo(value);
    }

    private void GoTo(Tabs tab){
        foreach(TabMapper entry in this._tabs){
            entry.element.gameObject.SetActive(entry.tab == tab);
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
