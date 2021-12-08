using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingManager : Singleton<LoggingManager>
{
    [SerializeField]
    private LogEntry _entryPrefab = null;
    [SerializeField]
    private RectTransform _logParent = null;
    public override void Initialize()
    {
        
    }

    public void Log(string log){
        Debug.Log(log);
        LogEntry instance = Instantiate<LogEntry>(this._entryPrefab, Vector3.zero, Quaternion.identity, this._logParent);
        instance.transform.SetSiblingIndex(0);
        instance.Log(log);
    }
}
