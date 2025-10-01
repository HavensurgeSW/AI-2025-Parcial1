using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class FSM<StateType, FlagType>
    where StateType : Enum
    where FlagType : Enum
{
    private const int UNNASSIGNED_TRANSITION = -1;
    private int currentState;
    private Dictionary<int, State> states;
    private Dictionary<int, Func<object[]>> behaviourOnTickParameters;
    private Dictionary<int, Func<object[]>> behaviourOnEnterParameters;
    private Dictionary<int, Func<object[]>> behaviourOnExitParameters;

    private (int destinationState, Action onTrnasition)[,] transitions;

    ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

    private BehaviourActions GetCurrentTickBehaviour => states[currentState].GetOnTickBehaviours
        (behaviourOnTickParameters[currentState]?.Invoke());
    private BehaviourActions GetCurrentOnEnterBehaviour => states[currentState].GetOnEnterBehaviours
        (behaviourOnEnterParameters[currentState]?.Invoke());
    private BehaviourActions GetCurrentOnExitBehaviour => states[currentState].GetOnExitBehaviours
        (behaviourOnExitParameters[currentState]?.Invoke());

    public FSM(StateType defaultState)
    {
        states = new Dictionary<int, State>();
        transitions = new (int, Action)[Enum.GetValues(typeof(StateType)).Length, Enum.GetValues(typeof(FlagType)).Length];
        for (int i = 0; i < transitions.GetLength(0); i++)
        {
            for (int j = 0; j < transitions.GetLength(1); j++)
            {
                transitions[i, j] = (UNNASSIGNED_TRANSITION, null);
            }
        }

        behaviourOnTickParameters = new Dictionary<int, Func<object[]>>();
        behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
        behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
        ForceSetState(defaultState);
    }

    public void AddState<TState>(StateType stateIndex, 
        Func<object[]> onTickParameters = null,
        Func<object[]> onEnterParameters = null,
        Func<object[]> onExitParameters = null)
        where TState : State, new()
    {
        if (!states.ContainsKey(Convert.ToInt32(stateIndex)))
        {
            TState state = new TState();
            state.OnFlag += Transition;
            states.Add(Convert.ToInt32(stateIndex), state);
            behaviourOnTickParameters.Add(Convert.ToInt32(stateIndex), onTickParameters);
            behaviourOnEnterParameters.Add(Convert.ToInt32(stateIndex), onEnterParameters);
            behaviourOnExitParameters.Add(Convert.ToInt32(stateIndex), onExitParameters);
        }
    }

    public void ForceState(StateType state)
    {
        currentState = Convert.ToInt32(state);
        //ExecuteBehaviour(GetCurrentOnEnterBehaviour);
    }

    
    public void ForceSetState(StateType state)
    {
        if (states.ContainsKey(currentState))
        {
            ExecuteBehaviour(GetCurrentOnExitBehaviour);
        }
        currentState = Convert.ToInt32(state);

        if (states.ContainsKey(currentState))
        {
            ExecuteBehaviour(GetCurrentOnEnterBehaviour);
        }
    }

    public void SetTransition(StateType originalState, FlagType flag, StateType destinationState, Action onTransition = null)
    {
        transitions[Convert.ToInt32(originalState), Convert.ToInt32(flag)] = (Convert.ToInt32(destinationState), onTransition);
    }

    public void Transition(Enum flag)
    {
        if (states.ContainsKey(currentState))
        {
            ExecuteBehaviour(GetCurrentOnExitBehaviour);
        }
        if (transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState != UNNASSIGNED_TRANSITION)
        {
            transitions[currentState, Convert.ToInt32(flag)].onTrnasition?.Invoke();
            currentState = transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState;
            ExecuteBehaviour(GetCurrentOnEnterBehaviour);
        }
    }

    public void Tick()
    {
        if (states.ContainsKey(currentState))
        {
            ExecuteBehaviour(GetCurrentTickBehaviour);
        }
    }

    private void ExecuteBehaviour(BehaviourActions behaviourActions)
    {
        if (behaviourActions.Equals(default))
            return;

        int executionOrder = 0;

        while ((behaviourActions.MainThreadBehaviours != null && behaviourActions.MainThreadBehaviours.Count > 0) ||
                behaviourActions.MultiThreadableBehaviours != null && behaviourActions.MultiThreadableBehaviours.Count > 0)
        {
            Task multithradableBehabiour = new Task(() =>
            {
                if (behaviourActions.MultiThreadableBehaviours != null)
                {
                    if (behaviourActions.MultiThreadableBehaviours.ContainsKey(executionOrder))
                    {
                        Parallel.ForEach(behaviourActions.MultiThreadableBehaviours[executionOrder], ParallelOptions, (behaviour) =>
                        {
                            behaviour?.Invoke();
                        });
                        behaviourActions.MultiThreadableBehaviours.TryRemove(executionOrder, out _);
                    }
                }
            });

            multithradableBehabiour.Start();

            if (behaviourActions.MainThreadBehaviours != null)
            {
                if (behaviourActions.MainThreadBehaviours.ContainsKey(executionOrder))
                {
                    foreach (Action behaviour in behaviourActions.MainThreadBehaviours[executionOrder])
                    {
                        behaviour?.Invoke();
                    }
                    behaviourActions.MainThreadBehaviours.Remove(executionOrder);
                }
            }

            multithradableBehabiour?.Wait();
            executionOrder++;
        }

        behaviourActions.TransitionBehaviour?.Invoke();
    }

    public void ValidateParameters(Type[] expectedParameters, Func<object[]> recievedParameters)
    {
        if (expectedParameters.Length == 0 && recievedParameters == null)
            return;

        List<Type> recievedParametersTypes = new List<Type>();
        foreach (object parameter in recievedParameters.Invoke())
        {
            recievedParametersTypes.Add(parameter.GetType());
        }

        if (expectedParameters.Length != recievedParametersTypes.Count)
        {
            throw new ArgumentException("Numer of parameters different from expected");
        }

        for (int i = 0; i < expectedParameters.Length; i++)
        {
            if (!expectedParameters[i].IsAssignableFrom(recievedParametersTypes[i]))
            {
                throw new InvalidCastException("Type " + recievedParametersTypes[i].Name
                    + " cannot be assigned to " + expectedParameters[i].Name);
            }
        }
    }
}