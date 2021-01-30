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
    void TryMount(XRGrabable g)
    {
        g.TryMount(this);
    }
    void UnmountIfMounted()
    {
        if (mounted) mounted.UnmountIfMounted();
    }
    public void _Mount(XRGrabable g)
    {
        if (mounted && mounted!=g) mounted.UnmountIfMounted();
        mounted = g;
    }
    public void _UnMount(XRGrabable g)
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
