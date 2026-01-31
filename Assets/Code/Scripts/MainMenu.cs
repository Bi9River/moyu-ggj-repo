using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // 和需要加载的场景名字一致
        SceneManager.LoadScene("StartingScene");
    }

    public void QuitGame()
    {
        Debug.Log("Game exited");

        #if UNITY_EDITOR
                // 如果在 Unity 编辑器中，点击退出则停止 Play 模式
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                // 如果是打包后的游戏，则正常退出
                Application.Quit();
        #endif
    }
}