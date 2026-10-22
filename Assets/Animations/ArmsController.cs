using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class ArmsController : MonoBehaviour
{
    public string triggerName = "Push";  // must match your Animator parameter
    public float hideDelay = 1.0f;       // length of the push clip

    Animator anim;
    Renderer[] renderers;

    void Awake()
    {
        anim = GetComponent<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false);
    }

    public void Push()
    {
        StopAllCoroutines();
        SetVisible(true);
        anim.SetTrigger(triggerName);
        StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        SetVisible(false);
    }

    void SetVisible(bool visible)
    {
        foreach (var r in renderers) r.enabled = visible;
    }
}