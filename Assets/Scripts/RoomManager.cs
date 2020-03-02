using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.IO;
using System.Collections.Generic;

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

    [SerializeField]
    private TagObject[] tagObjects;

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

    private Dictionary<AzureCVAnalyzer.AzureCVTag, GameObject> tagObjDict;
    private List<GameObject> magicObjects = new List<GameObject>();

    void Start()
    {
#if !UNITY_EDITOR
        roomPlanes.SetActive(false);
#endif

        DrawManager.Instance.OnDrawStart += CapturePlane;
        DrawManager.Instance.OnDrawEnd += CaptureDrawing;

        tagObjDict = new Dictionary<AzureCVAnalyzer.AzureCVTag, GameObject>();
        foreach (var pair in tagObjects)
        {
            tagObjDict.Add(pair.Tag, pair.Prefab);
        }
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
            case AzureCVAnalyzer.AzureCVTag.none:
                break;
            default: CreateMagic(tagObjDict[cvTag]);
                break;
        }
    }

    private void CreateMagic(GameObject prefab)
    {
        var initialPos = DrawManager.Instance.InitialPos;
        var farthestPos = DrawManager.Instance.FarthestPoint;
        var midpoint = Vector3.Lerp(initialPos, farthestPos, 0.5f);
        var size = Vector3.Distance(initialPos, farthestPos);

        var obj = Instantiate(prefab, midpoint, currentPlane.rotation);
        obj.transform.localScale = new Vector3(size, size, size);

        magicObjects.Add(obj);

        DrawManager.Instance.ClearDrawings();
    }

    public void DeleteMagicObjects()
    {
        foreach (var obj in magicObjects)
        {
            Destroy(obj);
        }
        magicObjects.Clear();
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

[Serializable]
public class TagObject
{
    [SerializeField]
    private AzureCVAnalyzer.AzureCVTag tag;
    [SerializeField]
    private GameObject prefab;

    public AzureCVAnalyzer.AzureCVTag Tag { get => tag; }
    public GameObject Prefab { get => prefab; }
}
