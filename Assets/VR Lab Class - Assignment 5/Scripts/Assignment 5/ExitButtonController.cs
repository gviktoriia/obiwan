using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Interaction.Samples;

public class ExitButtonController : NetworkBehaviour
{
    public ParticleSystem hitEffect;  // 击打特效
    public AudioClip hitSound;        // 击打音效

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HammerHead"))
        {
            // 服务器端处理退出逻辑
            if (IsServer)
            {
                PlayEffectsClientRpc();
                QuitGame();
            }
            else
            {
                // 客户端直接退出（无需服务器同步）
                PlayEffectsClientRpc();
                QuitGame();
            }
        }
    }

    [ClientRpc]
    private void PlayEffectsClientRpc()
    {
        hitEffect.Play();
        GetComponent<AudioSource>().PlayOneShot(hitSound);
    }

    private void QuitGame()
    {
        // 关闭网络连接（如果是多人游戏）
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        // 退出应用
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}