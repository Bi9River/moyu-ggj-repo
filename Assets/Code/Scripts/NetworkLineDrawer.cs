using UnityEngine;
using System.Collections.Generic;

public class NetworkLineDrawer : MonoBehaviour
{
    public Material lineMaterial; // 找一个发光的材质
    private List<LineRenderer> linePool = new List<LineRenderer>();
    private PlayerController player;
    private ConnectionManager connManager;

    void Start() {
        player = GetComponentInParent<PlayerController>();
        connManager = FindObjectOfType<ConnectionManager>();
    }

    void Update() {
        DrawConnections();
    }

    void DrawConnections() {
        NetworkNode[] allNodes = FindObjectsOfType<NetworkNode>();
        int lineIndex = 0;

        foreach (var node in allNodes) {
            if (node == player.currentNode) continue;

            if (connManager.CanConnect(player.currentNode.ipUint, node.ipUint)) {
                LineRenderer lr = GetOrCreateLine(lineIndex);
                lr.enabled = true;
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, node.transform.position);
                lineIndex++;
            }
        }

        // 隐藏多余的线
        for (int i = lineIndex; i < linePool.Count; i++) {
            linePool[i].enabled = false;
        }
    }

    LineRenderer GetOrCreateLine(int index) {
        if (index < linePool.Count) return linePool[index];

        GameObject lineObj = new GameObject("Line_" + index);
        lineObj.transform.SetParent(transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = lineMaterial;
        lr.positionCount = 2;
        
        linePool.Add(lr);
        return lr;
    }
}