using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    
    #region Version
    private void CheckVersion(){
        StartCoroutine(HttpUtils.GetRequest(
            VERSION_URL,
            (e) => {
                Debug.LogError(e);
            },
            (s) => {
                VersionUtils.VersionInfo info = JsonUtility.FromJson<VersionUtils.VersionInfo>(s);
                Debug.Log(VersionUtils.CompareVersion(info) ? "Newer version needed: " + info.url : "Up to date.");
                if(VersionUtils.CompareVersion(info)){
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
                            ColorUtils.ColorPreset.GREEN, 
                            () => { Application.OpenURL(info.url); })
                        );
                }
            },
            null
        ));
    }

    #endregion
}
