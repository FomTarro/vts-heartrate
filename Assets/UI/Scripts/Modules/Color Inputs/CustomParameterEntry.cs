using UnityEngine;
using UnityEngine.UI;

public class CustomParameterEntry : MonoBehaviour
{
    [SerializeField]
    private Text _title = null;
    [SerializeField]
    private Text _value = null;

    public void SetValue(string title, float value){
        this._title.text = title;
        this._value.text = value.ToString("0.0000");
    }
}
