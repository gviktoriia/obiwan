using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Interaction.Samples;

public class RestartButtonController : NetworkBehaviour
{
    public ParticleSystem hitEffect;
    public AudioClip hitSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HammerHead"))
        {
            if (IsServer)
            {
                TriggerRestart();
                PlayEffectsClientRpc();
            }
            else
            {
                RequestRestartServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRestartServerRpc()
    {
        TriggerRestart();
        PlayEffectsClientRpc();
    }

    private void TriggerRestart()
    {
        // 保持当前模式不变
        GameManager.Instance.RestartGameServerRpc(GameManager.Instance.CurrentMode.Value);
    }

    [ClientRpc]
    private void PlayEffectsClientRpc()
    {
        hitEffect.Play();
        GetComponent<AudioSource>().PlayOneShot(hitSound);
    }
}
