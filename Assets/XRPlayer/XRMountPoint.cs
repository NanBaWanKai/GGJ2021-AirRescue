using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class XRMountPoint : MonoBehaviour
{
    public List<string> acceptedMountTags;

    [HideInInspector]
    public Rigidbody body;
    [HideInInspector]
    public XRGrabable mounted;
    public bool CanMount(XRGrabable g)
    {
        if (!isActiveAndEnabled) return false;
        if (!acceptedMountTags.Contains(g.mountTag)) return false;
        return true;
    }
    public void Mount(XRGrabable g)
    {
        if (mounted) mounted.UnmountIfMounted();
        mounted = g;
    }
    public void UnMount(XRGrabable g)
    {
        if (mounted == g) mounted = null;
    }
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }
    private void OnDisable()
    {
        if (mounted) mounted.UnmountIfMounted();
    }
}
