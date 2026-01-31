using UnityEngine;

public class AirWallManager : MonoBehaviour
{
    public GameObject AirWallBeforePlay;
    
    public GameObject AirWallAfterPlay;

    public void StartPlay()
    {
        AirWallBeforePlay.SetActive(false);
        AirWallAfterPlay.SetActive(true);
    }
}
