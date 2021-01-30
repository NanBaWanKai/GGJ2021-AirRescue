using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHook : MonoBehaviour
{
    public LineRenderer line;
    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponentInParent<NPC>();
        if(npc && other == npc.hookPoint)
        {
        }
    }
}
