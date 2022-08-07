using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawValue
{
    private float _time = 0;  
    private int _beatsPerCycle = 1;
    private float _value;

    public SawValue(int beatsPerCycle){
        this._beatsPerCycle = beatsPerCycle;
    }

    public float GetValue(int newFrequency){
        newFrequency = newFrequency <= 0 ? 1 : newFrequency;
        // this._time = this._time > this._beatsPerCycle*(1f/newFrequency) ? 0 : this._time + Time.deltaTime;
        // this._value = this._time / (this._beatsPerCycle*(1f/newFrequency));
        // this._time = this._time <= 0 ? this._beatsPerCycle*(1f/newFrequency) : this._time - Time.deltaTime;
        return Mathf.Lerp(0, 1, _value);
    }
}
