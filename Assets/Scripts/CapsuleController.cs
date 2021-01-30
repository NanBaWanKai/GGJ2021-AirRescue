using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PhysicsEX = UnityEngine.Physics;

[RequireComponent(typeof(CapsuleCollider))]
public class CapsuleController : MonoBehaviour
{
    public LayerMask environmentLayers = -1;
    public LayerMask groundLayers = 0;
    public float stepHeight = .3f;
    public float slopeLimit = 60f;
    public SphereCollider headCollider;

    [System.Serializable] public class UpdateTransform : UnityEvent<Transform> { }
    public UpdateTransform updateAttach = new UpdateTransform();
    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }
    private void OnValidate()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        Debug.Assert(capsuleCollider.enabled == false);
    }
    #region Attach
    Transform attached;
    Rigidbody attachedBody;
    Vector3 attachAnchorPositionLS = Vector3.zero, attachedPositionLS;
    Vector3 attachedVelocity = Vector3.zero;
    //TODO Attach
    public Vector3 GetAttachedTranslation(float dt = 0, float smoothTime = .1f)
    {
        if (attached)
        {
            Vector3 translation = attached.TransformPoint(attachedPositionLS) - transform.TransformPoint(attachAnchorPositionLS);
            if (attachedBody) translation += attachedBody.velocity * Time.fixedDeltaTime;//fast moving vehicles, predict the position after the physics frame
            if (dt > 0)
                attachedVelocity = Vector3.Lerp(attachedVelocity, translation / dt, dt / smoothTime);
            return translation;
        }
        else return Vector3.zero;
    }
    public void SetAttach(Transform attached, Vector3 attachAnchorPositionLS)
    {
        this.attached = attached;
        this.attachAnchorPositionLS = attachAnchorPositionLS;
        if (attached) attachedBody = attached.GetComponentInParent<Rigidbody>();
        if (attachedBody && attachedBody.isKinematic && attachedBody.transform.parent)
        {
            var parentBody = attachedBody.transform.parent.GetComponentInParent<Rigidbody>();
            if (parentBody) attachedBody = parentBody;
        }
        Vector3 attachedPointWS = transform.TransformPoint(attachAnchorPositionLS);
        if (attachedBody) attachedPointWS -= attachedBody.velocity * Time.fixedDeltaTime;//Compensate the correlation in GetAttachedTranslation
        if (attached) attachedPositionLS = attached.InverseTransformPoint(attachedPointWS);
        updateAttach.Invoke(this.attached);
    }
    #endregion
    #region CapsuleCollider
    public Vector3 position { get => transform.position; set => transform.position = value; }
    public Quaternion rotation { get => transform.rotation; set => transform.rotation = value; }
    CapsuleCollider capsuleCollider;
    float R => capsuleCollider.radius;
    float H => capsuleCollider.height;
    float scale => transform.lossyScale.x;
    Vector3 up => transform.up;
    Vector3 GravityUp => transform.up;
    Vector3 TP(float x, float y, float z) => transform.TransformPoint(new Vector3(x, y, z));
    Vector3 TV(float x, float y, float z) => transform.TransformVector(new Vector3(x, y, z));
    Vector3 TD(float x, float y, float z) => transform.TransformDirection(new Vector3(x, y, z));
    Vector3 P1 => TP(0, R, 0);
    Vector3 P2 => TP(0, H - R, 0);
    public Vector3 ProjectStepOnGround(Vector3 delta, out bool isNextStepHit)
    {
        float stepDist = Mathf.Max(delta.magnitude, R * scale);
        isNextStepHit = PhysicsEX.SphereCast(TP(0, .9f * R + stepHeight, 0) + delta.normalized * Mathf.Max(.9f * R * scale, delta.magnitude),
            .9f * R * scale, TD(0, -1, 0), out RaycastHit nextStepHit, 2 * stepHeight * scale, groundLayers,QueryTriggerInteraction.Ignore);
        if (isNextStepHit)
        {
            Vector3 nextStep = TP(0, .9f * R + stepHeight, 0) + delta.normalized * Mathf.Max(.9f * R * scale, delta.magnitude) + nextStepHit.distance * TD(0, -1, 0) + TV(0, -.9f * R, 0);
            if (Vector3.Dot(nextStepHit.point - transform.position, GravityUp) < Mathf.Sin(slopeLimit * Mathf.Deg2Rad))
                delta = (nextStep - transform.position).normalized * delta.magnitude;
            else
                delta = Vector3.zero;
        }
        return delta;
    }
    public bool CheckGrounded(out float groundDist, out RaycastHit groundHit)
    {
        bool isThisStep = PhysicsEX.SphereCast(TP(0, .9f * R + stepHeight, 0),
            .9f * R * scale, TD(0, -1, 0), out groundHit, 2 * stepHeight * scale, groundLayers, QueryTriggerInteraction.Ignore);
        groundDist = isThisStep ? groundHit.distance - stepHeight * scale : float.PositiveInfinity;
        return isThisStep;
    }
    public Vector3 SweepCollider(Vector3 delta, bool slide, out bool isHit, out RaycastHit hit)
    {
        //Sweep
        isHit = PhysicsEX.CapsuleCast(P1, P2, .9f * R * scale, delta.normalized, out hit, delta.magnitude + .1f * R * scale, environmentLayers, QueryTriggerInteraction.Ignore);
        if (isHit)
        {
            Vector3 delta1 = delta.normalized * Mathf.Clamp(hit.distance - .2f * R * scale, 0, delta.magnitude);//Skinning is needed 
            //Additional Skinning to fix collider default skinning, otherwise will result to sliding when head-leaning
            if (slide)
                return delta1;
            else
            {
                Vector3 delta2 = Vector3.ProjectOnPlane(delta - delta1, hit.normal);
                bool isHit2 = PhysicsEX.CapsuleCast(P1 + delta1, P2 + delta1, .9f * R, delta2.normalized, out RaycastHit hit2, delta2.magnitude + .1f * R * scale, environmentLayers, QueryTriggerInteraction.Ignore);
                if (isHit2)
                {
                    return delta1 + delta2.normalized * Mathf.Clamp(hit2.distance - .1f * R * scale, 0, delta2.magnitude);
                }
                else
                {
                    return delta1 + delta2;
                }
            }
        }
        else
            return delta;
    }
    public Vector3 ResolveCollision(bool resolveHead = true)
    {
        Vector3 totalMoved = Vector3.zero;
        //Head Collision Resolving(?)
        int overlapCount; bool tmp;
        if (resolveHead && headCollider)
        {
            overlapCount = Physics.OverlapSphereNonAlloc(headCollider.transform.position, headCollider.radius * scale, colliderBuffer, environmentLayers);
            tmp = headCollider.enabled;
            headCollider.enabled = true;
            for (int i = 0; i < overlapCount; ++i)
            {
                var c = colliderBuffer[i];
                if (c == capsuleCollider || c == headCollider) continue;
                if (Physics.ComputePenetration(headCollider, headCollider.transform.position, headCollider.transform.rotation, c, c.transform.position, c.transform.rotation,
                    out Vector3 resolveDir, out float resolveDist))
                {
                    transform.position += resolveDir * resolveDist;
                    totalMoved += resolveDir * resolveDist;
                }
            }
            headCollider.enabled = tmp;
        }

        //Body Collision Resolving
        overlapCount = Physics.OverlapCapsuleNonAlloc(P1, P2, R * scale * 1.1f, colliderBuffer, environmentLayers);
        tmp = capsuleCollider.enabled;
        capsuleCollider.enabled = true;
        for (int i = 0; i < overlapCount; ++i)
        {
            var c = colliderBuffer[i];
            if (c == capsuleCollider || c == headCollider) continue;
            if (Physics.ComputePenetration(capsuleCollider, capsuleCollider.transform.position, capsuleCollider.transform.rotation, c, c.transform.position, c.transform.rotation,
                out Vector3 resolveDir, out float resolveDist))
            {
                transform.position += resolveDir * resolveDist;
                totalMoved += resolveDir * resolveDist;
            }
        }
        capsuleCollider.enabled = tmp;
        return totalMoved;
    }
    Collider[] colliderBuffer = new Collider[20];
    #endregion
}
