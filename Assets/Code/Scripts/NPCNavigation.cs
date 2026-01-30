using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ChaseInRange : MonoBehaviour
{
    [Header("Detection")]
    public float chaseRadius = 6f;
    public string playerTag = "Player";

    [Header("Behaviour")]
    public float stopDistance = 1.2f;
    public bool faceTarget = true;

    NavMeshAgent agent;
    Transform player;
    Vector3 spawnPos;
    bool chasing;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        spawnPos = transform.position;

        // 自动配置/校验 Trigger Sphere（可省去手动调）
        var sphere = GetComponent<SphereCollider>();
        if (!sphere) sphere = gameObject.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = chaseRadius;
    }

    void Update()
    {
        if (!chasing || player == null) return;

        agent.SetDestination(player.position);

        // 如果你想“明显朝向玩家”，可以额外手动转向（可选）
        if (faceTarget && agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 to = player.position - transform.position;
            to.y = 0;
            if (to.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(to);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        player = other.transform;
        chasing = true;

        // 让 Agent 自己旋转也行（一般更自然）
        agent.updateRotation = !faceTarget ? true : agent.updateRotation;

        agent.gameObject.tag = "SeenWithMaskOff";
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        chasing = false;
        player = null;

        agent.ResetPath(); // 停止
        // 可选：回到出生点
        // agent.SetDestination(spawnPos);
    }
}
