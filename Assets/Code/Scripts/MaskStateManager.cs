using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MaskStateManager : MonoBehaviour
{
    public enum MaskState
    {
        On,
        Off
    }

    public MaskState maskState = MaskState.On;
    
    [Header("Mask Visual Settings")]
    public Color maskColor = new Color(0.1f, 0.1f, 0.1f, 0.8f); // 遮罩颜色（深色半透明）
    public float maskHeightRatio = 0.5f; // 遮罩覆盖屏幕高度的比例（0.5 = 下半部分）
    public Vector2 leftEyePosition = new Vector2(0.3f, 0.7f); // 左眼位置（屏幕空间，0-1范围）
    public Vector2 rightEyePosition = new Vector2(0.7f, 0.7f); // 右眼位置
    public float eyeHoleRadius = 0.08f; // 眼睛孔的半径（屏幕空间比例）
    public float eyeHoleSmoothness = 0.02f; // 眼睛孔边缘平滑度
    
    [Header("Blink Effect Settings (Future Use)")]
    [Range(0f, 1f)]
    public float leftEyeBlink = 0f; // 左眼眨眼程度（0=睁开，1=完全闭合）
    [Range(0f, 1f)]
    public float rightEyeBlink = 0f; // 右眼眨眼程度
    public float blinkSpeed = 5f; // 眨眼动画速度
    
    private ArrayList _inactiveObjects;
    private GameObject _maskCanvas;
    private GameObject _maskOverlay;
    private Material _maskMaterial;

    public void Awake()
    {
        _inactiveObjects = new ArrayList();
        CreateMaskOverlay();
        RefreshSceneByMaskStatus();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMask();
        }
        
        // 实时更新Shader参数（支持在运行时修改Inspector参数）
        if (_maskMaterial != null && _maskOverlay != null && _maskOverlay.activeSelf)
        {
            UpdateShaderParameters();
        }
    }

    private void ToggleMask()
    {
        maskState = maskState == MaskState.On ? MaskState.Off : MaskState.On;
        Debug.Log($"Mask state changed to {maskState}");
        RefreshSceneByMaskStatus();
    }

    private void RefreshSceneByMaskStatus()
    {
        var objectsWithMaskOn = GameObject.FindGameObjectsWithTag("SeenWithMaskOn");
        var objectsWithMaskOff = GameObject.FindGameObjectsWithTag("SeenWithMaskOff");
        
        // 先重新激活之前被禁用的对象
        foreach (var obj in _inactiveObjects)
        {
            ((GameObject)obj).SetActive(true);
            Debug.Log($"Object {((GameObject)obj).name} is now active");
        }
        
        // 然后清空列表
        _inactiveObjects.Clear();

        if (maskState == MaskState.On)
        {
            foreach (var obj in objectsWithMaskOff)
            {
                obj.SetActive(false);
                _inactiveObjects.Add(obj);
                Debug.Log($"Object {obj.name} is now inactive");
            }
        }
        else
        {
            foreach (var obj in objectsWithMaskOn)
            {
                obj.SetActive(false);
                _inactiveObjects.Add(obj);
                Debug.Log($"Object {obj.name} is now inactive");
            }
        }
        
        // 更新对象颜色
        UpdateObjectColors();
        
        // 更新遮罩显示
        UpdateMaskOverlay();
    }
    
    private void UpdateObjectColors()
    {
        var objectsWithMaskOn = GameObject.FindGameObjectsWithTag("SeenWithMaskOn");
        var objectsWithMaskOff = GameObject.FindGameObjectsWithTag("SeenWithMaskOff");
        
        // 更新 SeenWithMaskOn 标签的对象颜色（仅当它们处于活动状态时）
        foreach (var obj in objectsWithMaskOn)
        {
            if (obj.activeSelf)
            {
                SetObjectColor(obj, Color.blue);
            }
        }
        
        // 更新 SeenWithMaskOff 标签的对象颜色
        foreach (var obj in objectsWithMaskOff)
        {
            if (obj.activeSelf)
            {
                SetObjectColor(obj, Color.yellow);
            }
        }
    }
    
    private void SetObjectColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 创建材质实例以避免影响其他使用相同材质的对象
            Material material = renderer.material;
            material.color = color;
            Debug.Log($"Object {obj.name} color set to {color}");
        }
        else
        {
            Debug.LogWarning($"Object {obj.name} does not have a Renderer component");
        }
    }
    
    private void CreateMaskOverlay()
    {
        // 查找或创建Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // 创建新的Canvas
            GameObject canvasObj = new GameObject("MaskCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 确保遮罩在最上层
            
            // 添加CanvasScaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            // 添加GraphicRaycaster（可选，如果需要交互）
            canvasObj.AddComponent<GraphicRaycaster>();
            
            _maskCanvas = canvasObj;
        }
        else
        {
            _maskCanvas = canvas.gameObject;
        }
        
        // 创建遮罩覆盖层（使用Shader）
        _maskOverlay = new GameObject("MaskOverlay");
        _maskOverlay.transform.SetParent(_maskCanvas.transform, false);
        
        RectTransform overlayRect = _maskOverlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.anchoredPosition = Vector2.zero;
        
        // 创建Image组件并使用自定义Shader材质
        Image maskImage = _maskOverlay.AddComponent<Image>();
        
        // 加载或创建Shader材质
        Shader maskShader = Shader.Find("Custom/MaskOverlay");
        if (maskShader == null)
        {
            Debug.LogError("MaskOverlay shader not found! Please make sure the shader is in the project.");
            return;
        }
        
        _maskMaterial = new Material(maskShader);
        maskImage.material = _maskMaterial;
        maskImage.color = Color.white; // Shader会处理颜色
        
        // 更新Shader参数
        UpdateShaderParameters();
        
        // 初始状态
        UpdateMaskOverlay();
    }
    
    private void UpdateShaderParameters()
    {
        if (_maskMaterial == null) return;
        
        // 设置遮罩参数
        _maskMaterial.SetColor("_MaskColor", maskColor);
        _maskMaterial.SetFloat("_MaskHeightRatio", maskHeightRatio);
        _maskMaterial.SetVector("_LeftEyePos", leftEyePosition);
        _maskMaterial.SetVector("_RightEyePos", rightEyePosition);
        _maskMaterial.SetFloat("_EyeHoleRadius", eyeHoleRadius);
        _maskMaterial.SetFloat("_EyeHoleSmoothness", eyeHoleSmoothness);
        
        // 设置眨眼参数（为未来扩展预留）
        _maskMaterial.SetFloat("_LeftEyeBlink", leftEyeBlink);
        _maskMaterial.SetFloat("_RightEyeBlink", rightEyeBlink);
        _maskMaterial.SetFloat("_BlinkSpeed", blinkSpeed);
    }
    
    private void UpdateMaskOverlay()
    {
        if (_maskOverlay == null) return;
        
        // 根据maskState显示或隐藏遮罩
        bool shouldShowMask = maskState == MaskState.On;
        _maskOverlay.SetActive(shouldShowMask);
        
        // 更新Shader参数（以防在Inspector中修改了参数）
        if (shouldShowMask)
        {
            UpdateShaderParameters();
        }
    }
}