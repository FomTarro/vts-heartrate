using UnityEngine;
using TMPro;

public class CustomParameterEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title = null;
    [SerializeField]
    private TMP_Text _value = null;

    public void SetValue(string title, float value){
        this._title.text = title;
        this._value.text = value.ToString("0.0000");
    }
}
