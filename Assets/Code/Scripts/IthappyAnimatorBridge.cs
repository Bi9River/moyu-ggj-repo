using System.Reflection;
using UnityEngine;

/// <summary>
/// 仅驱动 ithappy Creative_Characters_FREE 的 Animator，不参与移动控制。
/// 从 Starter Assets 的 ThirdPersonController + StarterAssetsInputs 读取状态，
/// 写入 ithappy Character_Movement 控制器需要的参数：Hor, Vert, State, IsJump。
/// 挂在和 ThirdPersonController、Animator 同一物体上即可。
/// 使用反射访问 Starter Assets，避免 Code 程序集引用 Unity.StarterAssets 造成循环依赖。
/// </summary>
[RequireComponent(typeof(Animator))]
public class IthappyAnimatorBridge : MonoBehaviour
{
    [Header("Animator 参数名（需与 ithappy Character_Movement.controller 一致）")]
    [SerializeField] private string horizontalParam = "Hor";
    [SerializeField] private string verticalParam = "Vert";
    [SerializeField] private string stateParam = "State";   // 0=走 1=跑
    [SerializeField] private string jumpParam = "IsJump";

    [Header("可选：平滑系数，0 表示不平滑")]
    [SerializeField, Min(0f)] private float inputSmoothSpeed = 4.5f;

    private Animator _animator;
    private Component _controller;
    private Component _input;
    private FieldInfo _groundedField;
    private FieldInfo _moveField, _sprintField, _jumpField;

    private Vector2 _flowAxis;
    private float _flowState;

    private int _horId;
    private int _vertId;
    private int _stateId;
    private int _jumpId;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        var controllerType = System.Type.GetType("StarterAssets.ThirdPersonController, Unity.StarterAssets");
        var inputType = System.Type.GetType("StarterAssets.StarterAssetsInputs, Unity.StarterAssets");
        if (controllerType != null) _controller = GetComponent(controllerType);
        if (inputType != null)
        {
            _input = GetComponent(inputType);
            if (_input != null)
            {
                _groundedField = controllerType?.GetField("Grounded", BindingFlags.Public | BindingFlags.Instance);
                _moveField = inputType.GetField("move", BindingFlags.Public | BindingFlags.Instance);
                _sprintField = inputType.GetField("sprint", BindingFlags.Public | BindingFlags.Instance);
                _jumpField = inputType.GetField("jump", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        _horId = Animator.StringToHash(horizontalParam);
        _vertId = Animator.StringToHash(verticalParam);
        _stateId = Animator.StringToHash(stateParam);
        _jumpId = Animator.StringToHash(jumpParam);
    }

    private void Update()
    {
        if (_animator == null || _controller == null || _input == null) return;

        // 从 Starter Assets 获取：移动方向、是否跑步、是否在空中（跳跃/下落）
        Vector2 axis = _moveField != null ? (Vector2)_moveField.GetValue(_input) : Vector2.zero;
        bool isRun = _sprintField != null && (bool)_sprintField.GetValue(_input);
        // Grounded 是 ThirdPersonController 的 public 字段（不是属性）
        bool grounded = _groundedField == null || (bool)_groundedField.GetValue(_controller);
        bool isAir = !grounded;

        // 与 ithappy CharacterMover 一致：State 0=走 1=跑
        float state = isRun ? 1f : 0f;

        if (inputSmoothSpeed > 0f)
        {
            float dt = Time.deltaTime;
            Vector2 delta = axis - _flowAxis;
            if (delta.sqrMagnitude > 0.01f)
                _flowAxis = Vector2.ClampMagnitude(_flowAxis + inputSmoothSpeed * dt * delta.normalized, 1f);
            else
                _flowAxis = Vector2.MoveTowards(_flowAxis, axis, inputSmoothSpeed * dt);

            _flowState = Mathf.MoveTowards(_flowState, state, inputSmoothSpeed * dt);
        }
        else
        {
            _flowAxis = Vector2.ClampMagnitude(axis, 1f);
            _flowState = state;
        }

        _animator.SetFloat(_horId, _flowAxis.x);
        _animator.SetFloat(_vertId, _flowAxis.y);
        _animator.SetFloat(_stateId, Mathf.Clamp01(_flowState));
        _animator.SetBool(_jumpId, isAir);
    }
}
