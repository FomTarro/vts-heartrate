using UnityEngine;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class GoToButton : MonoBehaviour
{
    private ExtendedButton _button = null;
    [SerializeField]
    private UIManager.Tabs _tab;

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerUp.AddListener(() => { UIManager.Instance.GoTo(this._tab); });
    }
}
