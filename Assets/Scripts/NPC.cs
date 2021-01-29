using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour
{
    State StateWaiting, StateRoped, StateOnPlane, StateDying;
    Animator anim;
    Transform attached;
    [System.Serializable] public class UpdateTransform : UnityEvent<Transform> { }
    public UpdateTransform updateAttach = new UpdateTransform();

    public void OnHook()
    {
        TransitionState(StateRoped);
    }
    public void OnTakeDamage()
    {
        TransitionState(StateDying);
    }
    public void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        
    }



    #region StateMachine
    protected delegate void StateEvent();
    protected delegate void StateUpdate(float dt);
    
    protected class State
    {
        public State(string name) { this.name = name; }
        public string name;
        public float time;
        public bool isFirstUpdate;
        public StateEvent OnEnter, OnExit, LateUpdate;
        public StateUpdate Update;
    }
    private State _currentState;
    protected State currentState => _currentState;
    void TransitionState(State newState, bool triggerEventsIfEnterSame = true)
    {
        if (newState == _currentState && !triggerEventsIfEnterSame) return;
        _currentState?.OnExit?.Invoke();
        newState.time = 0; newState.isFirstUpdate = true;
        newState.OnEnter?.Invoke();
        _currentState = newState;
#if UNITY_EDITOR
        debug_Statename = _currentState.name;
#endif
    }
    void UpdateState(float dt)
    {
        _currentState.time += dt; _currentState.isFirstUpdate = false;
        _currentState.Update?.Invoke(dt);
    }
    void LateUpdateState()
    {
        _currentState.LateUpdate?.Invoke();
    }
#if UNITY_EDITOR
    [SerializeField] private string debug_Statename;
#endif
    #endregion    
}
