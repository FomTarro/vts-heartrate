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
    private bool _appRequestIsLooping = false;

    public void SetAuthToken(string token){
        this._appToken = token;
    }

    public void ToggleAppRequestLoop(bool toggle, System.Action<HttpUtils.ConnectionStatus> onStatus){
        if(this._appRequestLoop != null){
            this._appRequestIsLooping = false;
        }
        if(toggle == true){
            HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
            status.status = HttpUtils.ConnectionStatus.Status.CONNECTING;
            onStatus.Invoke(status);
            this._appRequestIsLooping = true;
            this._appRequestLoop = StartCoroutine(GetAppData(onStatus));
        }
    }

    IEnumerator GetAppData(System.Action<HttpUtils.ConnectionStatus> onStatus){
        while(this._appRequestIsLooping){
            yield return HttpUtils.GetRequest(PULSOID_APP_URL, 
            (e) => { 
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.ERROR;
                if(e.statusCode == 403 || e.statusCode == 401){
                    status.message = "Invalid token";
                }else{
                    status.message = e.message;
                }
                onStatus.Invoke(status);
            }, 
            (s) => {
                PulsoidAppResult result = JsonUtility.FromJson<PulsoidAppResult>(s);
                this._appHeartrate = result.data.heart_rate;
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
                onStatus.Invoke(status);
            },
            this._appToken);
            float t = 0;
            while(t < this._appRefreshInterval){
                t+= Time.deltaTime;
                yield return null;
            } 
        }
        HttpUtils.ConnectionStatus exitStatus = new HttpUtils.ConnectionStatus();
        exitStatus.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
        onStatus.Invoke(exitStatus);
    }

    [System.Serializable]
    public class PulsoidAppResult{
        public string measured_at;
        public Data data;

        [System.Serializable]
        public class Data{
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
    private bool _feedRequestIsLooping = false;

    public void SetFeedURL(string url){
        //do a regex to validate pattern
        this._feedURL = url;
    }

    public void ToggleFeedRequestLoop(bool toggle, System.Action<HttpUtils.ConnectionStatus> onStatus){
        if(this._feedRequestLoop != null){
            this._feedRequestIsLooping = false;
        }
        if(toggle == true){
            this._feedRequestIsLooping = true;
            this._feedRequestLoop = StartCoroutine(GetFeedData(onStatus));
        }
    }

    IEnumerator GetFeedData(System.Action<HttpUtils.ConnectionStatus> onStatus){
        while(this._feedRequestIsLooping){
            yield return HttpUtils.GetRequest(this._feedURL, 
            (e) => { 
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.ERROR;
                status.message = e.message;
                onStatus.Invoke(status);
            }, 
            (s) => {
                PulsoidFeedResult result = JsonUtility.FromJson<PulsoidFeedResult>(s);
                this._feedHeartrate = result.bpm;
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
                onStatus.Invoke(status);
            },
            null);
            float t = 0;
            while(t < this._feedRefreshInterval){
                t+= Time.deltaTime;
                yield return null;
            } 
        }
        HttpUtils.ConnectionStatus exitStatus = new HttpUtils.ConnectionStatus();
        exitStatus.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
        onStatus.Invoke(exitStatus);
    }

    [System.Serializable]
    public class PulsoidFeedResult{
        public int bpm;
        public string measured_at;
    }

    #endregion

}
