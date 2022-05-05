using UnityEngine;
using TMPro;

public class CurrentVersionDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text = null;

    // Start is called before the first frame update
    void Start()
    {
        this._text.text = string.Format(Localization.LocalizationManager.Instance.GetString("settings_current_version"), Application.version);
    }
}
