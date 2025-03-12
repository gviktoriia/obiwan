using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Interaction.Samples;

public class ModeSelector : NetworkBehaviour
{
    [Header("UI Elements")]
    public GameObject easyModeButton; // 简单模式按钮物体
    public GameObject hardModeButton; // 困难模式按钮物体

    [Header("Feedback")]
    public ParticleSystem hitEffect;  // 击打特效
    public AudioClip switchSound;     // 切换音效

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HammerHead"))
        {
            // 服务器端处理模式切换
            if (IsServer)
            {
                ToggleMode();
                PlayEffectsClientRpc();
            }
            else
            {
                RequestToggleModeServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestToggleModeServerRpc()
    {
        ToggleMode();
        PlayEffectsClientRpc();
    }

    private void ToggleMode()
    {
        var currentMode = GameManager.Instance.CurrentMode.Value;
        var newMode = (currentMode == GameManager.GameMode.Easy) ?
            GameManager.GameMode.Hard :
            GameManager.GameMode.Easy;

        GameManager.Instance.SetModeServerRpc(newMode);
    }

    [ClientRpc]
    private void PlayEffectsClientRpc()
    {
        hitEffect.Play();
        GetComponent<AudioSource>().PlayOneShot(switchSound);
    }
}