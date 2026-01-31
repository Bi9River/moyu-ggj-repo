using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("controlled items")]
    public GameObject menuPanel;
    public MonoBehaviour ThirdPersonController; 

    void Start()
    {
        // 游戏启动时确保 UI 显示，玩家不动
        menuPanel.SetActive(true);
        ThirdPersonController.enabled = false;
    }

    public void PlayGame()
    {
        // 1. 隐藏 UI
        menuPanel.SetActive(false);
        Debug.Log("Game start");
    }

    public void QuitGame()
    {
        Debug.Log("Game exited");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
            }
}