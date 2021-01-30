using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SpawnPlayerEquipment : MonoBehaviour
{
    public string[] prefabNames;
    public string[] mountPoints;
    public void SpawnEquipment(GameObject player)
    {
        for(int i = 0; i < prefabNames.Length; ++i)
        {

            var prefab=PhotonNetwork.Instantiate(prefabNames[i], player.transform.position, player.transform.rotation);
            var grabable = prefab.GetComponent<XRGrabable>();
            var mountPoint = player.transform.Find(mountPoints[i])?.GetComponent<XRMountPoint>();
            if(grabable && mountPoint)
                grabable.mountOnDrop = mountPoint;
        }
    }
}
