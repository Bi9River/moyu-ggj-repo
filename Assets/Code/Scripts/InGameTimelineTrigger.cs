using UnityEngine;
using UnityEngine.Playables;
using Code.Scripts;

public class InGameTimelineTrigger : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] string playerTag = "Player";

    [Header("Timeline")]
    [SerializeField] PlayableDirector timeline;
    [SerializeField] bool playOnce = true;
    [SerializeField] bool disablePlayerControlDuringTimeline = true;
    [Tooltip("是否需要 M 键解锁后才响应（若有 InputGuideManager 则检查）")]
    [SerializeField] bool requireMaskKeyUnlocked = true;

    private bool hasPlayed;
    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered && playOnce) return;
        if (!other.CompareTag(playerTag)) return;
        if (timeline == null) return;

        triggered = true;
    }

    void Update()
    {
        if (hasPlayed || !triggered || timeline == null) return;

        // 若有 InputGuideManager 且要求解锁，则检查 M 键是否已解锁
        if (requireMaskKeyUnlocked && InputGuideManager.Instance != null && !InputGuideManager.Instance.MaskKeyUnlocked)
            return;

        // 监听玩家第一次按下 M 按键
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayTimeline();
            hasPlayed = true;
        }
    }

    void PlayTimeline()
    {
        if (disablePlayerControlDuringTimeline)
        {
            EnsurePlayerControlEnabled(false);
        }

        timeline.stopped += OnTimelineStopped;
        timeline.Play();
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        timeline.stopped -= OnTimelineStopped;

        if (disablePlayerControlDuringTimeline)
        {
            EnsurePlayerControlEnabled(true);
        }
    }

    void EnsurePlayerControlEnabled(bool enabled)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerControlEnabled(enabled);
        }
        else
        {
            Debug.LogWarning("[InGameTimelineTrigger] GameManager.Instance is null");
        }
    }
}
