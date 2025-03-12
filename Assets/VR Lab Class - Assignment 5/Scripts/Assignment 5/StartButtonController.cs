using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;
using VRSYS.Core.Networking;

public class StartButtonController : NetworkBehaviour
{
    //public AudioClip hitSound;
    //public ParticleSystem hitEffect;

    private void OnTriggerEnter(Collider other)
    {
        // 所有客户端检测碰撞，但仅服务器执行逻辑
        Debug.Log($"[All Clients] Collision detected with: {other?.gameObject.name}");

        if (other.CompareTag("HammerHead"))
        {
            Debug.Log($"[All Clients] Hammer hit detected. IsServer: {IsServer}");

            // 客户端向服务器发送请求
            if (!IsServer)
            {
                Debug.Log("[Client] Sending start request to server");
                RequestStartGameServerRpc();
            }
            else
            {
                Debug.Log("[Server] Directly starting game");
                DirectStartGame();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestStartGameServerRpc()
    {
        Debug.Log("[Server] Received start request from client");
        DirectStartGame();
    }

    private void DirectStartGame()
    {
        if (!GameManager.Instance.IsGameActive.Value)
        {
            Debug.Log("[Server] Starting game...");
            GameManager.Instance.StartGameServerRpc(GameManager.GameMode.Easy);
            //PlayEffectsClientRpc();
        }
    }

    //[ClientRpc]
    //private void PlayEffectsClientRpc()
    //{
    //    // 本地音效和粒子效果
    //    GetComponent<AudioSource>().PlayOneShot(hitSound);
    //    hitEffect.Play();

    //    // 本地触觉反馈（在所有客户端触发自己手柄的震动）
    //    TriggerLocalHapticFeedback();
    //}

    //private void TriggerLocalHapticFeedback()
    //{
    //    // 获取所有活动手柄设备
    //    var inputDevices = new List<InputDevice>();
    //    InputDevices.GetDevicesWithCharacteristics(
    //        InputDeviceCharacteristics.Controller |
    //        InputDeviceCharacteristics.HeldInHand,
    //        inputDevices);

    //    foreach (var device in inputDevices)
    //    {
    //        if (device.isValid)
    //        {
    //            // 发送震动脉冲（0.5强度，0.2秒）
    //            if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities) &&
    //                capabilities.supportsImpulse)
    //            {
    //                device.SendHapticImpulse(0, 0.5f, 0.2f);
    //            }
    //        }
    //    }
    //}
}