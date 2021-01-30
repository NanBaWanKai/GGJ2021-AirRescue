using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
public class CargoHook : MonoBehaviourPun
{
    public Transform planeRoot;
    public Transform[] seats;
    int seated;
    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            var npc = other.GetComponentInParent<NPC>();
            if (npc && other == npc.hookPoint)
            {
                if (seated < seats.Length)
                {
                    npc.BoardPlane(planeRoot, planeRoot.InverseTransformPoint(seats[seated].position), Quaternion.Inverse(planeRoot.rotation) * seats[seated].rotation);
                    ++seated;
                }
            }
        }
    }
}
