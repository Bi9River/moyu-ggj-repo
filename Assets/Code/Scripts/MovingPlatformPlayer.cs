using UnityEngine;

public class MovingPlatformPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 检查进来的是不是玩家
        if (other.CompareTag("Player"))
        {
            // 将玩家的父物体设为这个平台
            other.transform.SetParent(transform);
            Debug.Log("玩家已登上平台");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 玩家离开平台时
        if (other.CompareTag("Player"))
        {
            // 解除父子关系（设为 null 表示回到场景最顶层）
            other.transform.SetParent(null);
            Debug.Log("玩家已离开平台");
        }
    }
}