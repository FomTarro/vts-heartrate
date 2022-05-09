using UnityEngine;
using Localization;

public class CurrentVersionDisplay : MonoBehaviour
{
    [SerializeField]
    private LocalizedText _localizedText = null;

    // Start is called before the first frame update

    void Start(){
        this._localizedText.SetPostfix(Application.version);
        this._localizedText.Localize();
    }
}
