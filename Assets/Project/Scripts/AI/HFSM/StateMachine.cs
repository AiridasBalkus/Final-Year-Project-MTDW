public class StateMachine
{
    public State Current { get; private set; }

    public string Path
    {
        get
        {
            if (Current == null) return "";
            string sub = Current.Sub?.Path;
            return string.IsNullOrEmpty(sub) ? Current.Name : Current.Name + "/" + sub;
        }
    }

    public void Change(State next)
    {
        ExitCurrent();
        Current = next;
        next.Enter();
    }

    public void Tick()
    {
        State ticking = Current;
        if (ticking == null) return;

        // parent first, so it can leave before its children act
        ticking.Tick();
        if (Current == ticking) ticking.Sub?.Tick();
    }

    // exits the whole active branch, deepest state first
    public void ExitCurrent()
    {
        if (Current == null) return;

        State old = Current;
        Current = null;
        old.Sub?.ExitCurrent();
        old.Exit();
    }
}
