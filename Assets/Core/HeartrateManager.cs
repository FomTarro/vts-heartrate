using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartrateManager : Singleton<HeartrateManager>
{
    [SerializeField]
    private HeartratePlugin _plugin = null;
    public HeartratePlugin Plugin { get { return this._plugin;} }

    private const string VERSION_URL = @"https://www.skeletom.net/vts-heartrate/version";

    public override void Initialize(){
        CheckVersion();
    }

    private void CheckVersion(){
        StartCoroutine(HttpUtils.GetRequest(
            VERSION_URL,
            (e) => {
                Debug.LogError(e);
            },
            (s) => {
                VersionInfo info = JsonUtility.FromJson<VersionInfo>(s);
                Debug.Log(CompareVersion(info) ? "Newer version needed: " + info.url : "Up to date.");
                if(CompareVersion(info)){
                    UIManager.Instance.ShowPopUp(
                        "New Version Available!",
                        string.Format("A newer version of this Plugin is now available.\n"+
                        "<b>Version</b>: {0}\n"+
                        "<b>Release Date:</b> {1}\n\n"+
                        "You can download it at: {2}", info.version, info.date, info.url),
                        new PopUp.PopUpOption(
                            "Download", 
                            true, 
                            () => { Application.OpenURL(info.url); })
                        );
                }
            },
            null
        ));
    }

    /// <summary>
    /// Compares the current version to the remote version. Returns true if the remote is newer.
    /// </summary>
    /// <param name="remoteVersion"></param>
    /// <returns></returns>
    private bool CompareVersion(VersionInfo remoteVersion){
        string currentVersion = Application.version;
        return currentVersion.CompareTo(remoteVersion.version) < 0;
    }

    [System.Serializable]
    public class VersionInfo{
        public string version;
        public string date;
        public string url;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
