using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public NetworkNode currentNode;
    public ConnectionManager connManager;
    public float moveSpeed = 5f;

    private Vector3 targetMovePosition;
    private bool isMoving = false;

    void Start()
    {
        InitializePlayerPosition();
    }

    public void InitializePlayerPosition()
    {
        if (currentNode != null)
        {
            // 强制位置同步，不经过 Lerp
            Vector3 spawnPos = currentNode.transform.position + Vector3.up * 1.5f;
            transform.position = spawnPos;
            targetMovePosition = spawnPos;
            isMoving = false;
            Debug.Log("Player forced to starting node: " + currentNode.ipAddress);
        }
        else
        {
            // 如果没指定，尝试自动找场景里第一个节点
            NetworkNode firstNode = FindObjectOfType<NetworkNode>();
            if (firstNode != null)
            {
                currentNode = firstNode;
                InitializePlayerPosition();
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }

        // 平滑移动逻辑
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetMovePosition, Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, targetMovePosition) < 0.05f)
            {
                isMoving = false;
            }
        }
    }

    void HandleClick()
    {
        // 3D 射线检测
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            NetworkNode targetNode = hit.collider.GetComponent<NetworkNode>();

            if (targetNode != null && targetNode != currentNode)
            {
                // 进行子网判定
                if (connManager.CanConnect(currentNode.ipUint, targetNode.ipUint))
                {
                    MoveToNode(targetNode);
                }
                else
                {
                    Debug.Log("<color=red>Access Denied!</color> Node " + targetNode.ipAddress +
                              " is in a different subnet.");
                }
            }
        }
    }

    void MoveToNode(NetworkNode node)
    {
        currentNode = node;
        targetMovePosition = node.transform.position + Vector3.up * 1.5f;
        isMoving = true;
    }
}