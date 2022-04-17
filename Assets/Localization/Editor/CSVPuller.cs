using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
/// <summary>
/// Editor tool used for pulling down changes from our Google-Drive-hosted Localization CSV spreadsheet
/// </summary>
public class CSVPuller : EditorWindow {

    [SerializeField]
    private static UnityEngine.Object _csvAsset;
    private static string _shareLink = string.Empty;
    private static readonly Regex URL_REGEX = new Regex(@"/d/(.*)/edit");

    private const string PREF_KEY = "LOCALIZATION_CSV_LOCAL_PATH";
    private const string PREF_SOURCE_KEY = "LOCALIZATION_CSV_SOURCE_PATH";
    private const string TAB_NAME = "CSV Tools";
    private const string MENU_CONFIG = "Configure Settings";
    private const string MENU_PULL = "Pull Latest CSV";

    #region Init

    static CSVPuller()
    {
        EditorApplication.delayCall += LoadPrefs;
    }

    private static void LoadPrefs()
    {
        if (EditorPrefs.HasKey(PREF_KEY))
        {
            _csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(EditorPrefs.GetString(PREF_KEY));
        }
        if (EditorPrefs.HasKey(PREF_SOURCE_KEY))
        {
            _shareLink = EditorPrefs.GetString(PREF_SOURCE_KEY);
        }
    }

    #endregion

    #region GUI

    [MenuItem("Tools/" + TAB_NAME + "/" + MENU_CONFIG, false, 1)]
    private static void Init()
    {
        LoadPrefs();
        CSVPuller window = (CSVPuller)GetWindowWithRect(typeof(CSVPuller), new Rect(0, 0, 320, 192));
        window.titleContent = new GUIContent(TAB_NAME);
        window.Show();
    }

    [MenuItem("Tools/" + TAB_NAME + "/" + MENU_PULL, false, 100)]
    private static void UpdateMenuItem()
    {
        UpdateCSV(_csvAsset as TextAsset, GetKeyFromURL(_shareLink));
    }

    [MenuItem("Tools/" + TAB_NAME + "/" + MENU_PULL, true)]
    private static bool EnableUpdate()
    {
        return IsValidConfig(_csvAsset, _shareLink);
    }


    private void OnGUI()
    {
        SerializedObject serializedObject = new SerializedObject(this);

        GUILayout.Label("Source File Settings", EditorStyles.boldLabel);
        _shareLink = EditorGUILayout.TextField("Drive Share Link: ", _shareLink);
        EditorPrefs.SetString(PREF_SOURCE_KEY, _shareLink);

        EditorGUI.BeginDisabledGroup(true);
        string key = GetKeyFromURL(_shareLink);
        EditorGUILayout.TextField("File Key: ", key);
        EditorGUI.EndDisabledGroup();

        GUILayout.Label("", EditorStyles.boldLabel);
        GUILayout.Label("Destination File Settings", EditorStyles.boldLabel);     
        if (GUILayout.Button("Select CSV file to write"))
        {
            string filePath = EditorUtility.OpenFilePanel("Destination CSV Location", "Assets", "csv");
            if (filePath != string.Empty)
            {
                filePath = filePath.Substring(filePath.IndexOf("Assets"));
                EditorPrefs.SetString(PREF_KEY, filePath);
                _csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        GUILayout.Label("Target Asset: ", EditorStyles.label);
        _csvAsset = EditorGUILayout.ObjectField(_csvAsset, typeof(TextAsset), false);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();

        GUILayout.Label("", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(!IsValidConfig(_csvAsset, _shareLink));
        if (GUILayout.Button("Pull down latest CSV strings"))
        {
            UpdateMenuItem();
        }
        EditorGUI.EndDisabledGroup();
    }

    #endregion

    #region Helper Methods

    private static string GetKeyFromURL(string url)
    {
        string key = string.Empty;
        Match match = URL_REGEX.Match(url);
        if (match.Success && match.Groups.Count > 1)
        {
            key = match.Groups[1].Value;
        }
        return key;
    }

    private static bool IsValidConfig(UnityEngine.Object asset, string url)
    {
        return (asset != null && asset.GetType() == typeof(TextAsset)) && (GetKeyFromURL(url) != string.Empty);
    }

    private static void UpdateCSV(TextAsset csv, string sourceKey) {
        if (IsValidConfig(csv, _shareLink))
        {
            string url = @"https://docs.google.com/spreadsheet/ccc?key=" + sourceKey + "&usp=sharing&output=csv";

            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
            DriveClient wc = new DriveClient(new CookieContainer());
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:22.0) Gecko/20100101 Firefox/22.0");
            wc.Headers.Add("DNT", "1");
            wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            wc.Headers.Add("Accept-Encoding", "deflate");
            wc.Headers.Add("Accept-Language", "en-US,en;q=0.5");

            byte[] data = wc.DownloadData(url);
            string outputCSVdata = System.Text.Encoding.UTF8.GetString(data ?? new byte[] { });
            File.WriteAllText(AssetDatabase.GetAssetPath(csv), outputCSVdata);
            Debug.Log("CSV Updated successfully!");
            EditorUtility.SetDirty(csv);
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Unable to update CSV!");
        }
    }

    #endregion

    #region Hideous Internal Workings

    private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }

    private class DriveClient : WebClient
    {
        public DriveClient(CookieContainer container)
        {
            this.container = container;
        }

        private readonly CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            HttpWebRequest request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            HttpWebResponse response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }

    #endregion
}