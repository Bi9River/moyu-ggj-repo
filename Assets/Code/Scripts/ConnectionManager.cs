using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [Header("Network Settings")]
    public int maskBits = 24; 
    
    private NetworkNode[] allNodes;
    private PlayerController player;
    private TreeNetworkGenerator treeGenerator;

    void Start() 
    {
        // 1. 获取生成器并生成树状网络
        treeGenerator = GetComponent<TreeNetworkGenerator>();
        if (treeGenerator != null)
        {
            treeGenerator.GenerateTree();
            // 直接从生成器获取生成的节点列表，比 FindObjectsOfType 更快
            allNodes = treeGenerator.allGeneratedNodes.ToArray();
        }

        // 2. 获取 Player 引用 (注意：这里不要加 PlayerController 类型前缀，否则会变成局部变量)
        player = FindObjectOfType<PlayerController>();

        // 3. 初始化玩家位置
        if (player != null && treeGenerator != null && treeGenerator.rootNode != null) 
        {
            player.currentNode = treeGenerator.rootNode;
            Vector3 rootPos = treeGenerator.rootNode.transform.position + Vector3.up * 1.5f;
            
            // 强制瞬移
            player.transform.position = rootPos;
            
            // 别忘了告诉 Player 它的移动目标点现在也是这里，防止它往回跑
            // 如果你的 PlayerController 里有这个变量，请取消注释
            // player.targetMovePosition = rootPos; 
        }

        Debug.Log($"Network initialized. Total nodes: {allNodes?.Length}, Mask: /{maskBits}");
    }
    
    void Update() 
    {
        // 快捷键调整掩码
        if (Input.GetKeyDown(KeyCode.D))
        {
            maskBits = Mathf.Clamp(maskBits + 1, 0, 32);
            Debug.Log("Subnet Mask increased: /" + maskBits);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            maskBits = Mathf.Clamp(maskBits - 1, 0, 32);
            Debug.Log("Subnet Mask decreased: /" + maskBits);
        }
        
        // 每一帧根据当前掩码刷新节点颜色/状态
        RefreshNodes();
    }
    
    void RefreshNodes() 
    {
        if (player == null || player.currentNode == null || allNodes == null) return;

        foreach (var node in allNodes) 
        {
            // 计算是否连通
            bool reachable = CanConnect(player.currentNode.ipUint, node.ipUint);
            // 调用 NetworkNode 里的变色/状态切换逻辑
            node.SetState(reachable, node == player.currentNode);
        }
    }
    
    // 更加硬核且准确的掩码计算方式
    public uint GetMaskUint()
    {
        if (maskBits == 0) return 0;
        if (maskBits == 32) return 0xFFFFFFFF;
        
        // 使用左移操作生成掩码 (例如 /24 生成 255.255.255.0)
        return 0xFFFFFFFF << (32 - maskBits);
    }

    public bool CanConnect(uint ipA, uint ipB)
    {
        uint mask = GetMaskUint();
        // 位运算判定：两个 IP 在掩码范围内是否完全一致
        return (ipA & mask) == (ipB & mask);
    }
}