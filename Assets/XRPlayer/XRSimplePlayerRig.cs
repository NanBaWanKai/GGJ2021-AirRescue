using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRSimplePlayerRig : MonoBehaviour
{
    public GameObject avaterRoot;
    public int hiddenLayer, unhideLayer;
    public Transform head, body;
    public Transform trackedHead, playerRoot;
    public MeshRenderer[] controllerMeshs;
    public float neckLength = 0,neckLength2=.4f;
    float scale => playerRoot.lossyScale.x;

    void CopyPosRot(Transform from, Transform to) { to.position = from.position;to.rotation = from.rotation; }
    //[Button]
    private void Update()
    {
        CopyPosRot(trackedHead, head);
        Vector3 bodyPos = head.position - head.up * neckLength-playerRoot.up* neckLength2;
        body.position = bodyPos;
        body.rotation = playerRoot.rotation;
    }
    public void SetupLocalPlayer()
    {
        SetLayerRecursively(avaterRoot, hiddenLayer);
        foreach (var m in controllerMeshs)
            m.enabled = true;
    }
    public void SetupRemotePlayer()
    {
        SetLayerRecursively(avaterRoot, unhideLayer);
        foreach (var m in controllerMeshs)
            m.enabled = false;
    }
    static void SetLayerRecursively(GameObject go, int layer)
    {
        if (go != null)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; ++i)
                SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
        }
    }
}
