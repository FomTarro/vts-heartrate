using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Animation;

public class LoadScreen : MonoBehaviour
{
    [SerializeField]
    private AnimatedFlash _flash = null;

    public void Load(){
        this.gameObject.SetActive(true);
        this._flash.StartAnimation( () => { return this._loaded == true; }, 
        () => {}, 
        () => {}, 
        () => { this.gameObject.SetActive(false); }
        );
        StartCoroutine(FlipPages());
    }

    private bool _loaded = false;
    private IEnumerator FlipPages(){
        float f = 0.1f;
        yield return new WaitForSeconds(f);
        UIManager.Instance.GoTo(UIManager.Tabs.OUTPUTS);
        yield return new WaitForSeconds(f);
        UIManager.Instance.GoTo(UIManager.Tabs.SETTINGS);
        yield return new WaitForSeconds(f);
        UIManager.Instance.GoTo(UIManager.Tabs.HEARTRATE_INPUTS);
        this._loaded = true;
    }
}
