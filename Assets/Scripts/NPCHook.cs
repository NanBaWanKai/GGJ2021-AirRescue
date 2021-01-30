using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class NPCHook : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            var npc = other.GetComponentInParent<NPC>();
            if (npc && other == npc.hookPoint)
            {
                npc.Hook(XRPlayerLocomotion.instance.transform);
            }
        }
    }
}
