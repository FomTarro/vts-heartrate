using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawValue
{
    private float _time = 0;  
    private int _beatsPerCycle = 1;
    private float _pos;


    public SawValue(int beatsPerCycle){
        this._beatsPerCycle = beatsPerCycle;
    }

    public float GetValue(float hertz){
        float delta = (Time.deltaTime * (hertz)/(float)this._beatsPerCycle);
        this._time = this._pos >= 1 ? 0 : this._time + delta;
        this._pos = Mathf.Lerp(0, 1, this._time);
        return this._pos;
    }
}
