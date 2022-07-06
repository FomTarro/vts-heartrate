using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomParameterDisplay : MonoBehaviour
{
    [SerializeField]
    private CustomParameterEntry _paramPrefab = null;
    [SerializeField]
    private RectTransform _entryParent = null;
    // Start is called before the first frame update
    Dictionary<string, CustomParameterEntry> _parameterEntries = new Dictionary<string, CustomParameterEntry>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(string key in HeartrateManager.Instance.Plugin.ParameterMap.Keys){
            if(!this._parameterEntries.ContainsKey(key)){
                CustomParameterEntry entry = Instantiate<CustomParameterEntry>(this._paramPrefab, 
                    Vector3.zero, 
                    Quaternion.identity, 
                    this._entryParent);
                entry.SetValue(key, HeartrateManager.Instance.Plugin.ParameterMap[key]);
                entry.gameObject.SetActive(true);
                this._parameterEntries.Add(key, entry);
            }
        }
        List<string> parameters = new List<string>(this._parameterEntries.Keys);
        foreach(string key in parameters){
            if(!HeartrateManager.Instance.Plugin.ParameterMap.ContainsKey(key)){
                Destroy(this._parameterEntries[key].gameObject);
                this._parameterEntries.Remove(key);
            }else{
                this._parameterEntries[key].SetValue(key, HeartrateManager.Instance.Plugin.ParameterMap[key]);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(this._entryParent);

        // Dictionary<string, float> parameters = HeartrateManager.Instance.Plugin.ParameterMap;
        // List<string> keys = new List<string>(parameters.Keys);
        // keys.Sort();
        // foreach(Transform child in this._entryParent){
        //     Destroy(child.gameObject);
        // }
        // foreach(string key in keys){
        //     CustomParameterEntry entry = Instantiate<CustomParameterEntry>(this._paramPrefab, Vector3.zero, Quaternion.identity, this._entryParent);
        //     entry.SetValue(key, parameters[key]);
        // }
        // LayoutRebuilder.ForceRebuildLayoutImmediate(this._entryParent);
    }
}
