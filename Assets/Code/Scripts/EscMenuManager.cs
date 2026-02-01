using UnityEngine;
using UnityEngine.UI;

namespace Code.Scripts
{
    /// <summary>
    /// Esc 菜单管理器：按 Esc 显示/隐藏菜单，继续/退出游戏。
    /// 挂到场景中空物体上，将 CanvasEsc Prefab 拖到 menuPrefab；菜单实例会 DontDestroyOnLoad，跨场景可用。
    /// </summary>
    public class EscMenuManager : MonoBehaviour
    {
        public static EscMenuManager Instance { get; private set; }

        [Header("Esc Menu")]
        [Tooltip("CanvasEsc 预制体；运行时实例化并 DontDestroyOnLoad")]
        [SerializeField]
        GameObject menuPrefab;

        GameObject _menuPanel;
        Button _buttonResume;
        Button _buttonQuit;
        bool _menuVisible;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (menuPrefab == null)
            {
                Debug.LogWarning("[EscMenuManager] 未指定 menuPrefab，Esc 菜单不可用");
                return;
            }

            GameObject menuRoot = Instantiate(menuPrefab, transform);
            menuRoot.name = "CanvasEsc(Clone)";
            DontDestroyOnLoad(menuRoot);

            Transform panelT = menuRoot.transform.Find("Panel");
            if (panelT != null)
            {
                _menuPanel = panelT.gameObject;
                _buttonResume = panelT.Find("ButtonResume")?.GetComponent<Button>();
                _buttonQuit = panelT.Find("ButtonQuit")?.GetComponent<Button>();
            }
            else
            {
                Debug.LogWarning("[EscMenuManager] 未找到 Panel，使用根物体");
                _menuPanel = menuRoot;
            }

            if (_buttonResume != null)
                _buttonResume.onClick.AddListener(CloseMenu);
            else
                Debug.LogWarning("[EscMenuManager] 未找到 ButtonResume");

            if (_buttonQuit != null)
                _buttonQuit.onClick.AddListener(QuitGame);
            else
                Debug.LogWarning("[EscMenuManager] 未找到 ButtonQuit");

            _menuPanel.SetActive(false);
            _menuVisible = false;
        }

        void Update()
        {
            if (_menuPanel == null) return;
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (_menuVisible)
                CloseMenu();
            else
                OpenMenu();
        }

        /// <summary>打开 Esc 菜单：显示 Panel、禁用玩家控制。</summary>
        public void OpenMenu()
        {
            if (_menuPanel == null) return;
            _menuPanel.SetActive(true);
            _menuVisible = true;
            if (GameManager.Instance != null)
                GameManager.Instance.SetPlayerControlEnabled(false);
            Time.timeScale = 0f;
        }

        /// <summary>关闭 Esc 菜单：隐藏 Panel、恢复玩家控制。</summary>
        public void CloseMenu()
        {
            if (_menuPanel == null) return;
            _menuPanel.SetActive(false);
            _menuVisible = false;
            if (GameManager.Instance != null)
                GameManager.Instance.SetPlayerControlEnabled(true);
            Time.timeScale = 1f;
        }

        /// <summary>退出游戏（与 MainMenu 一致）。</summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
