using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRSYS.Core.Interaction.Samples;

public class UIButtonController : NetworkBehaviour
{
    public enum ButtonType { StartGame, Restart, Exit, ModeEasy, ModeHard }
    public ButtonType buttonType;

    [Header("Feedback")]
    public AudioClip clickSound;
    public float hapticStrength = 0.7f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log($"OnTriggerEnter called with {other.name}");
        // 本示例：只有自己所有的物体才能触发按钮
        if (!IsOwner) return;
        if (!other.CompareTag("HammerHead")) return;

        // 播放点击音效
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 如果有 XR Controller，可发送振动反馈
        XRBaseController xrController = other.GetComponentInParent<XRBaseController>();
        if (xrController != null)
        {
            xrController.SendHapticImpulse(hapticStrength, 0.3f);
        }

        // 通知服务器执行对应操作
        HandleButtonActionServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleButtonActionServerRpc()
    {
        switch (buttonType)
        {
            case ButtonType.StartGame:
                GameManager.Instance.StartGameServerRpc();
                break;
            case ButtonType.Restart:
                GameManager.Instance.RestartGameServerRpc();
                break;
            case ButtonType.Exit:
                NetworkManager.Singleton.Shutdown();
                break;
            case ButtonType.ModeEasy:
                GameManager.Instance.SetGameModeServerRpc(GameManager.GameMode.Easy);
                break;
            case ButtonType.ModeHard:
                GameManager.Instance.SetGameModeServerRpc(GameManager.GameMode.Hard);
                break;
        }
    }
}