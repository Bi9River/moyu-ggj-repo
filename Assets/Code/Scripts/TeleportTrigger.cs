using System;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Header("Teleport Target Point")]
    public Transform targetPoint;

    public void OnTriggerExit(Collider other)
    {
        UpdateTelepointTransform(other);
    }
    public void OnTriggerEnter(Collider other)
    {
        UpdateTelepointTransform(other);
    }

    private void UpdateTelepointTransform(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[TeleportTrigger] 玩家进入 " + this.name + " 区域");
        
        var manager = FindObjectOfType<LevelSceneManager>();
        if (manager != null)
            manager.UpdateCurrentTelepoint(targetPoint);
        else
            Debug.LogWarning("[LevelEndTrigger] 场景中未找到 LevelSceneManager");
    }
}
