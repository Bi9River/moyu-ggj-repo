using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 当 Timeline Signal 不生效时的备用方案：根据 PlayableDirector 的时间直接调用 MaskStateManager。
/// 挂到 NpcTimeline 同一 GameObject 上，配置与 Signal 相同的时间点。
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class TimelineMaskSignalBridge : MonoBehaviour
{
    [Header("Mask 切换时间点（与 Signal 时间一致）")]
    [Tooltip("在此时间调用 SetMaskOff")]
    [SerializeField] float maskOffTime = 0f;
    [Tooltip("在此时间调用 SetMaskOn")]
    [SerializeField] float maskOnTime = 2.65f;

    [Header("容差（秒）")]
    [SerializeField] float timeTolerance = 0.1f;

    [Header("MaskStateManager（空则 FindObjectOfType）")]
    [SerializeField] MaskStateManager maskStateManager;

    PlayableDirector _director;
    bool _maskOffFired;
    bool _maskOnFired;

    void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        if (maskStateManager == null)
            maskStateManager = FindObjectOfType<MaskStateManager>();
    }

    void OnEnable()
    {
        _maskOffFired = false;
        _maskOnFired = false;
        if (_director != null)
            _director.played += OnPlayed;
    }

    void OnDisable()
    {
        if (_director != null)
            _director.played -= OnPlayed;
    }

    void OnPlayed(PlayableDirector _)
    {
        _maskOffFired = false;
        _maskOnFired = false;
    }

    void Update()
    {
        if (maskStateManager == null || _director == null || _director.state != PlayState.Playing)
            return;

        float f = (float)_director.time;

        // 已越过时间点则触发（避免首帧或跳帧遗漏）
        if (!_maskOffFired && f >= maskOffTime - timeTolerance)
        {
            _maskOffFired = true;
            maskStateManager.SetMaskOff();
            Debug.Log($"[TimelineMaskSignalBridge] t={f:F2} -> SetMaskOff");
        }
        if (!_maskOnFired && f >= maskOnTime - timeTolerance)
        {
            _maskOnFired = true;
            maskStateManager.SetMaskOn();
            Debug.Log($"[TimelineMaskSignalBridge] t={f:F2} -> SetMaskOn");
        }
    }
}
