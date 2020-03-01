using UnityEngine;

public class HandMenu : MonoSingleton<HandMenu>
{
    public void OnUndoClick()
    {
        DrawManager.Instance.UndoLine();
    }
}