using UnityEngine;
using UnityEngine.UI;

public class CurrentVersionDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _text = null;

    // Start is called before the first frame update
    void Start()
    {
        this._text.text = string.Format(Localization.LocalizationManager.Instance.GetString("settings_current_version"), Application.version);
    }
}
