using System;
using UnityEngine;

public class AirWallManager : MonoBehaviour
{
    public GameObject AirWallBeforePlay;
    
    public GameObject AirWallAfterPlay;

    public void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartPlay()
    {
        AirWallBeforePlay.SetActive(false);
        AirWallAfterPlay.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
