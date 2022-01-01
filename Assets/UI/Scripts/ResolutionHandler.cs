using UnityEngine;

public class ResolutionHandler : MonoBehaviour
    {
    private int lastWidth = 0;
    private int lastHeight = 0;

    void Start () {

}

    void LateUpdate ()
    {
        var width = Screen.width;
        var height = Screen.height;

        if(lastWidth != width) // if the user is changing the width
        {
            // update the height
            float heightAccordingToWidth = width / 400f * 600f;
            Screen.SetResolution(width, (int)Mathf.Round(heightAccordingToWidth), FullScreenMode.Windowed, 0);
        }
        else if(lastHeight != height) // if the user is changing the height
        {
            // update the width
            float widthAccordingToHeight = height / 600f * 400f;
            Screen.SetResolution((int)Mathf.Round(widthAccordingToHeight), height, FullScreenMode.Windowed, 0);
        }

        lastWidth = width;
        lastHeight = height;
    }
}
