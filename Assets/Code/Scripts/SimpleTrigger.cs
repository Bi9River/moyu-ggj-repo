using UnityEngine;

public class SimpleTrigger : MonoBehaviour
{
    [Header("需要控制的物体")]
    public GameObject targetObject; // 在 Inspector 面板里把那个隐藏的物体拖进来

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入范围的是不是玩家
        if (other.CompareTag("Player"))
        {
            if (targetObject != null)
            {
                targetObject.SetActive(true); // 显示物体
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 检查离开范围的是不是玩家
        if (other.CompareTag("Player"))
        {
            if (targetObject != null)
            {
                targetObject.SetActive(false); // 隐藏物体
            }
        }
    }
}