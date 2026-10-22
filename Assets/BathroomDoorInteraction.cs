using UnityEngine;
using System.Collections;

public class BathroomDoorInteraction : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float interactionDistance = 2f;
    public bool isOpen = false;

    private Transform _playerTransform;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;
    private Collider[] _interactionColliders;
    private Vector3 _hingeLocalPosition;
    private Vector3 _hingeWorldPosition;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Door interaction requires a GameObject tagged Player.", this);
            enabled = false;
            return;
        }

        _playerTransform = player.transform;
        _interactionColliders = GetComponentsInChildren<Collider>();

        // Store the starting rotation as closed
        _closedRotation = transform.rotation;
        _hingeLocalPosition = FindLeftEdge();
        _hingeWorldPosition = transform.TransformPoint(_hingeLocalPosition);
        
        // Swing around the world vertical axis so the door opens horizontally.
        _openRotation = Quaternion.AngleAxis(openAngle, Vector3.up) * _closedRotation;
    }

    void Update()
    {
        if (DoorInteractionInput.IsWithinRange(_playerTransform, transform, _interactionColliders, interactionDistance) &&
            DoorInteractionInput.TryConsumeKeyPress())
        {
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            
            // Toggle the state before starting the movement
            isOpen = !isOpen; 
            _currentCoroutine = StartCoroutine(ToggleDoor());
        }
    }

    // The missing Coroutine function from your image
    IEnumerator ToggleDoor()
    {
        // Choose target based on current state
        Quaternion targetRotation = isOpen ? _openRotation : _closedRotation;
        
        // Smoothly rotate toward the target
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            Quaternion nextRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            SetRotationAroundHinge(nextRotation);
            yield return null; // Wait for the next frame
        }
        
        // Snap precisely to the target at the end
        SetRotationAroundHinge(targetRotation);
        _currentCoroutine = null;
    }

    Vector3 FindLeftEdge()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return Vector3.zero;

        Bounds localBounds = default;
        bool hasBounds = false;

        foreach (Renderer renderer in renderers)
        {
            Bounds worldBounds = renderer.bounds;
            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;

            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 worldCorner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                Vector3 localCorner = transform.InverseTransformPoint(worldCorner);
                if (hasBounds)
                    localBounds.Encapsulate(localCorner);
                else
                {
                    localBounds = new Bounds(localCorner, Vector3.zero);
                    hasBounds = true;
                }
            }
        }

        return new Vector3(localBounds.min.x, localBounds.center.y, localBounds.center.z);
    }

    void SetRotationAroundHinge(Quaternion rotation)
    {
        transform.rotation = rotation;
        transform.position = _hingeWorldPosition - rotation * Vector3.Scale(transform.lossyScale, _hingeLocalPosition);
    }
}
