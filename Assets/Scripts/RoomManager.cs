using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.IO;

public class RoomManager : MonoSingleton<RoomManager>
{
    [SerializeField]
    private GameObject roomPlanes;
    [SerializeField]
    private Camera captureCamera;
    [SerializeField]
    private Transform currentPlane;
    
    [SerializeField]
    private Mode appMode = Mode.Analyze;

    [SerializeField]
    private SpeechConfirmationTooltip tagConfirmationPrefab;

    enum Mode
    {
        Train,
        Analyze
    }

    private const string baseImageName = "Capture.jpg";
    private const string imageExtension = ".jpg";
    private string imageDir { 
        get 
        {
            var path = Path.Combine(Application.persistentDataPath, "Captures");
            if (!Directory.Exists(path)) 
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

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

        captureCamera.transform.position = currentPlane.forward * 1.5f;
        captureCamera.transform.LookAt(currentPlane);
    }

    public void CaptureDrawing()
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = captureCamera.targetTexture;
 
        captureCamera.Render();
 
        Texture2D image = new Texture2D(captureCamera.targetTexture.width, captureCamera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, captureCamera.targetTexture.width, captureCamera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;
 
        byte[] bytes = image.EncodeToJPG();
        Destroy(image);

        switch(appMode)
        {
            case Mode.Train: File.WriteAllBytes(GetUniqueFileName(), bytes);
                break;
            case Mode.Analyze: AzureCVAnalyzer.Instance.AnalyzeImage(bytes, 
                    (cvTag, probability) => VisionResult(cvTag, probability));
                break;
        }
    }

    private void VisionResult(AzureCVAnalyzer.AzureCVTag cvTag, double probability)
    {
        var showTag = Instantiate(tagConfirmationPrefab);
        showTag.SetText(cvTag.ToString() + ", with " + Math.Round(probability, 2)*100 + "%");
        showTag.TriggerConfirmedAnimation();

        switch(cvTag)
        {
            case AzureCVAnalyzer.AzureCVTag.circle:
                break;
            case AzureCVAnalyzer.AzureCVTag.square:
                break;
            case AzureCVAnalyzer.AzureCVTag.none:
                break;
        }
    }

    private string GetUniqueFileName()
    {
        string filePath = Path.Combine(imageDir, baseImageName);
        int i = 1;
        while(File.Exists(filePath))
        {
            var temp = Path.GetFileNameWithoutExtension(baseImageName);
            var newName = temp + i + imageExtension;
            filePath = Path.Combine(imageDir, newName);
            i++;
        }

        return filePath;
    }
}
