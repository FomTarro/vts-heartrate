using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class HeartrateInputModule : MonoBehaviour
{
    [SerializeField]
    private Toggle _toggle = null;

    public bool IsActive { get { return this._toggle.isOn; }} 

    public void OnToggle(bool value){
        if(value){
            Activate();
        }
    }

    public void Activate(){
        HeartrateManager.Instance.Plugin.SetActiveHeartrateInput(this);
    }

    public void Deactivate(){
        this._toggle.isOn = false;
    }

    public abstract int GetHeartrate();
}
