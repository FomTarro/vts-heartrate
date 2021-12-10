using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogEntry : MonoBehaviour
{

    [SerializeField]
    private RectTransform _backing = null;
    [SerializeField]
    private Text _text = null;

    private void OnValidate(){
       SetSize();
    }

    public void Log(string message){
        this._text.text = System.String.Format("[{0}]\n{1}", System.DateTime.Now.ToString(), message);
    }

    private void LateUpdate(){
        SetSize();
    }

    private void SetSize(){
        this._backing.sizeDelta = new Vector2(this._backing.sizeDelta.x, (this._text.rectTransform.sizeDelta.y + 20));
    }
}
