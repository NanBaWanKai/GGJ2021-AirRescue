using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IFancing
{
    public abstract class PunMonoBehaviour : MonoBehaviourPun,IPunObservable
    {
        protected virtual void Awake()
        {
            if (photonView)
            {
                photonView.OwnershipTransfer = OwnershipOption.Takeover;
            }
        }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() { }
        protected void ChangeFloatGlobal(ref float value, float target, Action<float> callback)
        {
            if (value != target)
            {
                photonView?./*TransferOwnership(PhotonNetwork.LocalPlayer);//*/ RequestOwnership();
                value = target;
                callback?.Invoke(value);
            }
        }
        protected void ChangeFloatLocal(ref float value, float target, Action<float> callback)
        {
            if (value != target)
            {
                value = target;
                callback?.Invoke(value);
            }
        }
        protected void ChangeIntGlobal(ref int value, int target, Action<int> callback)
        {
            if (value != target)
            {
                photonView?.RequestOwnership();
                value = target;
                callback?.Invoke(value);
            }
        }
        protected void ChangeIntLocal(ref int value, int target, Action<int> callback)
        {
            if (value != target)
            {
                value = target;
                callback?.Invoke(value);
            }
        }
        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }
    }
}
