using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PulsoidManager : Singleton<PulsoidManager>
{

    public override void Initialize()
    {
        this.PULSOID_STATE_TOKEN = new System.Random().Next(99999).ToString();
    }

    #region App-based

    private const string PULSOID_LOGIN_URL = 
        @"https://pulsoid.net/oauth2/authorize?client_id={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}";
    private const string PULSOID_REDIRECT_URL = 
        @"https://www.skeletom.net/vts-heartrate/oauth2/pulsoid";
    private string PULSOID_STATE_TOKEN = "";

    private const string PULSOID_APP_URL = 
        @"https://dev.pulsoid.net/api/v1/data/heart_rate/latest";

    public void Login(){
        string uri = string.Format(
            PULSOID_LOGIN_URL, 
            PulsoidCredentials.CLIENT_ID, 
            PULSOID_REDIRECT_URL, 
            @"data:heart_rate:read", 
            PULSOID_STATE_TOKEN);
        Application.OpenURL(uri);  
    }

    private int _appHeartrate = 0;
    public int AppHeartrate { get  {return this._appHeartrate; } }
    private string _appToken = null;
    [SerializeField]
    private float _appRefreshInterval = 0.5f;
    private Coroutine _appRequestLoop = null;

    public void SetAuthToken(string token){
        this._appToken = token;
    }

    public void ToggleAppRequestLoop(bool toggle){
        if(this._appRequestLoop != null){
            StopCoroutine(this._appRequestLoop);
        }
        if(toggle == true){
            this._appRequestLoop = StartCoroutine(GetAppData());
        }
    }

    IEnumerator GetAppData()
    {
        while(true){
            yield return HttpUtils.GetRequest(PULSOID_APP_URL, 
            (e) => { 
                // TODO: error handling on the UI
            }, 
            (s) => {
                PulsoidAppResult result = JsonUtility.FromJson<PulsoidAppResult>(s);
                this._appHeartrate = result.data.heart_rate;
            },
            this._appToken);
            float t = 0;
            while(t < this._appRefreshInterval){
                t+= Time.deltaTime;
                yield return null;
            } 
        }
    }

    [System.Serializable]
    public class PulsoidAppResult{
        public string measured_at;
        public Data data;

        [System.Serializable]
        public class Data {
            public int heart_rate;
        }
    }

    #endregion

    #region Feed-based

    private int _feedHeartrate = 0;
    public int FeedHeartrate { get  {return this._feedHeartrate; } }
    private string _feedURL = null;
    [SerializeField]
    private float _feedRefreshInterval = 0.5f;
    private Coroutine _feedRequestLoop = null;

    public void SetFeedURL(string url){
        //do a regex to validate pattern
        this._feedURL = url;
    }

    public void ToggleFeedRequestLoop(bool toggle){
        if(this._feedRequestLoop != null){
            StopCoroutine(this._feedRequestLoop);
        }
        if(toggle == true){
            this._feedRequestLoop = StartCoroutine(GetFeedData());
        }
    }

    IEnumerator GetFeedData()
    {
        while(true){
            yield return HttpUtils.GetRequest(this._feedURL, 
            (e) => { 
                // TODO: error handling on the UI
            }, 
            (s) => {
                PulsoidFeedResult result = JsonUtility.FromJson<PulsoidFeedResult>(s);
                this._feedHeartrate = result.bpm;
            },
            null);
            float t = 0;
            while(t < this._feedRefreshInterval){
                t+= Time.deltaTime;
                yield return null;
            } 
        }
    }

    [System.Serializable]
    public class PulsoidFeedResult {
        public int bpm;
        public string measured_at;
    }

    #endregion

}
