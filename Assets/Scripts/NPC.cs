using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using PhysicsEX = UnityEngine.Physics;

[RequireComponent(typeof(CapsuleController))]
public class NPC : MonoBehaviourPun
{
    Animator anim;
    public SphereCollider hookPoint;
    public Transform followPlayer;
    public Transform followPlane;
    public Rigidbody followRigidbody;
    public Vector3 onPlanePositionLS;
    public Quaternion onPlaneRotationLS;

    public Vector3 followBias;
    public float teleportDist=2f;
    CapsuleController controller;

    public void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CapsuleController>();
        //SetupStates();
    }
    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (followPlane)
            {
                controller.position = followPlane.TransformPoint(onPlanePositionLS);
                controller.rotation = followPlane.rotation * onPlaneRotationLS;
                controller.SetAttach(followPlane, Vector3.zero);
                anim.SetInteger("State", 2);
            }
            else if (followPlayer)
            {

                Vector3 newPos = followPlayer.position + followPlayer.rotation * followBias;
                Vector3 delta = controller.SweepCollider(newPos - transform.position, slide: true, out bool isHit, out RaycastHit hit);
                Vector3 newForward = Vector3.ProjectOnPlane(delta, Vector3.up);
                if (newForward.magnitude > 0)
                    transform.rotation = Quaternion.LookRotation(newForward.normalized, Vector3.up);

                transform.position += delta;
                controller.ResolveCollision(false);

                controller.SetAttach(null, Vector3.zero);
                anim.SetInteger("State", 1);
            }
            else
            {
                controller.SetAttach(null, Vector3.zero);
                anim.SetInteger("State", 0);
            }
        }
    }
    public void Hook(Transform player)
    {
        var pv = player.GetComponent<PhotonView>();
        if(pv)
            photonView.RPC("SetFollowPlayer", RpcTarget.All, pv.ViewID);
    }
    [PunRPC]
    void SetFollowPlayer(int viewID)
    {
        var pv = PhotonNetwork.GetPhotonView(viewID);
        if (pv)
            followPlayer = pv.transform;
    }
    public void BoardPlane(Transform plane, Vector3 onPlanePositionLS, Quaternion onPlaneRotationLS)
    {
        var pv = plane.GetComponent<PhotonView>();
        if (pv)
            photonView.RPC("SetFollowPlane", RpcTarget.All, pv.ViewID, onPlanePositionLS, onPlaneRotationLS);
    }
    [PunRPC]
    void SetFollowPlane(int viewID, Vector3 onPlanePositionLS, Quaternion onPlaneRotationLS)
    {
        var pv = PhotonNetwork.GetPhotonView(viewID);
        if (pv)
        {
            followPlane = pv.transform;
            followRigidbody = followPlane.GetComponent<Rigidbody>();
        }
        this.onPlanePositionLS = onPlanePositionLS;
        this.onPlaneRotationLS = onPlaneRotationLS;
    }
    /*
    State StateWaiting, StateRoped, StateOnPlane, StateDying;
    void SetupStates()
    {
        StateWaiting = new State("Waiting");
        StateWaiting.Update = UpdateWaiting;
        StateRoped = new State("Roped");
        StateRoped.Update = UpdateRoped;
        StateOnPlane = new State("OnPlane");
        StateOnPlane.Update = UpdateOnPlane;
        StateDying = new State("Dying");
        StateDying.Update = UpdateDying;
    }
    void UpdateWaiting(float dt)
    {
        anim.SetInteger("State", 0);
    }
    void UpdateRoped(float dt)
    {
        anim.SetInteger("State", 1);
        Vector3 pos = followPlayer.transform.position + followPlayer.transform.rotation * followBias;

    }
    void UpdateOnPlane(float dt)
    {
        anim.SetInteger("State", 2);
        transform.position = attached.TransformPoint(attachedPositionLS);
        updateAttach.Invoke(attached);
    }
    void UpdateDying(float dt)
    {
        anim.SetInteger("State", 3);
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
    */
}
