using System.Collections.Generic;
using UnityEngine;

public class MaskStateManager : MonoBehaviour
{
    public enum MaskState
    {
        On,
        Off
    }

    public MaskState maskState = MaskState.On;

    [Header("戴上面具应当激活和关闭的物体列表")]
    
    public List<GameObject> EnabledObjectsWhenMaskOn;
    public List<GameObject> DisabledObjectsWhenMaskOn;

    [Header("戴上面具应当切换材质的物体列表")]
    
    public List<GameObject> MaterialSwitchingObjectsWhenMaskOn;

    [Header("材质（仅用于戴面具时可见的物体，Mask On/Off下不同材质）")]
    public Material materialForMaskOn;
    public Material materialForMaskOff;

    /// <summary> 戴面具时可见的物体（Tag: SeenWithMaskOn）。假定 Awake 时均为 active，仅在此刻收集一次。 </summary>
    private List<GameObject> _seenWithMaskOnList = new List<GameObject>();

    /// <summary> 不戴面具时可见的物体（Tag: SeenWithMaskOff）。假定 Awake 时均为 active，仅在此刻收集一次。 </summary>
    private List<GameObject> _seenWithMaskOffList = new List<GameObject>();

    /// <summary> 当前被隐藏的物体，切换状态时先恢复再根据新状态隐藏另一批。 </summary>
    private List<GameObject> _inactiveObjects = new List<GameObject>();

    /// <summary> 参与材质切换的物体在首次切换前的原材质（Renderer -> 原材质），用于 materialForMaskOn/Off 为 null 时恢复。 </summary>
    private Dictionary<Renderer, Material> _originalMaterials = new Dictionary<Renderer, Material>();

    public void Awake()
    {
        RefreshTaggedObjectLists();
        CaptureOriginalMaterials();
        ApplyCurrentMaskState();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMask();
        }
    }

    public void SetMaskState(MaskState state)
    {
        if (maskState == state) return;
        maskState = state;
        Debug.Log($"Mask state set to {maskState}");
        ApplyCurrentMaskState();
    }

    public void SetMaskOn() => SetMaskState(MaskState.On);
    public void SetMaskOff() => SetMaskState(MaskState.Off);

    private void ToggleMask()
    {
        SetMaskState(maskState == MaskState.On ? MaskState.Off : MaskState.On);
    }

    private void ApplyCurrentMaskState()
    {
        // 先恢复上一状态被隐藏的物体（假定策划初始全 active，仅在此处会 SetActive(false)）
        foreach (var obj in _inactiveObjects)
        {
            obj.SetActive(true);
            Debug.Log($"Object {obj.name} is now active");
        }
        _inactiveObjects.Clear();

        if (maskState == MaskState.On)
            OnSetMaskActive();
        else
            OnSetMaskDisactive();
    }

    /// <summary>
    /// 仅在 Awake 时调用一次；假定此时所有相关物体均为 active，可被 FindGameObjectsWithTag 找到。
    /// </summary>
    private void RefreshTaggedObjectLists()
    {
        _seenWithMaskOnList.Clear();
        _seenWithMaskOffList.Clear();

        foreach (var go in GameObject.FindGameObjectsWithTag("SeenWithMaskOn"))
            _seenWithMaskOnList.Add(go);
        foreach (var go in GameObject.FindGameObjectsWithTag("SeenWithMaskOff"))
            _seenWithMaskOffList.Add(go);
    }

    /// <summary>
    /// 戴上面具时调用。只负责调用各小函数，后续扩展在此加一行即可。
    /// </summary>
    private void OnSetMaskActive()
    {
        ShowAndHideList(EnabledObjectsWhenMaskOn, DisabledObjectsWhenMaskOn);
        // ApplyColorsForMaskOn();
        ApplyMaterialForMaskOn();
        // SetColliderStatusForMaskOn();
    }

    /// <summary>
    /// 摘下面具时调用。只负责调用各小函数，后续扩展在此加一行即可。
    /// </summary>
    private void OnSetMaskDisactive()
    {
        ShowAndHideList(DisabledObjectsWhenMaskOn, EnabledObjectsWhenMaskOn);
        // ApplyColorsForMaskOff();
        ApplyMaterialForMaskOff();
        // SetColliderStatusForMaskOff();
    }

    private void ShowAndHideList(List<GameObject> showList, List<GameObject> hideList)
    {
        ShowList(showList);
        HideListAndTrack(hideList);
    }

    private void ShowList(List<GameObject> list)
    {
        foreach (var obj in list)
        {
            obj.SetActive(true);
            Debug.Log($"Object {obj.name} is now active");
        }
    }

    private void HideListAndTrack(List<GameObject> list)
    {
        foreach (var obj in list)
        {
            obj.SetActive(false);
            _inactiveObjects.Add(obj);
            Debug.Log($"Object {obj.name} is now inactive");
        }
    }

    // ----- 材质 -----

    /// <summary> 在首次切换材质前，记录 MaterialSwitchingObjectsWhenMaskOn 中每个物体当前的原材质。 </summary>
    private void CaptureOriginalMaterials()
    {
        if (MaterialSwitchingObjectsWhenMaskOn == null) return;
        foreach (var obj in MaterialSwitchingObjectsWhenMaskOn)
        {
            if (obj == null) continue;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null && !_originalMaterials.ContainsKey(renderer))
            {
                _originalMaterials[renderer] = renderer.sharedMaterial;
            }
        }
    }

    /// <summary> 戴面具时：对「戴面具可见」的物体叠加 materialForMaskOn（原材质 + 面具材质）；若为 null 则恢复原材质。 </summary>
    private void ApplyMaterialForMaskOn()
    {
        foreach (var obj in MaterialSwitchingObjectsWhenMaskOn)
        {
            if (obj == null || !obj.activeSelf) continue;
            if (materialForMaskOn == null)
                RestoreObjectMaterial(obj);
            else
                SetObjectMaterialOverlay(obj, materialForMaskOn);
        }
    }

    /// <summary> 不戴面具时：对「戴面具可见」的物体叠加 materialForMaskOff（原材质 + 面具材质）；若为 null 则恢复原材质。 </summary>
    private void ApplyMaterialForMaskOff()
    {
        foreach (var obj in MaterialSwitchingObjectsWhenMaskOn)
        {
            if (obj == null || !obj.activeSelf) continue;
            if (materialForMaskOff == null)
                RestoreObjectMaterial(obj);
            else
                SetObjectMaterialOverlay(obj, materialForMaskOff);
        }
    }

    /// <summary> 在原材质之上叠加一层材质（renderer.materials = [ 原材质, 叠加材质 ]），需叠加材质使用 Alpha 混合才能看到叠加效果。 </summary>
    private void SetObjectMaterialOverlay(GameObject obj, Material overlayMat)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"Object {obj.name} does not have a Renderer component");
            return;
        }
        if (!_originalMaterials.TryGetValue(renderer, out var original))
        {
            renderer.material = overlayMat;
            Debug.LogWarning($"Object {obj.name} has no recorded original material, applied overlay only");
            return;
        }
        renderer.materials = new Material[] { original, overlayMat };
        Debug.Log($"Object {obj.name} material set to overlay (original + overlay)");
    }

    /// <summary> 将物体的材质恢复为记录的原材质（仅原材质，无叠加）。 </summary>
    private void RestoreObjectMaterial(GameObject obj)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;
        if (_originalMaterials.TryGetValue(renderer, out var original))
        {
            renderer.materials = new Material[] { original };
            Debug.Log($"Object {obj.name} material restored to original");
        }
    }

    // ----- 颜色 -----

    /// <summary> 戴面具时的颜色：戴面具可见的物体设为灰色。 </summary>
    private void ApplyColorsForMaskOn()
    {
        foreach (var obj in _seenWithMaskOnList)
        {
            if (obj.activeSelf)
                SetObjectColor(obj, Color.grey);
        }
    }

    /// <summary> 不戴面具时的颜色：不戴面具可见的物体设为白色。 </summary>
    private void ApplyColorsForMaskOff()
    {
        foreach (var obj in _seenWithMaskOffList)
        {
            if (obj.activeSelf)
                SetObjectColor(obj, Color.white);
        }
    }

    private void SetObjectColor(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
            Debug.Log($"Object {obj.name} color set to {color}");
        }
        else
        {
            Debug.LogWarning($"Object {obj.name} does not have a Renderer component");
        }
    }
}