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
    private Vector3 _lastPosition;
    private readonly HashSet<Transform> _riders = new HashSet<Transform>();

    void Start()
    {
        _lastPosition = transform.position;
        EnsureTriggerCollider();
    }

    void LateUpdate()
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
            _lastPosition = transform.position;
            return;
        }

        MoveTowardsTarget();

        // 只做 position += delta，不调用 cc.Move(delta)，避免与 ThirdPersonController 叠加导致“按左更快/按右掉下去”
        Vector3 delta = transform.position - _lastPosition;
        if (delta.sqrMagnitude > 0.0001f && _riders.Count > 0)
        {
            foreach (Transform rider in _riders)
            {
                if (rider != null)
                    rider.position += delta;
            }
        }
        _lastPosition = transform.position;
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

    // --- Trigger 记录站上的人，不 SetParent；LateUpdate 里对 _riders 施加 position += delta（适配 CharacterController）---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _riders.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _riders.Remove(other.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            _riders.Add(collision.transform);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            _riders.Remove(collision.transform);
    }

    private void EnsureTriggerCollider()
    {
        foreach (var c in GetComponents<Collider>())
            if (c.isTrigger) return;

        var main = GetComponent<Collider>();
        if (main == null) return;

        var b = main.bounds;
        var tr = transform;
        var scale = tr.lossyScale;
        var size = new Vector3(
            scale.x > 0.001f ? b.size.x / scale.x : 1f,
            scale.y > 0.001f ? b.size.y / scale.y : 1f,
            scale.z > 0.001f ? b.size.z / scale.z : 1f);
        var center = tr.InverseTransformPoint(b.center);

        var box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;
        box.center = center;
    }
}