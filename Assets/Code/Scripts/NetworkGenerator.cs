using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SubnetConfig {
    public string subnetPrefix; // 例如 "192.168.1"
    public int nodeCount;       // 该网段内的节点数量
    public Vector3 clusterCenter; // 该簇在 3D 空间中心
    public float radius;        // 散布半径
}

public class NetworkGenerator : MonoBehaviour {
    public GameObject nodePrefab;
    public List<SubnetConfig> subnetConfigs;

    [HideInInspector] public List<NetworkNode> generatedNodes = new List<NetworkNode>();

    public void GenerateNetwork() {
        foreach (var config in subnetConfigs) {
            for (int i = 0; i < config.nodeCount; i++) {
                // 在球形范围内随机找个位置
                Vector3 randomPos = config.clusterCenter + Random.insideUnitSphere * config.radius;
                // 强制 Y 轴对齐，如果你想做平面的话；或者保持 3D 散布
                randomPos.y = 0; 

                GameObject go = Instantiate(nodePrefab, randomPos, Quaternion.identity, transform);
                NetworkNode node = go.GetComponent<NetworkNode>();
                
                // 自动生成 IP: 前缀 + 循环 index (1-254)
                string generatedIP = $"{config.subnetPrefix}.{i + 1}";
                node.ipAddress = generatedIP;
                
                // 强制初始化节点的 uint 值（因为它是动态生成的）
                node.ipUint = NetworkNode.IPToUint(generatedIP);
                
                generatedNodes.Add(node);
            }
        }
        Debug.Log($"Network generated with {generatedNodes.Count} nodes.");
    }
}