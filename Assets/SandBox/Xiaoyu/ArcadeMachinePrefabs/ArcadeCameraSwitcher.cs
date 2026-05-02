using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ArcadeCameraSwitcher : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera arcadeCamera;
    [SerializeField] private Transform playerTransformRoot;

    [Header("Controls")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private Collider interactVolume;
    [SerializeField] private bool requireInsideInteractVolume = true;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private bool requireFacingMachine = false;
    [SerializeField] [Range(-1f, 1f)] private float facingDotThreshold = 0.35f;

    [Header("Player Lock (Optional)")]
    [SerializeField] private List<Behaviour> disableComponentsOnEnter = new();
    [SerializeField] private bool lockAndHideCursorInArcade = true;

    private readonly Dictionary<Behaviour, bool> cachedEnabledStates = new();
    private Camera blendCamera;
    private bool inArcadeView;
    private bool isTransitioning;

    private void Reset()
    {
        arcadeCamera = GetComponentInChildren<Camera>(true);
        TryResolvePlayerReferences();
        TryResolveInteractVolume();
    }

    private void Awake()
    {
        TryResolvePlayerReferences();
        TryResolveInteractVolume();
        EnsureBlendCamera();
        SetCameraState(arcadeCamera, false);
        SetCameraState(blendCamera, false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(interactKey) || isTransitioning)
            return;

        if (!inArcadeView)
        {
            if (!CanEnterArcade())
                return;

            StartCoroutine(TransitionRoutine(enteringArcade: true));
        }
        else
        {
            StartCoroutine(TransitionRoutine(enteringArcade: false));
        }
    }

    private bool CanEnterArcade()
    {
        if (!playerCamera || !arcadeCamera)
            return false;

        TryResolveInteractVolume();

        if (requireInsideInteractVolume && interactVolume)
        {
            if (!IsPlayerInsideInteractVolume())
                return false;
        }
        else
        {
            Vector3 toMachine = transform.position - playerCamera.transform.position;
            if (toMachine.sqrMagnitude > interactDistance * interactDistance)
                return false;
        }

        if (!requireFacingMachine)
            return true;

        Vector3 toMachineFacing = transform.position - playerCamera.transform.position;
        Vector3 forward = playerCamera.transform.forward.normalized;
        Vector3 directionToMachine = toMachineFacing.normalized;
        return Vector3.Dot(forward, directionToMachine) >= facingDotThreshold;
    }

    private IEnumerator TransitionRoutine(bool enteringArcade)
    {
        TryResolvePlayerReferences();

        if (!playerCamera || !arcadeCamera)
            yield break;

        Camera from = enteringArcade ? playerCamera : arcadeCamera;
        Camera to = enteringArcade ? arcadeCamera : playerCamera;

        isTransitioning = true;

        if (enteringArcade)
        {
            SetPlayerControlsEnabled(false);
            if (lockAndHideCursorInArcade)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        EnsureBlendCamera();
        CopyCameraSettings(from, blendCamera);

        blendCamera.transform.SetPositionAndRotation(from.transform.position, from.transform.rotation);
        blendCamera.fieldOfView = from.fieldOfView;

        SetCameraState(from, false);
        SetCameraState(to, false);
        SetCameraState(blendCamera, true);

        float duration = Mathf.Max(0.01f, transitionDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            blendCamera.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, smooth);
            blendCamera.transform.rotation = Quaternion.Slerp(from.transform.rotation, to.transform.rotation, smooth);
            blendCamera.fieldOfView = Mathf.Lerp(from.fieldOfView, to.fieldOfView, smooth);

            yield return null;
        }

        blendCamera.transform.SetPositionAndRotation(to.transform.position, to.transform.rotation);
        blendCamera.fieldOfView = to.fieldOfView;

        SetCameraState(blendCamera, false);
        SetCameraState(to, true);

        if (!enteringArcade)
        {
            SetPlayerControlsEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        inArcadeView = enteringArcade;
        isTransitioning = false;
    }

    private void TryResolvePlayerReferences()
    {
        if (!arcadeCamera)
            arcadeCamera = GetComponentInChildren<Camera>(true);

        if (!playerCamera)
            playerCamera = FindBestPlayerCamera();

        if (!playerTransformRoot && playerCamera)
            playerTransformRoot = playerCamera.transform.parent;
    }

    private void TryResolveInteractVolume()
    {
        if (interactVolume)
            return;

        Collider rootCollider = GetComponent<Collider>();
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (!col || col == rootCollider)
                continue;

            if (col is BoxCollider && col.isTrigger)
            {
                interactVolume = col;
                return;
            }
        }

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (!col || col == rootCollider)
                continue;

            if (col.isTrigger)
            {
                interactVolume = col;
                return;
            }
        }
    }

    private bool IsPlayerInsideInteractVolume()
    {
        if (!interactVolume)
            return false;

        Vector3 samplePoint = playerCamera ? playerCamera.transform.position : transform.position;

        if (playerTransformRoot)
        {
            samplePoint = playerTransformRoot.position;

            if (playerTransformRoot.TryGetComponent<CharacterController>(out CharacterController controller))
                samplePoint = controller.bounds.center;
        }

        if (IsPointInsideCollider(interactVolume, samplePoint))
            return true;

        if (playerCamera && IsPointInsideCollider(interactVolume, playerCamera.transform.position))
            return true;

        return false;
    }

    private static bool IsPointInsideCollider(Collider col, Vector3 point)
    {
        Vector3 closest = col.ClosestPoint(point);
        return (closest - point).sqrMagnitude <= 0.0001f;
    }

    private Camera FindBestPlayerCamera()
    {
        GameObject named = GameObject.Find("PlayerCamera");
        if (named && named.TryGetComponent(out Camera namedCamera))
            return namedCamera;

        Camera[] allCameras = Object.FindObjectsOfType<Camera>(true);

        for (int i = 0; i < allCameras.Length; i++)
        {
            Camera cam = allCameras[i];
            if (cam.GetComponent("PlayerLook") != null)
                return cam;
        }

        for (int i = 0; i < allCameras.Length; i++)
        {
            Camera cam = allCameras[i];
            if (cam.gameObject.name == "PlayerCamera")
                return cam;
        }

        for (int i = 0; i < allCameras.Length; i++)
        {
            Camera cam = allCameras[i];
            if (cam.CompareTag("Player"))
                return cam;
        }

        return allCameras.Length > 0 ? allCameras[0] : null;
    }

    private void EnsureBlendCamera()
    {
        if (blendCamera)
            return;

        GameObject blendObject = new GameObject("ArcadeBlendCamera");
        blendObject.hideFlags = HideFlags.HideAndDontSave;
        blendCamera = blendObject.AddComponent<Camera>();

        Camera source = playerCamera ? playerCamera : arcadeCamera;
        if (source)
        {
            CopyCameraSettings(source, blendCamera);
            blendCamera.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            blendCamera.fieldOfView = source.fieldOfView;
        }
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        if (!enabled)
            cachedEnabledStates.Clear();

        if (disableComponentsOnEnter.Count == 0)
        {
            if (playerCamera)
            {
                Behaviour look = playerCamera.GetComponent("PlayerLook") as Behaviour;
                if (look)
                    disableComponentsOnEnter.Add(look);
            }

            if (playerTransformRoot)
            {
                Behaviour move = playerTransformRoot.GetComponent("PlayerMove") as Behaviour;
                if (move && !disableComponentsOnEnter.Contains(move))
                    disableComponentsOnEnter.Add(move);
            }
        }

        for (int i = 0; i < disableComponentsOnEnter.Count; i++)
        {
            Behaviour behaviour = disableComponentsOnEnter[i];
            if (!behaviour)
                continue;

            if (!enabled)
            {
                cachedEnabledStates[behaviour] = behaviour.enabled;
                behaviour.enabled = false;
            }
            else
            {
                if (cachedEnabledStates.TryGetValue(behaviour, out bool wasEnabled))
                    behaviour.enabled = wasEnabled;
                else
                    behaviour.enabled = true;
            }
        }

        if (enabled)
            cachedEnabledStates.Clear();
    }

    private static void SetCameraState(Camera cam, bool enabled)
    {
        if (!cam)
            return;

        cam.enabled = enabled;

        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener)
            listener.enabled = enabled;
    }

    private static void CopyCameraSettings(Camera source, Camera destination)
    {
        if (!source || !destination)
            return;

        destination.clearFlags = source.clearFlags;
        destination.backgroundColor = source.backgroundColor;
        destination.cullingMask = source.cullingMask;
        destination.nearClipPlane = source.nearClipPlane;
        destination.farClipPlane = source.farClipPlane;
        destination.orthographic = source.orthographic;
        destination.orthographicSize = source.orthographicSize;
        destination.allowHDR = source.allowHDR;
        destination.allowMSAA = source.allowMSAA;
        destination.depth = 100f;
    }

    private void OnDisable()
    {
        if (!inArcadeView)
            return;

        SetCameraState(blendCamera, false);
        SetCameraState(arcadeCamera, false);
        SetCameraState(playerCamera, true);
        SetPlayerControlsEnabled(true);
        inArcadeView = false;
    }

    private void OnDestroy()
    {
        if (blendCamera)
            Destroy(blendCamera.gameObject);
    }
}
