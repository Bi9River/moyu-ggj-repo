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

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered && playOnce) return;
        if (!other.CompareTag(playerTag)) return;
        if (timeline == null) return;

        triggered = true;
        PlayTimeline();
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
