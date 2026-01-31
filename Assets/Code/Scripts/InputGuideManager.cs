using UnityEngine;

namespace Code.Scripts
{
    /// <summary>
    /// 指引功能管理器：控制跳跃(Space)和M键在引导前的解锁状态。
    /// 玩家在到达特定指引区域前，这些按键会被屏蔽。
    /// </summary>
    public class InputGuideManager : MonoBehaviour
    {
        public static InputGuideManager Instance { get; private set; }

        [Header("初始解锁状态（引导前默认均为 false）")]
        [SerializeField] private bool jumpUnlockedAtStart;
        [SerializeField] private bool maskKeyUnlockedAtStart;

        /// <summary>跳跃(Space)是否已解锁</summary>
        public bool JumpUnlocked { get; private set; }

        /// <summary>M键（面具切换）是否已解锁</summary>
        public bool MaskKeyUnlocked { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            JumpUnlocked = jumpUnlockedAtStart;
            MaskKeyUnlocked = maskKeyUnlockedAtStart;

            Debug.Log($"[InputGuideManager] 初始化 - 跳跃:{JumpUnlocked} M键:{MaskKeyUnlocked}");
        }

        /// <summary>解锁跳跃</summary>
        public void UnlockJump()
        {
            if (!JumpUnlocked)
            {
                JumpUnlocked = true;
                Debug.Log("[InputGuideManager] 已解锁跳跃 (Space)");
            }
        }

        /// <summary>解锁M键</summary>
        public void UnlockMaskKey()
        {
            if (!MaskKeyUnlocked)
            {
                MaskKeyUnlocked = true;
                Debug.Log("[InputGuideManager] 已解锁 M 键");
            }
        }

        /// <summary>重置为未解锁（例如新游戏）</summary>
        public void ResetAll()
        {
            JumpUnlocked = jumpUnlockedAtStart;
            MaskKeyUnlocked = maskKeyUnlockedAtStart;
            Debug.Log("[InputGuideManager] 已重置解锁状态");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
