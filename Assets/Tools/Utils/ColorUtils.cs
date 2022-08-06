using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorUtils
{
    public static readonly Color WHITE = new Color32(225, 225, 225, 255);
    public static readonly Color GREEN = new Color32(202, 253, 132, 255);
    public static readonly Color RED = new Color32(253, 132, 144, 255);
    public static readonly Color BLUE = new Color32(132, 230, 253, 255);

    public enum ColorPreset {
        WHITE = 0,
        GREEN = 1,
        RED = 2,
        BLUE = 3,
    }
    public static Color ColorPresetToUnityColor(ColorPreset color){
        if(color == ColorPreset.WHITE){
            return ColorUtils.WHITE;
        }else if(color == ColorPreset.GREEN){
            return ColorUtils.GREEN;
        }else if(color == ColorPreset.RED){
            return ColorUtils.RED;
        }else if(color == ColorPreset.BLUE){
            return ColorUtils.BLUE;
        }
        return ColorUtils.WHITE;
    }
}
