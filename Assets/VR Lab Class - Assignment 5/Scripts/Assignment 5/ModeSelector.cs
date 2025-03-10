using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelector : NetworkBehaviour
{
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;

    void Start()
    {
        easyButton.onClick.AddListener(() => SetMode(GameManager.GameMode.Easy));
        hardButton.onClick.AddListener(() => SetMode(GameManager.GameMode.Hard));
    }

    [ServerRpc]
    private void SetModeServerRpc(GameManager.GameMode mode)
    {
        if (!GameManager.Instance.IsGameActive.Value)
            GameManager.Instance.CurrentMode.Value = mode;
    }

    // 客户端调用
    public void SetMode(GameManager.GameMode mode)
    {
        if (IsOwner)
            SetModeServerRpc(mode);
    }
}