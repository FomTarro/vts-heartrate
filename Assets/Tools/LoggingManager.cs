using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingManager : Singleton<LoggingManager>
{
    private Queue<string> _loggingQueue = new Queue<string>();
    public override void Initialize()
    {
        _loggingQueue = new Queue<string>();
    }

    public void Log(string log){
        Debug.Log(log);
        this._loggingQueue.Enqueue(log);
    }

    private void Update(){
        if(this._loggingQueue.Count > 0){
            string log = this._loggingQueue.Dequeue();
            // LogEntry instance = Instantiate<LogEntry>(this._entryPrefab, Vector3.zero, Quaternion.identity, this._logParent);
            // instance.transform.SetSiblingIndex(0);
            // instance.Log(log);
        }
    }
}
