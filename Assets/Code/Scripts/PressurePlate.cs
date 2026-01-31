using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public SimpleDoor door;
    [SerializeField] private int objectCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        // 打印进入物体的名字，方便你调试
        Debug.Log("检测到物体进入: " + other.gameObject.name);

        // 修改判断逻辑：如果是玩家 OR 标签是 GravityBox
        if (other.CompareTag("Player") || other.CompareTag("GravityBox"))
        {
            objectCount++;
            if (door != null) door.Open();
            Debug.Log(">>> 激活！当前有效物体数: " + objectCount);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("检测到物体离开: " + other.gameObject.name);

        if (other.CompareTag("Player") || other.CompareTag("GravityBox"))
        {
            objectCount--;
            if (objectCount <= 0)
            {
                objectCount = 0;
                if (door != null) door.Close();
                Debug.Log(">>> 所有物体离开，开关关闭。");
            }
        }
    }
}