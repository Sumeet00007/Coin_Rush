
using UnityEngine;

public static class ScreenOrientationManager
{
    /// <summary>
    /// Force the application into Portrait mode.
    /// Landscape orientations are completely disabled.
    /// </summary>
    public static void SetPortrait()
    {
        // Disable autorotation completely.
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;

        // Immediately force Portrait.
        Screen.orientation = ScreenOrientation.Portrait;
    }

 
    public static void SetLandscape()
    {
        // Disable portrait autorotation.
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // Allow landscape orientations.
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Force landscape immediately.
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}

