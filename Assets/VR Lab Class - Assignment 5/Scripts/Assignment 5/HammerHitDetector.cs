using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class HammerHitDetector : NetworkBehaviour
{
    [Header("Settings")]
    public int hitScore = 10;
    public float hapticAmplitude = 0.5f;
    public float hapticDuration = 0.1f;

    // 这个是挥动锤子时给玩家震动反馈的控制器
    // 需要在 Inspector 或运行时手动赋值
    private XRBaseController controller;

    private void OnTriggerEnter(Collider other)
    {
        // 让服务器统一处理碰撞（避免多个客户端重复判定）
        if (!IsServer) return;

        if (other.CompareTag("Mole"))
        {
            MoleController mole = other.GetComponent<MoleController>();
            if (mole != null && mole.IsActive.Value)
            {
                // 服务器端让地鼠隐藏
                mole.OnHitServerRpc();

                // 给玩家加分（服务器端加分，Netcode 同步给所有人）
                GameManager.Instance.AddScoreServerRpc(hitScore);

                // 如果本地有手柄控制器，也可以做一次“本地震动”
                // 这里仅示例：若需要每个客户端都震动，需要再加网络同步或局部逻辑
                if (controller != null)
                {
                    controller.SendHapticImpulse(hapticAmplitude, hapticDuration);
                }
            }
        }
    }
}