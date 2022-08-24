using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class APIMessage {
    public string apiVersion = "1.0";
    public string messageType = "APIMessage";
    public long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

public class ErrorMessage : APIMessage {
    public Data data = new Data();
    public ErrorMessage(StatusCode status, string message){
        this.messageType = "ErrorResponse";
        this.data = new Data();
        this.data.errorCode = status;
        this.data.message = message;
        Debug.LogError("API ERROR: " + message);
    }

    [System.Serializable]
    public class Data{
        public StatusCode errorCode;
        public string message;
    }

    [System.Serializable]
    public enum StatusCode : int {
        OK = 200,
        BAD_REQUEST = 400,
        FORBIDDEN = 403,
        SERVER_ERROR = 500,
    }
}

#region Data Models

public class DataMessage : APIMessage {

    public Data data = new Data();
    public DataMessage(int heartrate){
        this.messageType = "DataResponse";
        this.data = new Data();
        this.data.heartrate = heartrate;
    }

    [System.Serializable]
    public class Data{
        public int heartrate;
        public VTSParamaters parameters = new VTSParamaters();
        public List<Tint> tints = new List<Tint>();
    }

    [System.Serializable]
    public class VTSParamaters{
        public float vts_heartrate_linear;
        public float vts_heartrate_pulse;
        public float vts_heartrate_breath;
        public float vts_heartrate_bpm;
        public float vts_heartrate_bpm_ones;
        public float vts_heartrate_bpm_tens;
        public float vts_heartrate_bpm_hundreds;
        public float vts_heartrate_repeat_1;
        public float vts_heartrate_repeat_5;
        public float vts_heartrate_repeat_10;
        public float vts_heartrate_repeat_20;
        public float vts_heartrate_repeat_30;
        public float vts_heartrate_repeat_60;
        public float vts_heartrate_repeat_120;
        public float vts_heartrate_repeat_breath;
    }

    [System.Serializable]
    public class Tint{
        public Tint(Color32 maximumColor, Color32 currentColor, string[] matchers){
            this.maximumColor = maximumColor;
            this.currentColor = currentColor;
            this.matchers = matchers;
        }
        public Color32 maximumColor;
        public Color32 currentColor; 
        public string[] matchers;
    }
}

#endregion

#region Event Models

public abstract class EventMessage : APIMessage { }

public class ExpressionEventMessage : EventMessage {

    public Data data = new Data();
    public ExpressionEventMessage(int threshold, int heartrate, string expression, ExpressionModule.TriggerBehavior behavior, bool activated){
        this.messageType = "ExpressionEventResponse";
        this.data = new Data();
        this.data.threshold = threshold;
        this.data.heartrate = heartrate;
        this.data.expression = expression;
        this.data.behavior = MapBehavior(behavior);
        this.data.activated = activated;
    }
    [System.Serializable]
    public class Data {
        public int threshold;
        public int heartrate;
        public string expression;
        public ExpressionTriggerBehavior behavior;
        public bool activated;
    }

    [System.Serializable]
    public enum ExpressionTriggerBehavior : int {   
        UNKNOWN = -1,     
        ACTIVATE_ABOVE_DEACTIVATE_BELOW = 0,
        DEACTIVATE_ABOVE_ACTIVATE_BELOW = 1,
        ACTIVATE_ABOVE = 2,
        DEACTIVATE_ABOVE = 3,
        ACTIVATE_BELOW = 4,
        DEACTIVATE_BELOW = 5,
    }

    private ExpressionTriggerBehavior MapBehavior(ExpressionModule.TriggerBehavior behavior){
        switch(behavior){
            case ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE:
                return ExpressionTriggerBehavior.ACTIVATE_ABOVE;
            case ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW:
                return ExpressionTriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW;
            case ExpressionModule.TriggerBehavior.ACTIVATE_BELOW:
                return ExpressionTriggerBehavior.ACTIVATE_BELOW;
            case ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE:
                return ExpressionTriggerBehavior.DEACTIVATE_ABOVE;
            case ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW:
                return ExpressionTriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW;
            case ExpressionModule.TriggerBehavior.DEACTIVATE_BELOW:
                return ExpressionTriggerBehavior.DEACTIVATE_BELOW;
        }
        return ExpressionTriggerBehavior.UNKNOWN;
    }
}

public class HotkeyEventMessage : EventMessage {
    public Data data = new Data();
    public HotkeyEventMessage(int threshold, int heartrate, string hotkey, HotkeyModule.TriggerBehavior behavior){
        this.messageType = "HotkeyEventResponse";
        this.data = new Data();
        this.data.threshold = threshold;
        this.data.heartrate = heartrate;
        this.data.hotkey = hotkey;
        this.data.behavior = MapBehavior(behavior);
    }

    [System.Serializable]
    public class Data {
        public int threshold;
        public int heartrate;
        public string hotkey;
        public HotkeyTriggerBehavior behavior;
    }

    [System.Serializable]
    public enum HotkeyTriggerBehavior : int {   
        UNKNOWN = -1,     
        ACTIVATE_ABOVE_ACTIVATE_BELOW = 0,
        ACTIVATE_ABOVE = 1,
        ACTIVATE_BELOW = 2,
    }

    private HotkeyTriggerBehavior MapBehavior(HotkeyModule.TriggerBehavior behavior){
        switch(behavior){
            case HotkeyModule.TriggerBehavior.ACTIVATE_ABOVE:
                return HotkeyTriggerBehavior.ACTIVATE_ABOVE;
            case HotkeyModule.TriggerBehavior.ACTIVATE_BELOW:
                return HotkeyTriggerBehavior.ACTIVATE_BELOW;
            case HotkeyModule.TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW:
                return HotkeyTriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW;
        }
        return HotkeyTriggerBehavior.UNKNOWN;
    }
}

#endregion

#region Input Models

public class InputMessage : APIMessage {
    public Data data = new Data();
    public InputMessage(){
        this.messageType = "InputResponse";
        this.data = new Data();
    }

    [System.Serializable]
    public class Data{
        public int heartrate;
    }
}

public class AuthenticationMessage : APIMessage {
    public Data data = new Data();
    public AuthenticationMessage(string pluginName, string pluginAuthor, string pluginAbout, string token, bool authenticated){
        this.messageType = "AuthenticationResponse";
        this.data = new Data();
        this.data.pluginName = pluginName;
        this.data.pluginAuthor = pluginAuthor;
        this.data.pluginAbout = pluginAbout;
        this.data.token = token;
        this.data.authenticated = authenticated;
    }

    [System.Serializable]
    public class Data{
        public string pluginName;
        public string pluginAuthor;
        public string pluginAbout;
        public string token;
        public bool authenticated = false;
    }
}

#endregion