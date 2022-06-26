
[System.Serializable]
public class APIMessage {
    public string apiVersion = "1.0";
    public string messageType = "APIMessage";
    public long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

#region Data Models

public class DataMessage : APIMessage {

    public Data data = new Data();
    public DataMessage(){
        this.messageType = "DataResponse";
        this.data = new Data();
    }

    [System.Serializable]
    public class Data{
        public float heartrate;
        public float vts_heartrate_bpm;
        public float vts_heartrate_pulse;
        public float vts_heartrate_breath;
        public float vts_heartrate_linear;
    }
}

#endregion

#region Event Models

public abstract class EventMessage : APIMessage { }

public class ExpressionEventMessage : EventMessage {

    public Data data = new Data();
    public ExpressionEventMessage(int threshold, string expression, ExpressionModule.TriggerBehavior behavior, bool activated){
        this.messageType = "ExpressionEventResponse";
        this.data = new Data();
        this.data.threshold = threshold;
        this.data.expression = expression;
        this.data.behavior = MapBehavior(behavior);
        this.data.activated = activated;
    }
    [System.Serializable]
    public class Data {
        public int threshold;
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
    public HotkeyEventMessage(int threshold, string hotkey, HotkeyModule.TriggerBehavior behavior){
        this.messageType = "HotkeyEventResponse";
        this.data = new Data();
        this.data.threshold = threshold;
        this.data.hotkey = hotkey;
        this.data.behavior = MapBehavior(behavior);
    }

    [System.Serializable]
    public class Data {
        public int threshold;
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
    public AuthenticationMessage(){
        this.messageType = "AuthenticationResponse";
        this.data = new Data();
    }

    [System.Serializable]
    public class Data{
        public string pluginName;
        public string pluginAuthor;
        public string token;
        public bool authenticated = false;
    }
}

public class ErrorMessage : APIMessage {
    public Data data = new Data();
    public ErrorMessage(){
        this.messageType = "ErrorResponse";
        this.data = new Data();
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

#endregion