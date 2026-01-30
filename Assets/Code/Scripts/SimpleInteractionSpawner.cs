using UnityEngine;

public class SimpleInteractionSpawner : MonoBehaviour
{
    [Header("设置")]
    public GameObject itemPrefab;    
    public Transform spawnPoint;     
    public float interactionRange = 3f; 

    private GameObject player;       
    private GameObject currentSpawnedObject; // 新增：用来存储当前场景中生成的物体

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

        if (distance <= interactionRange && Input.GetKeyDown(KeyCode.E))
        {
            TriggerSwitch();
        }
    }

    void TriggerSwitch()
    {
        if (itemPrefab != null && spawnPoint != null)
        {
            // --- 核心逻辑修改 ---
            // 1. 检查是否已经有一个物体存在，如果有，先删掉它
            if (currentSpawnedObject != null)
            {
                Destroy(currentSpawnedObject);
                Debug.Log("旧物体已清理");
            }

            // 2. 生成新物体，并将它赋值给变量记录下来
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