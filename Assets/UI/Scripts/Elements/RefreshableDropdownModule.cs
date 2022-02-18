using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.InputTools;

public class RefreshableDropdownModule : MonoBehaviour
{
    [SerializeField]
    private RefreshableDropdown _dropdown = null;
    [SerializeField]
    private ExtendedButton _button = null;

    private void OnValidate(){
        this._dropdown = GetComponentInChildren<RefreshableDropdown>();
    }
    // Start is called before the first frame update
    void Start()
    {
        this._button.onPointerUp.AddListener(this._dropdown.Refresh);
    }
}
