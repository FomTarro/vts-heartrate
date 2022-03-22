using UnityEngine;
using System;

public static class MathUtils
{
    /// <summary>
    /// Attempts to convert a string into a byte
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte StringToByte(string value){
        try{
            return Convert.ToByte(value);
        }catch(Exception e){
            Debug.LogWarning(e);
            return 0;
        }
    }
}
