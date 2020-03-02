using UnityEngine;

public class HandMenu : MonoSingleton<HandMenu>
{
    public void OnUndoClick()
    {
        DrawManager.Instance.UndoLine();
    }

    public void OnCaptureClick()
    {
        return;
        RoomManager.Instance.CaptureDrawing();
        DrawManager.Instance.ClearDrawings();
    }

    public void ClearObjects()
    {
        RoomManager.Instance.DeleteMagicObjects();
    }
}