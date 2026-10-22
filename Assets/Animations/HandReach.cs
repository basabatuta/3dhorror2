using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HandReach : MonoBehaviour
{
    public Transform target;
    [Range(0, 1)] public float weight = 0f;
    public float blendSpeed = 5f;

    Animator anim;
    Renderer[] armRenderers;

    void Awake()
    {
        anim = GetComponent<Animator>();
        armRenderers = GetComponentsInChildren<Renderer>();
        SetVisible(false); // hidden by default
    }

    void OnAnimatorIK(int layerIndex)
    {
        float goal = target != null ? 1f : 0f;
        weight = Mathf.MoveTowards(weight, goal, blendSpeed * Time.deltaTime);

        SetVisible(weight > 0.01f);

        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);

        if (target != null)
        {
            anim.SetIKPosition(AvatarIKGoal.RightHand, target.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand, target.rotation);
        }
    }

    void SetVisible(bool visible)
    {
        foreach (var r in armRenderers)
            if (r.enabled != visible) r.enabled = visible;
    }
}