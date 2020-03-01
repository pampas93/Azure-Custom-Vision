using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class RoomManager : MonoSingleton<RoomManager>
{
    [SerializeField]
    private GameObject roomPlanes;

    [SerializeField]
    private Camera captureCamera;

    [SerializeField]
    private Transform currentPlane;

    void Start()
    {
#if !UNITY_EDITOR
        roomPlanes.SetActive(false);
#endif

        DrawManager.Instance.OnDrawStart += CapturePlane;
        DrawManager.Instance.OnDrawEnd += CaptureDrawing;
    }

    void CapturePlane(IMixedRealityCursor cursor, Vector3 drawPos)
    {
        currentPlane.position = cursor.Position;
        currentPlane.rotation = cursor.Rotation;

        captureCamera.transform.position = currentPlane.forward * 1.5f;;
        captureCamera.transform.LookAt(currentPlane);
    }

    void CaptureDrawing()
    {
    }
}
