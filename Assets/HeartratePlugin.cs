using VTS.Networking.Impl;
using VTS.Models.Impl;
using VTS.Models;
using VTS;

using UnityEngine;
using System;
using System.Collections.Generic;
public class HeartratePlugin : VTSPlugin
{
    private Color32 _color = Color.white;
    private string[] _matchers = new string[0];

    [SerializeField]
    [Range(50, 120)]
    private int _heartRate = 70;
    private int _maxRate = 100;
    private int _minRate = 70;

    // Start is called before the first frame update
    void Start()
    {
        // Everything you need to get started!
        Initialize(
            new WebSocketImpl(),
            new JsonUtilityImpl(),
            new TokenStorageImpl(),
            () => {Debug.Log("Connected!");},
            () => {Debug.LogWarning("Disconnected!");},
            () => {Debug.LogError("Error!");});
    }

    private void FixedUpdate(){
        ArtMeshMatcher matcher = new ArtMeshMatcher();
        matcher.tintAll = false;
        matcher.nameContains = this._matchers;
        this.TintArtMesh(
            Color32.Lerp(Color.white, this._color, 
            ((float)(this._heartRate-this._minRate)/(float)(this._maxRate - this._minRate))),  
            0.5f, 
            matcher,
            (success) => {

            },
            (error) => {

            });
    }

    public void SetRed(string value){
        this._color = new Color32(
            StringToByte(value), 
            this._color.g, 
            this._color.b, 
            this._color.a);
    }

    public void SetGreen(string value){
        this._color = new Color32(
            this._color.r, 
            StringToByte(value), 
            this._color.b,
            this._color.a);
    }

    public void SetBlue(string value){
        this._color = new Color32(
            this._color.r, 
            this._color.g, 
            StringToByte(value), 
            this._color.a);
    }

    public void SetAlpha(string value){
        this._color = new Color32(
            this._color.r, 
            this._color.g, 
            this._color.b,
            StringToByte(value));
    }

    public void SetMatchers(string value){
        String[] sanitized = value.Trim().Split(' ', ',');
        this._matchers = sanitized;
    }


    private byte StringToByte(string value){
        try{
            return Convert.ToByte(value);
        }catch(Exception e){
            return 0;
        }
    }
}
