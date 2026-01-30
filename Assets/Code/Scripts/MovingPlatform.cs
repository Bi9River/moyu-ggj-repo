using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("移动设置")]
    public List<Transform> waypoints; // 坐标点列表
    public float moveSpeed = 3.0f;    // 移动速度
    public float waitTime = 0.5f;     // 到达原点后的停留时间

    private int _currentIndex = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;

    void Update()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        if (_isWaiting)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitTime)
            {
                _isWaiting = false;
                _waitTimer = 0f;
            }
            return;
        }

        MoveTowardsTarget();
    }

    void MoveTowardsTarget()
    {
        Transform target = waypoints[_currentIndex];
        // 平滑移动
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // 检查是否到达
        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            _currentIndex = (_currentIndex + 1) % waypoints.Count;
            _isWaiting = true;
        }
    }

    // --- 关键部分：载着玩家移动 ---
    
    private void OnCollisionEnter(Collision collision)
    {
        // 只有当物体从上方接触平台时，才将其设为子物体
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}