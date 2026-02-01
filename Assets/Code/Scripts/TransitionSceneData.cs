namespace Code.Scripts
{
    /// <summary>
    /// 静态数据传递：在加载过渡场景前设置，过渡场景读取后用于显示对应文字。
    /// </summary>
    public static class TransitionSceneData
    {
        /// <summary>加载完过渡后要去的目标场景名</summary>
        public static string NextSceneName { get; set; }

        /// <summary>文字映射的 Key，用于在过渡场景中显示对应 UI</summary>
        public static string TextKey { get; set; }

        public static void Set(string nextScene, string textKey = null)
        {
            NextSceneName = nextScene;
            TextKey = string.IsNullOrEmpty(textKey) ? nextScene : textKey;
        }

        public static void Clear()
        {
            NextSceneName = null;
            TextKey = null;
        }
    }
}
