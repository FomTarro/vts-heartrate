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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<string, float> parameters = HeartrateManager.Instance.Plugin.ParameterMap;
        List<string> keys = new List<string>(parameters.Keys);
        keys.Sort();
        foreach(Transform child in this._entryParent){
            Destroy(child.gameObject);
        }
        foreach(string key in keys){
            CustomParameterEntry entry = Instantiate<CustomParameterEntry>(this._paramPrefab, Vector3.zero, Quaternion.identity, this._entryParent);
            entry.SetValue(key, parameters[key]);
        }
        // LayoutRebuilder.ForceRebuildLayoutImmediate(this._entryParent);
    }
}
