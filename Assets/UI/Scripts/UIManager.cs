﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class UIManager : Singleton<UIManager>, IEventPublisher<UIManager.Tabs>
{
    [Header("Tabs")]
    [SerializeField]
    private List<TabMapper> _tabs = new List<TabMapper>();
    private Dictionary<Tabs, UnityEvent> _tabEvents = new Dictionary<Tabs, UnityEvent>();

    [SerializeField]
    private ScrollRect _scroll = null;

    private RectTransform _selected = null;

    [SerializeField]
    private PopUp _popUp = null;

    public static readonly float UI_CYCLE_TIME = 0.35f;


    // Start is called before the first frame update
    void Start()
    {
        GoTo(Tabs.HEARTRATE_INPUTS);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (this._requestRebuild)
        {
            foreach (TabMapper entry in this._tabs)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(entry.element);
            }
            this._requestRebuild = false;
        }
        this._scroll.content.sizeDelta = new Vector2(this._scroll.content.sizeDelta.x, this._selected.rect.height);
    }

    public override void Initialize()
    {
        foreach (TabMapper entry in this._tabs)
        {
            entry.element.gameObject.SetActive(true);
        }
    }

    public EventCallbackRegistration RegisterEventCallback(Tabs tab, System.Action onSelect)
    {
        if (!this._tabEvents.ContainsKey(tab))
        {
            this._tabEvents.Add(tab, new UnityEvent());
        }
        this._tabEvents[tab].AddListener(new UnityAction(onSelect));
        return new EventCallbackRegistration(System.Guid.NewGuid().ToString());
    }

    public void UnregisterEventCallback(EventCallbackRegistration registration)
    {
        // TODO
    }

    public void GoTo(Tabs tab)
    {
        foreach (TabMapper entry in this._tabs)
        {
            CanvasGroup group = entry.element.GetComponent<CanvasGroup>();
            group.interactable = entry.tab == tab;
            group.blocksRaycasts = group.interactable;
            group.alpha = entry.tab == tab ? 1.0f : 0.0f;
            foreach (RectTransform other in entry.others)
            {
                other.gameObject.SetActive(entry.tab == tab);
            }
            if (entry.tab == tab)
            {
                _selected = entry.element;
                if (this._tabEvents.ContainsKey(tab))
                {
                    this._tabEvents[tab].Invoke();
                }
            }
        }
        this._scroll.verticalNormalizedPosition = 1;
    }

    private bool _requestRebuild = false;
    public void ResizeContent()
    {
        // foreach (TabMapper entry in this._tabs)
        // {
        //     LayoutRebuilder.ForceRebuildLayoutImmediate(entry.element);
        // }
        this._requestRebuild = true;
    }

    public void ShowPopUp(string titleKey, string bodyKey, params PopUp.PopUpOption[] options)
    {
        this._popUp.Show(titleKey, bodyKey, options);
    }

    public void HidePopUp()
    {
        this._popUp.Hide();
    }

    [System.Serializable]
    public class TabMapper
    {
        public Tabs tab;
        public RectTransform element;
        public RectTransform[] others;
    }

    [System.Serializable]
    public enum Tabs : int
    {
        HEARTRATE_INPUTS = 1,
        OUTPUTS = 2,
        DEBUG_LOGS = 3,
        SETTINGS = 4,
        PROFILES = 5,
    }
}
