using System;
using System.Collections.Generic;

public class ShiftingAverage
{
    private Queue<float> _window;
    private int _size = 1;
    public float Average { 
        get{ 
            float sum = 0.0f;
            foreach(float i in this._window){
                sum += i;
            }
            return sum / Math.Max(1, this._window.Count);
        } 
    }

    public ShiftingAverage(int size){
        this._size = size;
        this._window = new Queue<float>(this._size);
    }

    public void AddValue(float value){
        if(this._window.Count > this._size){
            this._window.Dequeue();
        }
        this._window.Enqueue(value);
    }
}
