using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.InputTools;
using TMPro;

public class PluginAuthEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title = null;
    [SerializeField]
    private ExtendedButton _button = null;
    private string _token;

    

    public void Configure(APIManager.PluginData data){
        this._title.text = data.pluginName;
        this._token = data.token;
        this._button.onPointerUp.AddListener(() => { 
            APIManager.Instance.RevokeTokenData(this._token); 
            Destroy(this.gameObject);
        });

    }
}
