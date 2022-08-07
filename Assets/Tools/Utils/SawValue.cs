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

    public float GetValue(int heartRate){
        float delta = (Time.deltaTime * ((float)heartRate)/(float)this._beatsPerCycle);
        this._time = this._pos >= 1 ? 0 : this._time + delta;
        this._pos = Mathf.Lerp(0, 1, this._time / 60f);
        // this._time = this._time > this._beatsPerCycle*(1f/newFrequency) ? 0 : this._time + Time.deltaTime;
        // this._value = this._time / (this._beatsPerCycle*(1f/newFrequency));
        return this._pos;
    }
}
