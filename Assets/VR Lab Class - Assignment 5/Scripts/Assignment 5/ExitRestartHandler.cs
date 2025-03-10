using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ExitRestartHandler : NetworkBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button restartButton;

    void Start()
    {
        exitButton.onClick.AddListener(ExitGame);
        restartButton.onClick.AddListener(RestartGame);
    }

    public void ExitGame()
    {
        if (IsServer) NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }

    public void RestartGame()
    {
        if (IsServer)
            GameManager.Instance.RestartGameServerRpc();
    }
}
