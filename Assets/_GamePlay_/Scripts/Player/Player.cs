using UnityEngine;

public class Player : Ply_GameUnit
{
    [Header("Reference")]
    public PlayerGraphicController graphic;
    public PlayerMovement movement;

    // Pushing state
    private Box pushingBox;
    private FixedJoint pushJoint;
    private Vector3 pushOffset;
    public float pushFollowSpeed = 12f;

    public void Despawn()
    {
        Ply_Pool.Ins.Despawn(PoolType.Player, this);
    }

    public void StartPushing(Box box, Rigidbody playerRb)
    {
        if (box == null) return;
        // stop any previous push
        ForceStopPushing();

        pushingBox = box;
        // record offset so box keeps relative position to player while pushing
        pushOffset = pushingBox.transform.position - this.transform.position;
        // ensure box is kinematic so we move it by transform to avoid joint jitter
        pushingBox.SetKinematic(true);
        // set animator flag if available
        if (movement != null && movement.animator != null)
        {
            movement.animator.SetBool("isPushing", true);
        }
    }

    public Box StopPushing()
    {
        Box b = pushingBox;
        if (pushJoint != null)
        {
            Destroy(pushJoint);
            pushJoint = null;
        }
        pushingBox = null;
        // clear animator flag
        if (movement != null && movement.animator != null)
        {
            movement.animator.SetBool("isPushing", false);
        }
        return b;
    }

    public void ForceStopPushing()
    {
        if (pushJoint != null)
        {
            Destroy(pushJoint);
            pushJoint = null;
        }
        pushingBox = null;
        if (movement != null && movement.animator != null)
        {
            movement.animator.SetBool("isPushing", false);
        }
    }

    public bool IsPushing()
    {
        return pushingBox != null;
    }

    public Box GetPushingBox()
    {
        return pushingBox;
    }

    private void Update()
    {
        if (pushingBox != null)
        {
            Vector3 target = this.transform.position + pushOffset;
            pushingBox.transform.position = Vector3.Lerp(pushingBox.transform.position, target, Time.deltaTime * pushFollowSpeed);
        }
    }
}
