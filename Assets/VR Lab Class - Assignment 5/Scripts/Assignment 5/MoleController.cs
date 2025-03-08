using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class MoleController : NetworkBehaviour
{
    /// <summary>
    /// 是否激活状态（是否“顶出来”），由服务器控制，所有客户端可读
    /// </summary>
    public NetworkVariable<bool> IsActive = new NetworkVariable<bool>( );

    /// <summary>
    /// 地鼠在地面上停留的时间（可在 easy/hard 模式下动态修改）
    /// </summary>
    public NetworkVariable<float> StayDuration = new NetworkVariable<float>(
        2f
    );

    [Header("Visual Settings")]
    public float popUpSpeed = 5f;  // 弹出速度
    public float hideSpeed = 5f;   // 回退速度

    private Vector3 hiddenPos;     // 地鼠初始下方位置
    private Vector3 visiblePos;    // 地鼠顶出后的位置
    private Coroutine moveCoroutine;

    private void Awake()
    {
        // 计算上下两种位置
        hiddenPos = transform.position + Vector3.down * 0.5f;
        visiblePos = transform.position + Vector3.up * 0.1f;

        // 初始化时先隐藏在下方
        transform.position = hiddenPos;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        IsActive.Value = false;

        // 当 IsActive 值发生变化时，所有客户端都执行对应动画
        IsActive.OnValueChanged += (oldVal, newVal) =>
        {
            if (newVal) // 变为 true，弹出来
            {
                if (moveCoroutine != null) StopCoroutine(moveCoroutine);
                moveCoroutine = StartCoroutine(MoveTo(visiblePos, popUpSpeed));

                // 服务器端再启动一个“StayDuration”后隐藏的协程
                if (IsServer)
                {
                    StartCoroutine(HideAfterDelay(StayDuration.Value));
                }
            }
            else // 变为 false，隐藏回去
            {
                if (moveCoroutine != null) StopCoroutine(moveCoroutine);
                moveCoroutine = StartCoroutine(MoveTo(hiddenPos, hideSpeed));
            }
        };
    }

    // 使用协程来平滑移动到目标位置
    private IEnumerator MoveTo(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    // 停留一定时间后自动回到隐藏状态
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (IsActive.Value)
            IsActive.Value = false;
    }

    // 让服务器端弹出地鼠
    [ServerRpc]
    public void PopUpServerRpc()
    {
        if (!IsActive.Value)
        {
            IsActive.Value = true;
        }
    }

    // 让服务器端隐藏地鼠
    [ServerRpc]
    public void HideServerRpc()
    {
        if (IsActive.Value)
        {
            IsActive.Value = false;
        }
    }

    // 当被击中时，让服务器端把地鼠设为隐藏
    [ServerRpc(RequireOwnership = false)]
    public void OnHitServerRpc()
    {
        if (IsActive.Value)
        {
            IsActive.Value = false;
        }
    }
}