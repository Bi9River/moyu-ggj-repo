using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Code.Scripts;

/// <summary>
/// 玩家进入 Trigger 后：先花一段时间让 NPC 转向玩家（期间不播红色），
/// 转向完成后红色从淡到浓，到 TeleportDelay 时传送，传送后红色从浓到淡消失。
/// </summary>
public class NpcAttack : MonoBehaviour
{
    [Header("传送")]
    [Tooltip("玩家进入后等待多久再传送（从进入 trigger 开始算，含转向+红屏填满）")]
    public float TeleportDelay = 0.0f;

    [Header("NPC 转向玩家")]
    [Tooltip("NPC 转向玩家方向的时长；这段时间内不播红色")]
    public float NpcTurnDuration = 0.5f;
    [Tooltip("要转向的物体（只绕 Y 轴转向）；不填则用本物体")]
    public Transform npcBodyToRotate;

    [Header("红色遮罩（全屏淡↔浓）")]
    [Tooltip("红色从淡到浓填满整屏的时间")]
    public float RedFillDuration = 0.75f;
    [Tooltip("传送后红色从浓到淡从屏幕消失的时间")]
    public float RedFadeDuration = 0.75f;
    [Tooltip("可选：指定用于画红屏的全屏 Image；不填则运行时自动创建")]
    public Image vignetteOverlay;

    static Image _cachedOverlay;
    static readonly int ProgressId = Shader.PropertyToID("_Progress");
    static readonly int PhaseId = Shader.PropertyToID("_Phase");

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var manager = FindObjectOfType<LevelSceneManager>();
        if (manager == null)
        {
            Debug.LogWarning("[NpcAttack] 场景中未找到 LevelSceneManager");
            return;
        }

        StartCoroutine(TeleportSequence(manager));
    }

    IEnumerator TeleportSequence(LevelSceneManager manager)
    {
        // 看向玩家期间禁止玩家输入，红屏全部消去后再恢复
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(false);

        float turnDuration = Mathf.Max(0f, NpcTurnDuration);
        float fillDuration = Mathf.Max(0.01f, RedFillDuration);
        float fadeDuration = Mathf.Max(0.01f, RedFadeDuration);

        Transform npcBody = npcBodyToRotate != null ? npcBodyToRotate : transform;
        Transform player = GetPlayerTransform();

        // 1. NPC 转向玩家（这段时间不播红色）
        if (turnDuration > 0.01f && player != null)
        {
            Quaternion startRot = npcBody.rotation;
            Vector3 toPlayer = player.position - npcBody.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer);
                for (float t = 0f; t < turnDuration; t += Time.deltaTime)
                {
                    float n = Mathf.Clamp01(t / turnDuration);
                    npcBody.rotation = Quaternion.Slerp(startRot, targetRot, n);
                    yield return null;
                }
                npcBody.rotation = Quaternion.LookRotation(toPlayer);
            }
        }

        Image overlay = vignetteOverlay != null ? vignetteOverlay : GetOrCreateVignetteOverlay();
        if (overlay == null)
        {
            float remainDurationLookAtUser = TeleportDelay - turnDuration - fillDuration;
            if (remainDurationLookAtUser > 0f) yield return new WaitForSeconds(remainDurationLookAtUser);
            manager.OnPlayerTeleport();
            if (GameManager.Instance != null) GameManager.Instance.SetPlayerControlEnabled(true);
            yield break;
        }

        Material mat = GetOrCreateVignetteMaterial(overlay);
        if (mat == null)
        {
            float remainDurationCreateVignette = TeleportDelay - turnDuration - fillDuration;
            if (remainDurationCreateVignette > 0f) yield return new WaitForSeconds(remainDurationCreateVignette);
            manager.OnPlayerTeleport();
            if (GameManager.Instance != null) GameManager.Instance.SetPlayerControlEnabled(true);
            yield break;
        }

        // 2. 全屏红色从淡到浓
        mat.SetFloat(PhaseId, 0f);
        for (float t = 0f; t < fillDuration; t += Time.deltaTime)
        {
            mat.SetFloat(ProgressId, Mathf.Clamp01(t / fillDuration));
            yield return null;
        }
        mat.SetFloat(ProgressId, 1f);

        // 3. 等到 TeleportDelay 再传送
        float remain = TeleportDelay - turnDuration - fillDuration;
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        manager.OnPlayerTeleport();

        // 4. 全屏红色从浓到淡消失
        mat.SetFloat(PhaseId, 1f);
        mat.SetFloat(ProgressId, 0f);
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            mat.SetFloat(ProgressId, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        mat.SetFloat(ProgressId, 1f);

        // 红屏全部消去后再恢复玩家输入
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(true);
    }

    static Transform GetPlayerTransform()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
            return GameManager.Instance.CurrentPlayer.transform;
        var go = GameObject.FindWithTag("Player");
        return go != null ? go.transform : null;
    }

    static Image GetOrCreateVignetteOverlay()
    {
        if (_cachedOverlay != null) return _cachedOverlay;

        Shader shader = Shader.Find("Custom/UI Red Vignette");
        if (shader == null) return null;

        var go = new GameObject("RedVignetteOverlay");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.AddComponent<GraphicRaycaster>();

        var imageGo = new GameObject("Image");
        imageGo.transform.SetParent(go.transform, false);
        var rect = imageGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imageGo.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
        image.material = new Material(shader);

        _cachedOverlay = image;
        _cachedOverlay.material.SetFloat(ProgressId, 0f);
        _cachedOverlay.material.SetFloat(PhaseId, 0f);

        return _cachedOverlay;
    }

    static Material GetOrCreateVignetteMaterial(Image overlay)
    {
        if (overlay.material != null && overlay.material.shader.name == "Custom/UI Red Vignette")
            return overlay.material;
        Shader shader = Shader.Find("Custom/UI Red Vignette");
        if (shader == null) return null;
        var mat = new Material(shader);
        mat.SetFloat(ProgressId, 0f);
        mat.SetFloat(PhaseId, 0f);
        overlay.material = mat;
        return mat;
    }
}
