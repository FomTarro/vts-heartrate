using UnityEngine;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;


public abstract class WebServerRule : MonoBehaviour
{
#if UNITY_STANDALONE || UNITY_EDITOR

    public bool BlockOnMatch
    {
        get { return blockOnMatch; }
    }

    protected void Start()
    {

        foreach(string pattern in urlRegexList)
        {
            //Debug.Log(pattern);
            regexList.Add(new Regex(pattern.ToLower()));
        }
    }

    public IEnumerator ProcessRequest(HttpListenerContext context, System.Action<bool> isMatch)
    {
        string url = context.Request.RawUrl;
        //Debug.Log(url);

        bool match = false;
        foreach(Regex regex in regexList)
        {
            if (regex.Match(url.ToLower()).Success)
            {
                match = true;
                break;
            }
        }

        if (!match)
        {
            isMatch(false);
            yield break;
        }

        IEnumerator e = OnRequest(context);
        do
        {
            yield return null;
        }
        while (e.MoveNext());

        isMatch(true);
    }

    protected abstract IEnumerator OnRequest(HttpListenerContext context);
#endif

    [SerializeField]
    [Tooltip("Allows you to specify a number of regex expressions to perform on the url. If any create a match on a request then this rule will run for the request.")]
    protected string[] urlRegexList = new string[]{"^*"};

    [SerializeField]
    [Tooltip("If true then if this rule executes it will stop execution to further rules.")]
    private bool blockOnMatch = false;

    private List<Regex> regexList = new List<Regex>();

    /// <summary>
    /// The approximate number of bytes able to be written in 1MS during the last HTML request
    /// </summary>
    protected static double bytesPerMillisecond = 1;
}
