using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class RefreshableDropdown<T> : MonoBehaviour
{
    [SerializeField]
    private Dropdown _dropdown = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public abstract void Refresh();

    private void RefreshValues(List<T> values){
        int currentIndex = this._dropdown.value;
        string currentSelection = 
            this._dropdown.options.Count > 0 ? 
            this._dropdown.options[currentIndex].text : 
            null;
        values.Sort();
        List<string> options = new List<string>();
        foreach(T value in values){
            options.Add(value.ToString());
        }
        this._dropdown.ClearOptions();
        this._dropdown.AddOptions(options);
        this._dropdown.RefreshShownValue();
        this._dropdown.SetValueWithoutNotify(
            Mathf.Min(this._dropdown.options.Count, 
            StringToIndex(currentSelection)));
    }

    private int StringToIndex(string val){
        return this._dropdown.options.FindIndex((o) 
            => { return o.text.Equals(val); });
    }
}
