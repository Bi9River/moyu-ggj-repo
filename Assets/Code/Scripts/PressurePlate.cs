using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public SimpleDoor door;
    [SerializeField] private int objectCount = 0;

    // 只要有任何物体进入，就会触发
    private void OnTriggerEnter(Collider other)
    {
        // 关键调试：不管是什么，先打印出名字
        Debug.Log("【物理检测】有东西进来了！名字是: " + other.gameObject.name);

        if (other.CompareTag("Player") || other.attachedRigidbody != null)
        {
            objectCount++;
            if (door != null) door.Open();
            Debug.Log(">>> 成功激活开关！当前物体数: " + objectCount);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("【物理检测】有东西离开了: " + other.gameObject.name);

        if (other.CompareTag("Player") || other.attachedRigidbody != null)
        {
            objectCount--;
            if (objectCount <= 0)
            {
                objectCount = 0;
                if (door != null) door.Close();
                Debug.Log(">>> 开关关闭。");
            }
        }
    }
}