using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.Scripts
{
    /// <summary>
    /// 场景过渡管理器：跨场景存在，负责切场景时的过渡 UI。
    /// 支持每个关卡配置自己的过渡 Canvas（可显示不同文字）。
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        private bool _isTransitioning;
        private GameObject _currentTransitionGO;
        private CanvasGroup _currentCanvasGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[SceneTransitionManager] 已创建单例");
        }

        /// <summary>
        /// 带过渡的加载场景（使用关卡配置的 Canvas）
        /// </summary>
        /// <param name="sceneName">目标场景名</param>
        /// <param name="transitionCanvas">过渡用 Canvas（关卡配置的，可包含文字等）</param>
        /// <param name="fadeOutTime">淡出时长（秒）</param>
        /// <param name="fadeInTime">淡入时长（秒）</param>
        /// <param name="disablePlayerDuringTransition">过渡期间是否禁用玩家控制</param>
        public void LoadSceneWithTransition(
            string sceneName,
            Canvas transitionCanvas,
            float fadeOutTime,
            float fadeInTime,
            bool disablePlayerDuringTransition = true)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneTransitionManager] sceneName 为空");
                return;
            }
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] 正在过渡中，忽略请求");
                return;
            }
            StartCoroutine(TransitionWithCanvasCoroutine(
                sceneName, transitionCanvas, fadeOutTime, fadeInTime, disablePlayerDuringTransition));
        }

        /// <summary>
        /// 带过渡的加载场景（无 Canvas，使用默认黑屏）
        /// </summary>
        public void LoadSceneWithTransition(
            string sceneName,
            float fadeOutTime = 1f,
            float fadeInTime = 0.8f,
            bool disablePlayerDuringTransition = true)
        {
            LoadSceneWithTransition(sceneName, null, fadeOutTime, fadeInTime, disablePlayerDuringTransition);
        }

        private IEnumerator TransitionWithCanvasCoroutine(
            string sceneName,
            Canvas transitionCanvas,
            float fadeOutTime,
            float fadeInTime,
            bool disablePlayerDuringTransition)
        {
            _isTransitioning = true;
            Debug.Log($"[SceneTransitionManager] 开始过渡 → {sceneName} (FadeOut:{fadeOutTime}s, FadeIn:{fadeInTime}s)");

            if (disablePlayerDuringTransition && GameManager.Instance != null)
                GameManager.Instance.SetPlayerControlEnabled(false);

            // 准备过渡 Canvas
            if (transitionCanvas != null)
            {
                _currentTransitionGO = transitionCanvas.gameObject;
                
                // 移到 SceneTransitionManager 下，确保跨场景保留
                _currentTransitionGO.transform.SetParent(transform);
                
                // 确保 Canvas 设置正确
                transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                transitionCanvas.sortingOrder = 9999;
                
                // 获取或添加 CanvasGroup
                _currentCanvasGroup = _currentTransitionGO.GetComponent<CanvasGroup>();
                if (_currentCanvasGroup == null)
                    _currentCanvasGroup = _currentTransitionGO.AddComponent<CanvasGroup>();
                
                _currentCanvasGroup.alpha = 0f;
                _currentTransitionGO.SetActive(true);
                
                Debug.Log($"[SceneTransitionManager] 使用关卡 Canvas: {_currentTransitionGO.name}");
            }
            else
            {
                // 没有配置 Canvas，创建临时黑屏
                _currentTransitionGO = CreateDefaultTransitionUI();
                _currentCanvasGroup = _currentTransitionGO.GetComponent<CanvasGroup>();
                Debug.Log("[SceneTransitionManager] 使用默认黑屏过渡");
            }

            // 淡出（显示过渡画面）
            _currentCanvasGroup.blocksRaycasts = true;
            yield return Fade(0f, 1f, fadeOutTime);

            // 异步加载场景
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
                yield return null;

            op.allowSceneActivation = true;
            while (!op.isDone)
                yield return null;

            Debug.Log($"[SceneTransitionManager] 场景 {sceneName} 已加载");

            // 等一帧让新场景初始化
            yield return null;

            // 淡入（隐藏过渡画面，显示新场景）
            yield return Fade(1f, 0f, fadeInTime);
            _currentCanvasGroup.blocksRaycasts = false;

            // 销毁过渡 Canvas
            if (_currentTransitionGO != null)
            {
                Destroy(_currentTransitionGO);
                _currentTransitionGO = null;
                _currentCanvasGroup = null;
            }

            if (disablePlayerDuringTransition && GameManager.Instance != null)
                GameManager.Instance.SetPlayerControlEnabled(true);

            _isTransitioning = false;
            Debug.Log($"[SceneTransitionManager] 过渡完成: {sceneName}");
        }

        private GameObject CreateDefaultTransitionUI()
        {
            var canvasGO = new GameObject("DefaultTransitionCanvas");
            canvasGO.transform.SetParent(transform);
            
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var canvasGroup = canvasGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            var imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(canvasGO.transform);
            var image = imageGO.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;

            var rect = imageGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            canvasGO.SetActive(true);
            return canvasGO;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (_currentCanvasGroup == null) yield break;
            
            if (duration <= 0f)
            {
                _currentCanvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _currentCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _currentCanvasGroup.alpha = to;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
