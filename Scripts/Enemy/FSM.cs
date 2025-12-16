using System;
using System.Collections.Generic;

public interface IState<EState, T> where EState : Enum
{
    public EState State { get; }
    public void Enter(T controller);
    public void Update(T controller);
    public void Exit(T controller);
}

public class FSM<EState, T> where EState : Enum
{
    private T controller;
    public T Controller => controller;
    private EState current;
    public EState Current => current;
    private IState<EState, T>[] states;

    public FSM(T controller, IEnumerable<IState<EState, T>> stateObjects)
    {
        this.controller = controller;

        states = new IState<EState, T>[Enum.GetValues(typeof(EState)).Length];
        foreach (IState<EState, T> state in stateObjects)
        {
            states[Convert.ToInt32(state.State)] = state;
        }
    }

    public void Update()
    {
        states[Convert.ToInt32(current)].Update(controller);
    }

    public void Transit(EState next)
    {
        states[Convert.ToInt32(current)].Exit(controller);
        states[Convert.ToInt32(next)].Enter(controller);
        current = next;
    }
}