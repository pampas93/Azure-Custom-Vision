using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class DrawManager : MonoSingleton<DrawManager>, IMixedRealityPointerHandler
{
    [SerializeField]
    private float lineWidth = 0.01f;

    [SerializeField]
    private Material defaultMaterial;

    private bool enableDraw = true;
    public bool EnableDraw
    {
        get => enableDraw;
        set => enableDraw = value;
    }

    private Vector3 prevPointDistance = Vector3.zero;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private LineRenderer currentLineRender;
    private int positionCount = 0; // 2 by default
    private Vector3 grabPosition = new Vector3();
    private float minDistanceBeforeNewPoint = 0.001f;

    void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    void OnDisable()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }

    void UpdateLine()
    {
        if(prevPointDistance == null)
        {
            prevPointDistance = grabPosition;
        }

        if(prevPointDistance != null && Mathf.Abs(Vector3.Distance(prevPointDistance, 
            grabPosition)) >= minDistanceBeforeNewPoint)
        {
            prevPointDistance = grabPosition;
            AddPoint(prevPointDistance);
        }
    }

    void AddNewLineRenderer()
    {
        positionCount = 0;
        GameObject go = new GameObject($"LineRenderer_{lines.Count}");
        go.transform.parent = transform;
        LineRenderer goLineRenderer = go.AddComponent<LineRenderer>();
        goLineRenderer.startWidth = lineWidth;
        goLineRenderer.endWidth = lineWidth;
        goLineRenderer.useWorldSpace = true;
        goLineRenderer.material = defaultMaterial;
        goLineRenderer.positionCount = 1;
        goLineRenderer.numCapVertices = 90;
        goLineRenderer.SetPosition(0, grabPosition);

        currentLineRender = goLineRenderer;
        lines.Add(goLineRenderer);
    }

    void AddPoint(Vector3 position)
    {
        currentLineRender.SetPosition(positionCount, position);
        positionCount++;
        currentLineRender.positionCount = positionCount + 1;
        currentLineRender.SetPosition(positionCount, position);
    }

    public void UndoLine()
    {
        if (lines.Count > 0)
        {
            var toDelete = lines[lines.Count-1];
            Destroy(toDelete.gameObject);
            lines.RemoveAt(lines.Count-1);
        }
    }
    

#region IMixedRealityPointerHandler
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if (!EnableDraw) return;

        if (eventData.Pointer is LinePointer pointer)
        {

            grabPosition = pointer.BaseCursor.Position;
        }

        AddNewLineRenderer();
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (!EnableDraw) return;

        if (eventData.Pointer is LinePointer pointer)
        {
            grabPosition = pointer.BaseCursor.Position;
        }
        
        UpdateLine();
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData) {}

    public void OnPointerClicked(MixedRealityPointerEventData eventData) {}
#endregion
}
