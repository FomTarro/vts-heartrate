using System.Collections;
using System.Collections.Generic;
using UI.InputTools;
using UnityEngine;

public class WidgetOutput : MonoBehaviour
{
    [SerializeField]
    private ExtendedButton _button;
    private void Start()
    {
        _button.onPointerUp.AddListener(GetWidget);
    }
    public void GetWidget()
    {
        Application.OpenURL(WidgetURL());
    }

    private string WidgetURL()
    {
        return string.Format(
            "https://www.skeletom.net/vts-heartrate/widget?port={0}",
            APIManager.Instance.Port);
    }
}
