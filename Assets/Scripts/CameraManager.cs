using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private const float _threshold = 0.01f;
    private const int DefaultPriority = 10;
    private const int PlayerPriority = 15;

    [Tooltip("A reference to the PlayerFollowCamera prefab.")]
    public GameObject VirtualCameraPrefab;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public Transform CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    [Tooltip("Select this for the cameraManager that belongs to the player")]
    [SerializeField] private bool InitializeAsPlayerCamera = false;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        virtualCamera = Instantiate(VirtualCameraPrefab, transform.position, transform.rotation).GetComponent<CinemachineVirtualCamera>();
        virtualCamera.Priority = InitializeAsPlayerCamera ? PlayerPriority : DefaultPriority;
        virtualCamera.Follow = CinemachineCameraTarget;
    }

    public void TogglePlayerCamera(bool toggleAsPlayer)
    {
        virtualCamera.Priority = toggleAsPlayer ? PlayerPriority : DefaultPriority;
    }

    public void CameraRotation(Vector2 lookVector)
    {
        // if there is an input and camera position is not fixed
        if (lookVector.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += lookVector.x * Time.deltaTime;
            _cinemachineTargetPitch += lookVector.y * Time.deltaTime;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
