using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrentProfileDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;

    // Update is called once per frame
    void Update()
    {
        this._text.text = SaveDataManager.Instance.CurrentProfile.NameAndProfile;
    }
}
