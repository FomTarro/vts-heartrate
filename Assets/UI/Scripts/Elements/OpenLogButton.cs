using UnityEngine;
using UI.InputTools;
using System.IO;

[RequireComponent(typeof(ExtendedButton))]
public class OpenLogButton : MonoBehaviour
{
    private ExtendedButton _button = null;

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerUp.AddListener(() => {Application.OpenURL(Path.Combine(Application.persistentDataPath, "Player.log"));});
    }
}

