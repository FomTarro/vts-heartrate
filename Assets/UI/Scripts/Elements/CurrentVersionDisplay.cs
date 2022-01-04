using UnityEngine;
using UnityEngine.UI;

public class CurrentVersionDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _text = null;

    // Start is called before the first frame update
    void Start()
    {
        this._text.text = string.Format("Current Version: {0}", Application.version);
    }
}
