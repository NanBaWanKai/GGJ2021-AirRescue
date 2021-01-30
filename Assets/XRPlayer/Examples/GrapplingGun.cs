using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrapplingGun : MonoBehaviour
{
    public Transform hook;
    public LineRenderer line;
    public LayerMask grabableLayers;
    public LayerMask collisionLayers;
    public float ejectSpeed = 100f;
    public float maxDist = 50f;

    Transform attached;
    Rigidbody attachedRigidBody;
    Vector3 attachedPositionLS;
    Quaternion attachedRotationLS;
    

    public enum State { Ready, Ejecting, Attached};
    public State state;

    Vector3 hookPosition;
    Quaternion hookRotation;
    Vector3 ejectVelocity;
    Rigidbody body;
    XRGrabable grabable;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grabable = GetComponent<XRGrabable>();
        
    }
    bool triggerReleased = false;
    private void OnEnable()
    {
        //hook.parent = null;
    }
    private void FixedUpdate()
    {
        if (state == State.Ready)
        {
            hookPosition = transform.position;
            hookRotation = transform.rotation;

            if (grabable.hand && grabable.hand.input_trigger < .5f)
                triggerReleased = true;
            if (grabable.hand && grabable.hand.input_trigger > .5f && triggerReleased)
            {
                triggerReleased = false;
                ejectVelocity = transform.forward * ejectSpeed + body.velocity;
                state = State.Ejecting;
            }
        }
        else if (state == State.Ejecting)
        {
            if (ejectVelocity.magnitude <= .1f)
            {
                state = State.Ready;
            }
            else if (Vector3.Distance(transform.position, hookPosition) > maxDist)
            {
                state = State.Ready;
            }
            else if (!grabable.hand || grabable.hand.input_trigger < .5f)
            {
                state = State.Ready;
            }
            else
            {
                if(Physics.Raycast(hookPosition-ejectVelocity*Time.fixedDeltaTime*.1f, ejectVelocity.normalized,out var hitInfo, ejectVelocity.magnitude * Time.fixedDeltaTime, collisionLayers))
                {
                    if((grabableLayers.value & (1<< hitInfo.collider.gameObject.layer)) != 0)
                    {
                        attachedRigidBody = hitInfo.collider.attachedRigidbody;
                        if (attachedRigidBody != body)
                        {
                            attached = attachedRigidBody ? attachedRigidBody.transform : hitInfo.collider.transform;
                            hookPosition = hitInfo.point;
                            attachedPositionLS = attached.InverseTransformPoint(hitInfo.point);
                            attachedRotationLS = Quaternion.Inverse(attached.rotation) * hookRotation;
                            state = State.Attached;
                        }
                    }
                    else
                    {
                        state = State.Ready;
                    }
                }
                else
                {
                    hookPosition += ejectVelocity * Time.fixedDeltaTime;
                }
            }

        }
        else if(state==State.Attached)
        {
            if (!attached)
            {
                state = State.Ready;
            }
            else if (!grabable.hand || grabable.hand.input_trigger < .5f)
            {
                state = State.Ready;
            }
            else
            {
                hookPosition = attached.TransformPoint(attachedPositionLS);
                hookRotation = attached.rotation * attachedRotationLS;
                if (attachedRigidBody)
                    hookPosition += attachedRigidBody.velocity * Time.fixedDeltaTime;
                XRPlayerLocomotion.instance.SetHook(attached, attachedRigidBody, attachedPositionLS);
            }
        }
        hook.position = hookPosition;
        hook.rotation = hookRotation;
        line.useWorldSpace = true;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, hookPosition);
    }
}
