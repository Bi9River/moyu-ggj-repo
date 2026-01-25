using UnityEngine;
using System.Collections.Generic;

public class TreeNetworkGenerator : MonoBehaviour
{
    public GameObject nodePrefab;

    [Header("Tree Default Topology")]
    // 默认 3 层：Root(10.x.x.x) -> Subnet(10.1.x.x) -> Host(10.1.1.x)
    public int maxDepth = 3;        
    
    // 每个节点默认分出 3 个叉，总共会有 1 + 3 + 9 = 13 个节点
    public int branchingFactor = 3; 
    
    // 层级间距设为 12，给连线留出视觉空间
    public float levelDistance = 12f; 
    
    // 扇形展开 120 度，看起来像一棵树而不是一捆柴
    public float spreadAngle = 120f;   

    [HideInInspector] public NetworkNode rootNode;
    [HideInInspector] public List<NetworkNode> allGeneratedNodes = new List<NetworkNode>();

    public void GenerateTree()
    {
        allGeneratedNodes.Clear();
        // 根节点强制生成在原点，IP 默认为 A 类私网地址
        rootNode = SpawnNode(Vector3.zero, "10.0.0.1", 0);
        
        // 开始递归：从根节点出发，向前方(Forward)生长
        GrowBranch(rootNode, 1, Vector3.forward);
    }

    void GrowBranch(NetworkNode parent, int depth, Vector3 direction)
    {
        if (depth >= maxDepth) return;

        for (int i = 0; i < branchingFactor; i++)
        {
            // 扇形展开计算
            float angle = (i - (branchingFactor - 1) / 2f) * (spreadAngle / (branchingFactor - 1));
            Vector3 branchDir = Quaternion.Euler(0, angle, 0) * direction;
            Vector3 spawnPos = parent.transform.position + branchDir * levelDistance;

            // IP 分配逻辑：根据深度修改对应的段
            string[] ipParts = parent.ipAddress.Split('.');
            ipParts[depth] = (i + 1).ToString();
            string newIP = string.Join(".", ipParts);

            NetworkNode childNode = SpawnNode(spawnPos, newIP, depth);
            
            // 递归向下生长
            GrowBranch(childNode, depth + 1, branchDir);
        }
    }

    NetworkNode SpawnNode(Vector3 pos, string ip, int depth)
    {
        GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
        go.name = $"Node_{ip}";
        NetworkNode node = go.GetComponent<NetworkNode>();
        node.ipAddress = ip;
        node.ipUint = NetworkNode.IPToUint(ip);
        
        // 视觉提示：越深层越小，体现层级感
        go.transform.localScale = Vector3.one * (1.2f - depth * 0.2f);
        
        allGeneratedNodes.Add(node);
        return node;
    }
}