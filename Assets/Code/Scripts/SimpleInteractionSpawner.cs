using UnityEngine;

public class SimpleInteractionSpawner : MonoBehaviour
{
    [Header("原有设置")]
    public GameObject itemPrefab;    
    public Transform spawnPoint;     
    public float interactionRange = 3f; 

    [Header("感应显示的物体列表")]
    [Tooltip("玩家进入范围显示，离开范围隐藏")]
    public GameObject[] objectsToToggle; 

    private GameObject player;       
    private GameObject currentSpawnedObject; 

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("提示：场景里没找到 Tag 为 'Player' 的物体！");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);

        // --- 核心逻辑：范围感应 ---
        if (distance <= interactionRange)
        {
            // 在范围内：显示物体
            SetObjectsActiveState(true);

            // 原有逻辑：按 E 生成
            if (Input.GetKeyDown(KeyCode.E))
            {
                TriggerSwitch();
            }
        }
        else
        {
            // 在范围外：隐藏物体
            SetObjectsActiveState(false);
        }
    }

    // 统一管理物体状态的方法
    void SetObjectsActiveState(bool shouldBeActive)
    {
        if (objectsToToggle == null) return;

        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null && obj.activeSelf != shouldBeActive)
            {
                obj.SetActive(shouldBeActive);
                // 只有状态改变时才打印，避免刷屏
                // Debug.Log($"{obj.name} 状态已更新为: {shouldBeActive}");
            }
        }
    }

    void TriggerSwitch()
    {
        if (itemPrefab != null && spawnPoint != null)
        {
            if (currentSpawnedObject != null)
            {
                Destroy(currentSpawnedObject);
            }

            currentSpawnedObject = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("开关触发：新物体已生成！");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}