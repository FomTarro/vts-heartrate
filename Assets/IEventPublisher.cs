public interface IEventPublisher<T> where T : System.Enum {
    EventCallbackRegistration RegisterEventCallback(T eventType, System.Action callback);
    void UnregisterEventCallback(EventCallbackRegistration registration);
} 

public struct EventCallbackRegistration {
    public readonly string uuid;
    public EventCallbackRegistration(string uuid){
        this.uuid = uuid;
    }

    public override bool Equals(object obj){
        return base.Equals(obj);
    }

    public override int GetHashCode(){
        return this.uuid.GetHashCode();
    }

    public override string ToString(){
        return this.uuid;
    }
}
