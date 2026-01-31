using UnityEngine;
using UnityEngine.EventSystems; // 必须引用事件系统

public class UIHoverSwitcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 引用")]
    public GameObject normalImage;   // 对应 Image 物体
    public GameObject hoverImage;    // 对应 Image Onhover 物体

    void Start()
    {
        // 初始状态：显示普通图，隐藏悬停图
        ShowNormal();
    }

    // 当鼠标移入时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        normalImage.SetActive(false);
        hoverImage.SetActive(true);
    }

    // 当鼠标移出时触发
    public void OnPointerExit(PointerEventData eventData)
    {
        ShowNormal();
    }

    private void ShowNormal()
    {
        normalImage.SetActive(true);
        hoverImage.SetActive(false);
    }

    // 确保点击开始后，如果 UI 没消失，也能恢复状态
    void OnDisable()
    {
        ShowNormal();
    }
}