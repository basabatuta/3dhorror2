using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public sealed class ArmsRigPushAnimation : MonoBehaviour
{
    private const string RigResourcePath = "arms_rig";
    private const string PushClipName = "ArmsRig|push.R";
    private const string CameraMountName = "PlayerCameraRoot";

    private GameObject _armsRig;
    private Animator _animator;
    private AnimationClip _pushClip;
    private PlayableGraph _playableGraph;
    private AnimationClipPlayable _pushPlayable;
    private float _animationElapsed;

    private void Awake()
    {
        Transform mount = transform.Find(CameraMountName);
        if (mount == null)
        {
            Debug.LogError($"Arms rig animation requires a child named {CameraMountName}.", this);
            enabled = false;
            return;
        }

        GameObject rigPrefab = Resources.Load<GameObject>(RigResourcePath);
        if (rigPrefab == null)
        {
            Debug.LogError($"Could not load the arms rig at Resources/{RigResourcePath}.", this);
            enabled = false;
            return;
        }

        _pushClip = FindPushClip();
        if (_pushClip == null)
        {
            Debug.LogError($"Could not find the {PushClipName} animation clip in Resources/{RigResourcePath}.", this);
            enabled = false;
            return;
        }

        _armsRig = Instantiate(rigPrefab, mount);
        _armsRig.transform.localPosition = Vector3.zero;
        _armsRig.transform.localRotation = Quaternion.identity;
        _armsRig.transform.localScale = Vector3.one;

        _animator = _armsRig.GetComponentInChildren<Animator>(true);
        if (_animator == null)
        {
            Debug.LogError("The arms rig has no Animator component.", this);
            Destroy(_armsRig);
            enabled = false;
            return;
        }

        _playableGraph = PlayableGraph.Create(nameof(ArmsRigPushAnimation));
        _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _pushPlayable = AnimationClipPlayable.Create(_playableGraph, _pushClip);
        _pushPlayable.SetApplyFootIK(false);
        _pushPlayable.SetApplyPlayableIK(false);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(
            _playableGraph,
            "Arms rig push",
            _animator);
        output.SetSourcePlayable(_pushPlayable);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _animationElapsed = 0f;
            _pushPlayable.SetTime(0d);
            _pushPlayable.SetSpeed(1d);
            _playableGraph.Play();
        }

        if (_playableGraph.IsValid() && _playableGraph.IsPlaying())
        {
            _animationElapsed += Time.deltaTime;
            if (_animationElapsed >= _pushClip.length)
            {
                _playableGraph.Stop();
                _animator.Rebind();
                _animator.Update(0f);
            }
        }
    }

    private void OnDestroy()
    {
        if (_playableGraph.IsValid())
            _playableGraph.Destroy();
    }

    private AnimationClip FindPushClip()
    {
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>(RigResourcePath);
        string expectedName = NormalizeClipName(PushClipName);
        foreach (AnimationClip clip in clips)
        {
            if (NormalizeClipName(clip.name).EndsWith(expectedName))
                return clip;
        }

        return null;
    }

    private static string NormalizeClipName(string clipName)
    {
        System.Text.StringBuilder normalizedName = new System.Text.StringBuilder(clipName.Length);
        foreach (char character in clipName)
        {
            if (char.IsLetterOrDigit(character))
                normalizedName.Append(char.ToLowerInvariant(character));
        }

        return normalizedName.ToString();
    }
}
