﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Animation;

public class HeartrateInfo : MonoBehaviour
{
    [SerializeField]
    private Text _heartRate = null;
    [SerializeField]
    private AnimatedScale _heartRateAnimation = null;

    // Update is called once per frame
    void LateUpdate()
    {
        this._heartRateAnimation.scaleTime = 60f/((float)HeartrateManager.Instance.Plugin.HeartRate);
        this._heartRate.text = HeartrateManager.Instance.Plugin.HeartRate.ToString();
    }
}