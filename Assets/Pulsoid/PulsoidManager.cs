using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsoidManager : Singleton<PulsoidManager>
{
    private const string PULSOID_URL = 
        @"https://pulsoid.net/oauth2/authorize?client_id={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}";
    private const string PULSOID_REDIRECT_URL = @"https://www.skeletom.net/vts-heartrate/oauth2/pulsoid";
    private string PULSOID_STATE_TOKEN = "";
    public override void Initialize()
    {
        this.PULSOID_STATE_TOKEN = new System.Random().Next(99999).ToString();
    }

    public void Login(){
        string uri = string.Format(
            PULSOID_URL, 
            PulsoidCredentials.CLIENT_ID, 
            PULSOID_REDIRECT_URL, 
            @"data:heart_rate:read", 
            PULSOID_STATE_TOKEN);
        Application.OpenURL(uri);  
    }

}
