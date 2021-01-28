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

    public Transform attachedPos;
    public Rigidbody attachedRigidBody;

    public enum State { Ready, Ejecting, Attached};
    public State state;

    Vector3 ejectVelocity;
    Rigidbody body;
    XRGrabable grabable;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grabable = GetComponent<XRGrabable>();
    }
    private void FixedUpdate()
    {
        if (state == State.Ready)
        {
            if(grabable.hand && grabable.hand.input_trigger > .5f)
            {
                ejectVelocity = transform.forward * ejectSpeed + body.velocity;
                state = State.Ejecting;
            }
        }else if (state == State.Ejecting)
        {

        }
        else if(state==State.Attached)
        {
            if (grabable.hand && grabable.hand.input_trigger > .5f)
            {
                state = State.Ready;
            }

        }

    }
}
