using UnityEngine;

public static class VersionUtils
{
    /// <summary>
    /// Compares the current version to the remote version. Returns true if the remote is newer.
    /// </summary>
    /// <param name="remoteVersion"></param>
    /// <returns></returns>
    public static bool CompareVersion(VersionInfo remoteVersion){
        string currentVersion = Application.version;
        return IsOlderThan(currentVersion, remoteVersion.version);
    }

    /// <summary>
    /// Compares the Version A to the Version B. Returns true if the VersionB is newer.
    /// </summary>
    /// <param name="versionA"></param>
    /// <param name="versionB"></param>
    /// <returns></returns>
    public static bool IsOlderThan(string versionA, string versionB){
        return versionA.CompareTo(versionB) < 0;
    }

    [System.Serializable]
    public class VersionInfo{
        public string version;
        public string date;
        public string url;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
