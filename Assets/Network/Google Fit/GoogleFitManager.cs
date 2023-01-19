using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleFitManager : Singleton<GoogleFitManager> {

    private const string AUTH_URL = @"https://accounts.google.com/o/oauth2/v2/auth?redirect_uri={0}&prompt=consent&response_type=code&client_id={1}&scope=https://www.googleapis.com/auth/fitness.heart_rate.read&access_type=offline";
    private const string REDIRECT_URL = @"https://www.skeletom.net/vts-heartrate/oauth2/google";
    
	public override void Initialize() {
		string url = string.Format(
            AUTH_URL,
            REDIRECT_URL,
            GoogleCredentials.CLIENT_ID
        );
        Application.OpenURL(url);
	}
}
