﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartrateManager : Singleton<HeartrateManager>
{
    [SerializeField]
    private HeartratePlugin _plugin = null;
    public HeartratePlugin Plugin { get { return this._plugin;} }

    public override void Initialize(){}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}