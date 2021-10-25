using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartState : BaseState
{
    //public IdleState(TurnBasedFsm fsm, IGameData gameData, GameParameters gameParameters,
    //    Observer gameEvents) :
    //    base(fsm, gameData, gameParameters, gameEvents)

    public StartState(TurnBasedFsm fsm) : base(fsm)
    {
        //
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        Stuff();
    }

    public void Stuff()
    {
        // when done

    
    }

    public override void OnExitState()
    {
        base.OnExitState();


        Fsm.AdvanceState<IdleState>();
    }
    
}
