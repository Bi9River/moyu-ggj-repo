using UnityEngine;
using UnityEngine.UI;

namespace Code.Scripts
{
    /// <summary>
    /// 指引触发器：玩家进入特定区域时显示按键UI并解锁对应功能。
    /// 在场景中放置带 Collider(Trigger) 的空物体，挂载此脚本并配置要解锁的功能和UI。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GuideTriggerZone : MonoBehaviour
    {
        public enum GuideType
        {
            Jump,   // 解锁跳跃 (Space)
            MaskKey // 解锁 M 键
        }

        [Header("要解锁的功能")]
        [SerializeField] private GuideType guideType = GuideType.Jump;

        [Header("指引UI（可选）")]
        [Tooltip("进入区域时显示的UI对象，如按键提示。不填则只解锁功能不显示UI")]
        [SerializeField] private GameObject guideUIObject;
        [Tooltip("按键文本，如 \"Space\" 或 \"M\"。会自动填充到 Text 或 TMP_Text 组件")]
        [SerializeField] private string keyLabel = "Space";
        [Tooltip("进入后是否隐藏UI（仅触发一次）")]
        [SerializeField] private bool hideUIAfterUnlock = true;
        [Tooltip("离开区域时是否隐藏UI")]
        [SerializeField] private bool hideUIOnExit = true;

        private Collider _collider;
        public bool _hasTriggered;
        private Text _legacyText;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (!_collider.isTrigger)
            {
                _collider.isTrigger = true;
                Debug.LogWarning($"[GuideTriggerZone] {gameObject.name} 的 Collider 已自动设为 Trigger");
            }

            if (guideUIObject != null)
            {
                _legacyText = guideUIObject.GetComponentInChildren<Text>();
                guideUIObject.SetActive(false);
            }

            _hasTriggered = false;

            if (string.IsNullOrEmpty(keyLabel))
                keyLabel = guideType == GuideType.Jump ? "Space" : "M";
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var manager = InputGuideManager.Instance;
            if (manager == null) return;

            switch (guideType)
            {
                case GuideType.Jump:
                    manager.UnlockJump();
                    break;
                case GuideType.MaskKey:
                    manager.UnlockMaskKey();
                    break;
            }

            if (guideUIObject != null && !_hasTriggered)
            {
                if (_legacyText != null)
                    _legacyText.text = keyLabel;
                guideUIObject.SetActive(true);
            }

            _hasTriggered = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (guideUIObject != null && hideUIOnExit)
                guideUIObject.SetActive(false);

            if (hideUIAfterUnlock && _hasTriggered && guideUIObject != null)
                guideUIObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            var c = GetComponent<Collider>();
            if (c == null) return;
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            if (c is BoxCollider bc)
                Gizmos.DrawCube(bc.center, bc.size);
            else if (c is SphereCollider sc)
                Gizmos.DrawSphere(sc.center, sc.radius);
        }
    }
}
