using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Navbar : MonoBehaviour
{
    [SerializeField]
    private Text _heartRate = null;

    [Header("Tabs")]
    [SerializeField]
    private List<TabMapper> _tabs = new List<TabMapper>();


    // Start is called before the first frame update
    void Start()
    {

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

    // Update is called once per frame
    void Update()
    {
        this._heartRate.text = HeartrateManager.Instance.Plugin.HeartRate.ToString();
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
