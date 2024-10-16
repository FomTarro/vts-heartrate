﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class CollapseButton : MonoBehaviour
{
    private ExtendedButton _button = null;
    [SerializeField]
    private ExtendedButton _title = null;

    [SerializeField]
    private Image _image = null;

    [SerializeField]
    private bool _initialState = true;
    private bool _toggleState = false;

    [SerializeField]
    private RectTransform _root = null;
    [SerializeField]
    private List<GameObject> _content = new List<GameObject>();
    [SerializeField]
    private List<GameObject> _minimizedContent = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        if (this._content != null && this._content.Count > 0)
        {
            this._button.onPointerUp.AddListener(() => { ToggleCollapse(!this._toggleState); });
            if (this._title != null)
            {
                this._title.onPointerUp.AddListener(() => { ToggleCollapse(!this._toggleState); });
            }
        }
    }
    private void OnEnable()
    {
        ToggleCollapse(this._initialState);
    }

    public void ToggleCollapse(bool state)
    {
        if (this._content != null && this._content.Count > 0)
        {
            foreach (GameObject go in this._content)
            {
                // go.transform.localScale = new Vector3(1, state ? 1 : 0, 1);
                go.SetActive(state);
            }
            if (this._minimizedContent != null && this._minimizedContent.Count > 0)
            {
                foreach (GameObject go in this._minimizedContent)
                {
                    go.SetActive(!state);
                }
            }
            this._image.rectTransform.eulerAngles = new Vector3(0, 0, state ? 0 : 90);
            if (this._root != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this._root);
                LayoutRebuilder.ForceRebuildLayoutImmediate(this._root.GetComponentInParent<RectTransform>());
            }
            UIManager.Instance.ResizeContent();
        }
        this._toggleState = state;
    }
}
