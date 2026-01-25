using System;
using UnityEngine;
using System.Net; // 方便处理 IP 字符串转换

public class NetworkNode : MonoBehaviour
{
    public string ipAddress = "192.168.1.1";
    [HideInInspector] public uint ipUint; // 转换成 uint 方便位运算
    
    private MeshRenderer meshRenderer;
    private Color originalColor;
    
    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;
        ipUint = IPToUint(ipAddress); // 确保初始化
    }
    
    void Awake()
    {
        ipUint = IPToUint(ipAddress);
    }

    // 将字符串 IP 转换为 32 位无符号整数
    public static uint IPToUint(string ipStr)
    {
        IPAddress address = IPAddress.Parse(ipStr);
        byte[] bytes = address.GetAddressBytes();
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
    
    public void SetState(bool isReachable, bool isCurrent) {
        if (isCurrent) {
            meshRenderer.material.color = Color.green; // 当前所在节点
        } else {
            // 可触达则高亮（青色），不可触达则半透明灰
            meshRenderer.material.color = isReachable ? Color.cyan : new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }
}