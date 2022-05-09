using System.Collections.Generic;
using UnityEngine;

public class HeartrateManager : Singleton<HeartrateManager>
{
    [SerializeField]
    private HeartratePlugin _plugin = null;
    public HeartratePlugin Plugin { get { return this._plugin;} }

    private const string VERSION_URL = @"https://www.skeletom.net/vts-heartrate/version";

    public override void Initialize(){
        this.Plugin.OnLaunch();
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
                    Dictionary<string, string> strings = new Dictionary<string, string>();
                    strings.Add("settings_new_version_body_populated", 
                        string.Format(Localization.LocalizationManager.Instance.GetString("settings_new_version_body"), 
                        info.version, 
                        info.date, 
                        info.url));
                    Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
                    UIManager.Instance.ShowPopUp(
                        "settings_new_version_title",
                        string.Format("settings_new_version_body_populated", info.version, info.date, info.url),
                        new PopUp.PopUpOption(
                            "settings_new_version_button_download", 
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
