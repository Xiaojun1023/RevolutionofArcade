using UnityEngine;

public class DoorAutoOpen : MonoBehaviour
{
    private const string LeftDoorName = "BP_Door_C_1_SM_ArcadeDoors_Left_01a_3_12";
    private const string RightDoorName = "BP_Door_C_1_SM_ArcadeDoors_Right_01a_2_11";

    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float triggerRadius = 3f;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private bool useHorizontalDistance = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Vector3 colliderPadding = new Vector3(0.08f, 0.08f, 0.08f);

    private Quaternion leftClosedLocalRotation;
    private Quaternion rightClosedLocalRotation;
    private Quaternion leftOpenLocalRotation;
    private Quaternion rightOpenLocalRotation;
    private Vector3 doorCenterWorldPosition;
    private float nextPlayerLookupTime;
    private bool hasCachedDoorState;

    private void Reset()
    {
        AutoAssignDoors();
    }

    private void Awake()
    {
        AutoAssignDoors();
        CacheDoorState();
        ConfigureDoorRuntimeSetup(leftDoor);
        ConfigureDoorRuntimeSetup(rightDoor);
        TryResolvePlayerTransform(forceLookup: true);
    }

    private void Update()
    {
        if (!hasCachedDoorState)
        {
            AutoAssignDoors();
            CacheDoorState();
            if (!hasCachedDoorState)
            {
                return;
            }
        }

        doorCenterWorldPosition = (leftDoor.position + rightDoor.position) * 0.5f;
        TryResolvePlayerTransform(forceLookup: false);

        bool shouldOpen = IsPlayerInRange();
        Quaternion leftTarget = shouldOpen ? leftOpenLocalRotation : leftClosedLocalRotation;
        Quaternion rightTarget = shouldOpen ? rightOpenLocalRotation : rightClosedLocalRotation;
        float step = rotationSpeed * Time.deltaTime;

        leftDoor.localRotation = Quaternion.RotateTowards(leftDoor.localRotation, leftTarget, step);
        rightDoor.localRotation = Quaternion.RotateTowards(rightDoor.localRotation, rightTarget, step);
    }

    private void AutoAssignDoors()
    {
        if (leftDoor != null && rightDoor != null)
        {
            return;
        }

        foreach (Transform child in transform)
        {
            if (leftDoor == null && child.name == LeftDoorName)
            {
                leftDoor = child;
            }

            if (rightDoor == null && child.name == RightDoorName)
            {
                rightDoor = child;
            }
        }
    }

    private void CacheDoorState()
    {
        if (leftDoor == null || rightDoor == null)
        {
            hasCachedDoorState = false;
            return;
        }

        leftClosedLocalRotation = leftDoor.localRotation;
        rightClosedLocalRotation = rightDoor.localRotation;
        doorCenterWorldPosition = (leftDoor.position + rightDoor.position) * 0.5f;
        leftOpenLocalRotation = leftClosedLocalRotation * Quaternion.Euler(0f, CalculateOpenAngleDelta(leftDoor), 0f);
        rightOpenLocalRotation = rightClosedLocalRotation * Quaternion.Euler(0f, CalculateOpenAngleDelta(rightDoor), 0f);
        hasCachedDoorState = true;
    }

    private void ConfigureDoorRuntimeSetup(Transform door)
    {
        if (door == null)
        {
            return;
        }

        SetStaticRecursively(door, false);
        EnsureBoxCollider(door);
    }

    private void SetStaticRecursively(Transform root, bool isStatic)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.isStatic = isStatic;
        foreach (Transform child in root)
        {
            SetStaticRecursively(child, isStatic);
        }
    }

    private void EnsureBoxCollider(Transform door)
    {
        if (door == null)
        {
            return;
        }

        MeshCollider meshCollider = door.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.enabled = false;
        }

        if (!TryCalculateLocalRendererBounds(door, out Vector3 center, out Vector3 size))
        {
            return;
        }

        BoxCollider boxCollider = door.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = door.gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = false;
        boxCollider.center = center;
        boxCollider.size = size + colliderPadding;
    }

    private bool TryCalculateLocalRendererBounds(Transform root, out Vector3 localCenter, out Vector3 localSize)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            localCenter = Vector3.zero;
            localSize = Vector3.one;
            return false;
        }

        bool hasValidBounds = false;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            Vector3 ext = worldBounds.extents;
            Vector3 cen = worldBounds.center;

            Vector3[] corners =
            {
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
            };

            for (int c = 0; c < corners.Length; c++)
            {
                Vector3 localPoint = root.InverseTransformPoint(corners[c]);
                if (!hasValidBounds)
                {
                    min = localPoint;
                    max = localPoint;
                    hasValidBounds = true;
                }
                else
                {
                    min = Vector3.Min(min, localPoint);
                    max = Vector3.Max(max, localPoint);
                }
            }
        }

        if (!hasValidBounds)
        {
            localCenter = Vector3.zero;
            localSize = Vector3.one;
            return false;
        }

        localCenter = (min + max) * 0.5f;
        localSize = max - min;
        return true;
    }

    private float CalculateOpenAngleDelta(Transform door)
    {
        float directionSign = Mathf.Sign(Vector3.Dot(door.position - doorCenterWorldPosition, door.right));
        if (Mathf.Approximately(directionSign, 0f))
        {
            directionSign = 1f;
        }

        // Reverse sign so both leaves swing inward (toward interior) in this scene setup.
        return Mathf.Clamp(openAngle, 0f, 179f) * -directionSign;
    }

    private void TryResolvePlayerTransform(bool forceLookup)
    {
        if (playerTransform != null && playerTransform.gameObject.activeInHierarchy)
        {
            return;
        }

        if (!forceLookup && Time.time < nextPlayerLookupTime)
        {
            return;
        }

        nextPlayerLookupTime = Time.time + 1f;

        CharacterController[] controllers = FindObjectsOfType<CharacterController>();
        if (TrySelectClosestTransform(controllers, out Transform controllerTransform))
        {
            playerTransform = controllerTransform;
            return;
        }

        if (!string.IsNullOrEmpty(playerTag))
        {
            try
            {
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(playerTag);
                if (TrySelectClosestTransform(taggedObjects, out Transform taggedTransform))
                {
                    playerTransform = taggedTransform;
                    return;
                }
            }
            catch (UnityException)
            {
                // Ignore invalid or missing tags and continue with fallbacks.
            }
        }

        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }
    }

    private bool TrySelectClosestTransform(CharacterController[] controllers, out Transform result)
    {
        result = null;
        if (controllers == null || controllers.Length == 0)
        {
            return false;
        }

        float closestSqrDistance = float.MaxValue;
        for (int i = 0; i < controllers.Length; i++)
        {
            CharacterController controller = controllers[i];
            if (controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy)
            {
                continue;
            }

            float sqrDistance = (controller.transform.position - doorCenterWorldPosition).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                result = controller.transform;
            }
        }

        return result != null;
    }

    private bool TrySelectClosestTransform(GameObject[] gameObjects, out Transform result)
    {
        result = null;
        if (gameObjects == null || gameObjects.Length == 0)
        {
            return false;
        }

        float closestSqrDistance = float.MaxValue;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            GameObject gameObject = gameObjects[i];
            if (gameObject == null || !gameObject.activeInHierarchy)
            {
                continue;
            }

            float sqrDistance = (gameObject.transform.position - doorCenterWorldPosition).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                result = gameObject.transform;
            }
        }

        return result != null;
    }

    private bool IsPlayerInRange()
    {
        if (playerTransform == null)
        {
            return false;
        }

        Vector3 playerPosition = playerTransform.position;
        Vector3 centerPosition = doorCenterWorldPosition;

        if (useHorizontalDistance)
        {
            playerPosition.y = 0f;
            centerPosition.y = 0f;
        }

        float range = Mathf.Max(0f, triggerRadius);
        return (playerPosition - centerPosition).sqrMagnitude <= range * range;
    }
}
