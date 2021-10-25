using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState // , IListener, IRestartGame (?)
{
    //------------------------------------------------------------------

    #region Constructor

    public BaseState(TurnBasedFsm fsm) //, IGameData gameData, GameParameters gameParameters, Observer gameEvents)
    {
        Fsm = fsm;
        //GameData = gameData;
        //GameParameters = gameParameters;
        //GameEvents = gameEvents;

        // Subscribe game events.
        //GameEvents.AddListener(this);
        IsInitialized = true;
    }

    #endregion

    //------------------------------------------------------------------

    #region Properties

    public TurnBasedFsm Fsm { get; set; }
    //protected IGameData GameData { get; }
    //protected GameParameters GameParameters { get; }
    //protected Observer GameEvents { get; }
    public bool IsInitialized { get; }

    #endregion

    //------------------------------------------------------------------

    #region Operations

    //public virtual void OnClear() => GameEvents.RemoveListener(this);
    public virtual void OnClear()
    {
    }

    public virtual void OnInitialize()
    {
    }

    public virtual void OnEnterState()
    {
        Debug.Log($"Entering {this.GetType()}.");
    }

    public virtual void OnExitState()
    {
        Debug.Log($"Exiting {this.GetType()}.");
    }

    public virtual void OnUpdate()
    {

    }

    

    //void IRestartGame.OnRestart() => OnClear();

    #endregion

    //------------------------------------------------------------------
}