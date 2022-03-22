using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillatingValue
{
    private float _frequency = 0;
    private float _time = 0;  

    public float GetValue(float newFrequency){
        float radians = (2 * Mathf.PI) * this._frequency * this._time;
        // reset the time value to 0 after a complete cycle (2*Pi radians)
        this._time = radians >= (Mathf.PI * 2) || this._frequency == 0  ? 0f : this._time;
        // Only change frequency after a complete cycle to avoid stuttering
        this._frequency = this._time == 0f ? newFrequency : this._frequency;
        this._time = this._time + Time.deltaTime;
        return 0.5f * (1 + Mathf.Sin((2 * Mathf.PI) * this._frequency * this._time));
    }  
}
