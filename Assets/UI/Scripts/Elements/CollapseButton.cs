using UnityEngine;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class CollapseButton : MonoBehaviour
{
    private ExtendedButton _button = null;
    [SerializeField]
    private GameObject _content = null;

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerUp.AddListener(ToggleCollapse);
    }

    private void ToggleCollapse(){
        _content.gameObject.SetActive(!_content.gameObject.activeSelf);
        UIManager.Instance.ResizeContent();
    }
}
