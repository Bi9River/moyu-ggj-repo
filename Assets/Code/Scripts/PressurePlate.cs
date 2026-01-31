using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public SimpleDoor door;
    
    [Header("新增功能：指定激活物体")]
    [Tooltip("当压力板激活时，该物体会被 SetActive(true)")]
    public GameObject targetObject; 

    [SerializeField] private int objectCount = 0; // 原代码保留

    private bool _isPlayerOnPlate = false;
    private bool _isGravityBoxOnPlate = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("检测到物体进入: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            _isPlayerOnPlate = true;
            UpdateStatus(true);
        }
        else if (other.CompareTag("GravityBox"))
        {
            _isGravityBoxOnPlate = true;
            UpdateStatus(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("检测到物体离开: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            _isPlayerOnPlate = false;
        }
        else if (other.CompareTag("GravityBox"))
        {
            _isGravityBoxOnPlate = false;
        }

        // 只有当玩家和箱子都不在上面时，才关闭
        if (!_isPlayerOnPlate && !_isGravityBoxOnPlate)
        {
            UpdateStatus(false);
        }
    }

    /// <summary>
    /// 统一处理门和物体的状态切换
    /// </summary>
    private void UpdateStatus(bool isActive)
    {
        if (isActive)
        {
            // 执行开启逻辑
            if (door != null) door.Open();
            if (targetObject != null) targetObject.SetActive(true);
            
            Debug.Log("压力板激活：门已开启，物体已显示");
        }
        else
        {
            // 执行关闭逻辑
            if (door != null) door.Close();
            if (targetObject != null) targetObject.SetActive(false);
            
            Debug.Log("压力板重置：门已关闭，物体已隐藏");
        }
    }
}