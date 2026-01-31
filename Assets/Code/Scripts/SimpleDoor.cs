using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0, 4, 0); // 门往哪儿开，开多远（默认向上升4米）
    public float speed = 5f; // 开门速度

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;
    }

    void Update()
    {
        // 让门平滑地移动到目标位置
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
    }

    public void Open()
    {
        targetPosition = closedPosition + openOffset;
         Debug.Log("门收到指令：开门！"); // 添加这一行
    targetPosition = closedPosition + openOffset;
    }

    public void Close()
    {
        targetPosition = closedPosition;
    }


}