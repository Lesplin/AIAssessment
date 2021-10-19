using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachine : MonoBehaviour
{
    // enum describing AI states.
    protected enum State { WatchTV = 0, Eat = 1, Toilet = 2, Bathe = 3, Sleep = 4};

    protected State AIState = State.WatchTV;

    protected void ChangeState(State newState)
    {
        // change the state
        AIState = newState;
    }

    protected bool isState(State checkState)
    {
        return (AIState == checkState);
    }
}
