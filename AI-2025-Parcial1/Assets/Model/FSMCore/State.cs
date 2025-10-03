using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.Graph.Core;


namespace KarplusParcial1.FSM.Core
{
    public class BehaviourActions : IResetable
    {
        private Dictionary<int, List<Action>> mainThreadBehaviours;
        private ConcurrentDictionary<int, ConcurrentBag<Action>> multiThreadableBehaviours;
        private Action transitionBehaviour;

        public Dictionary<int, List<Action>> MainThreadBehaviours => mainThreadBehaviours;
        public ConcurrentDictionary<int, ConcurrentBag<Action>> MultiThreadableBehaviours => multiThreadableBehaviours;
        public Action TransitionBehaviour => transitionBehaviour;

        public void AddMainThreadableBehaviour(int exceutionOrder, Action behaviour)
        {
            if (mainThreadBehaviours == null)
                mainThreadBehaviours = new Dictionary<int, List<Action>>();
            if (!mainThreadBehaviours.ContainsKey(exceutionOrder))
                mainThreadBehaviours.Add(exceutionOrder, new List<Action>());

            mainThreadBehaviours[exceutionOrder].Add(behaviour);
        }

        public void AddMultiThreadableBehaviour(int exceutionOrder, Action behaviour)
        {
            if (multiThreadableBehaviours == null)
                multiThreadableBehaviours = new ConcurrentDictionary<int, ConcurrentBag<Action>>();
            if (!multiThreadableBehaviours.ContainsKey(exceutionOrder))
                multiThreadableBehaviours.TryAdd(exceutionOrder, new ConcurrentBag<Action>());

            multiThreadableBehaviours[exceutionOrder].Add(behaviour);
        }

        public void SetTransitionBehaviour(Action behaviour)
        {
            transitionBehaviour = behaviour;
        }

        public void Reset()
        {
            if (mainThreadBehaviours != null)
            {
                foreach (KeyValuePair<int, List<Action>> behaviour in mainThreadBehaviours)
                {
                    behaviour.Value.Clear();
                }
                mainThreadBehaviours.Clear();
            }
            if (multiThreadableBehaviours != null)
            {
                foreach (KeyValuePair<int, ConcurrentBag<Action>> behaviour in multiThreadableBehaviours)
                {
                    behaviour.Value.Clear();
                }
                multiThreadableBehaviours.Clear();
            }
            transitionBehaviour = null;
        }
    }

    public abstract class State
    {
        public Action<Enum> OnFlag;

        public virtual Type[] OnEnterParameterTypes => Array.Empty<Type>();
        public virtual Type[] OnTickParameterTypes => Array.Empty<Type>();
        public virtual Type[] OnExitParameterTypes => Array.Empty<Type>();



        public virtual BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            return new BehaviourActions();
        }
        public virtual BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            return new BehaviourActions();
        }
        public virtual BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            return new BehaviourActions();
        }

        //public virtual BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        //{
        //    return null;
        //}
        //public virtual BehaviourActions GetOnTickBehaviours(params object[] parameters)
        //{
        //    return null;
        //}
        //public virtual BehaviourActions GetOnExitBehaviour(params object[] parameters)
        //{
        //    return null;
        //}
    }    
}

