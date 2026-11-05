// Leaf states leave Sub null. A composite state creates Sub and calls Sub.Change(firstChild) in Enter.
public abstract class State
{
    public string Name { get; }
    public StateMachine Sub { get; protected set; }

    protected State(string name)
    {
        Name = name;
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
}
