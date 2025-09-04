using System;
using UnityEngine;

class Agent : MonoBehaviour
{
    //Creeper
    public enum States
    {
        Patrol,
        Chase,
        Explode
    }

    public enum Flags
    {
        OnTargetReach,
        OnTargetNear,
        OnTargetLost
    }

    public FSM<States, Flags> fsm;

    public Transform target;
    public float speed;
    public float explodeDistance;
    public float lostDistance;

    public Transform wayPoint1;
    public Transform wayPoint2;
    public float chaseDistance;

    public void Start()
    {
        fsm = new FSM<States, Flags>(States.Patrol);

        fsm.AddState<PatrolState>(States.Patrol, 
            onTickParameters: () => new object[] { wayPoint1, wayPoint2, transform, target, speed, chaseDistance, Time.deltaTime}
            );

        fsm.AddState<ChaseState>(States.Chase,
            onTickParameters: () => new object[] { transform, target, speed, explodeDistance, lostDistance, Time.deltaTime }
            );

        fsm.AddState<ExplodeState>(States.Explode);

        fsm.SetTransition(States.Patrol, Flags.OnTargetNear, States.Chase, () => { Debug.Log("Te vi"); });
        fsm.SetTransition(States.Chase, Flags.OnTargetReach, States.Explode);
        fsm.SetTransition(States.Chase, Flags.OnTargetLost, States.Patrol);
    }

    private void Update()
    {
        fsm.Tick();
    }
}
