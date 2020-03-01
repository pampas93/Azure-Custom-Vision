using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoSingleton<RoomManager>
{
    [SerializeField]
    private GameObject roomPlanes;

    void Start()
    {
#if !UNITY_EDITOR
        roomPlanes.SetActive(false);
#endif
    }
}
