using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

public class GetDataRule : WebServerRule
{
    const string ROW_FORMAT = "\"{0}\": {1},";
    protected override IEnumerator OnRequest(HttpListenerContext context)
    {
        string data = string.Format(ROW_FORMAT, "timestamp", System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        Dictionary<string, float> paramMap = HeartrateManager.Instance.Plugin.ParameterMap;
        foreach(string id in paramMap.Keys){
            data += string.Format(ROW_FORMAT, id, paramMap[id]);
        }
        data = "{"+data.ToLower()+"}";
        data = data.Replace(",}", "}");
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);

        context.Response.OutputStream.Write(buffer,0,buffer.Length);
        context.Response.OutputStream.Close();
        context.Response.Close();
        yield return null;
    }

}
