using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortSelector : MonoBehaviour
{

    [SerializeField]
    private Dropdown _dropdown = null;

    private List<string> _portNumbers = new List<string>();

    public void Start(){
        this._dropdown.onValueChanged.AddListener((i) => { 
            SetPort(i);
        });
    }

    public void OnEnable(){
        try{
            RefreshPorts();
        }catch(System.Exception){

        }
    }

    public void RefreshPorts(){
        int currentIndex = this._dropdown.value;
        List<int> ports = new List<int>(HeartrateManager.Instance.Plugin.GetPorts().Keys);
        ports.Sort();
        this._portNumbers = new List<string>();
        foreach(int port in ports){
            this._portNumbers.Add(port.ToString());
        }
        this._dropdown.ClearOptions();
        this._dropdown.AddOptions(this._portNumbers);
        this._dropdown.RefreshShownValue();
        this._dropdown.SetValueWithoutNotify(Mathf.Min(this._dropdown.options.Count, currentIndex));
    }

    public void SetPort(int index){
        HeartrateManager.Instance.Plugin.SetPort(int.Parse(this._portNumbers[index]));
        HeartrateManager.Instance.Plugin.Connect();
    }
}
