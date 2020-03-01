using UnityEngine;

public class HandMenu : MonoSingleton<HandMenu>
{
    public void OnUndoClick()
    {
        DrawManager.Instance.UndoLine();
    }

    public void OnCaptureClick()
    {
        RoomManager.Instance.CaptureDrawing();
        DrawManager.Instance.ClearDrawings();
    }
}