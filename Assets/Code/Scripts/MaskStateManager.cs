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

    /// <summary> 戴面具时可见的物体（Tag: SeenWithMaskOn）。假定 Awake 时均为 active，仅在此刻收集一次。 </summary>
    private List<GameObject> _seenWithMaskOnList = new List<GameObject>();

    /// <summary> 不戴面具时可见的物体（Tag: SeenWithMaskOff）。假定 Awake 时均为 active，仅在此刻收集一次。 </summary>
    private List<GameObject> _seenWithMaskOffList = new List<GameObject>();

    /// <summary> 当前被隐藏的物体，切换状态时先恢复再根据新状态隐藏另一批。 </summary>
    private List<GameObject> _inactiveObjects = new List<GameObject>();

    public void Awake()
    {
        RefreshTaggedObjectLists();
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
        HideListAndTrack(_seenWithMaskOffList);
        ApplyColorsForMaskOn();
        // ApplyMaterialForMaskOn();
        // SetColliderStatusForMaskOn();
    }

    /// <summary>
    /// 摘下面具时调用。只负责调用各小函数，后续扩展在此加一行即可。
    /// </summary>
    private void OnSetMaskDisactive()
    {
        HideListAndTrack(_seenWithMaskOnList);
        ApplyColorsForMaskOff();
        // ApplyMaterialForMaskOff();
        // SetColliderStatusForMaskOff();
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
