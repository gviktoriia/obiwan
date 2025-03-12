using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerUIController : NetworkBehaviour
{
    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text timerText;

    private void Start()
    {
        // 仅本地玩家初始化UI
        if (IsOwner)
        {
            // 绑定UI到头部
            Transform head = GetComponentInChildren<Camera>().transform;
            scoreText.transform.SetParent(head, false);
            timerText.transform.SetParent(head, false);

            // 初始隐藏
            scoreText.gameObject.SetActive(true);
            timerText.gameObject.SetActive(true);
        }
    }

    // 本地更新分数
    public void UpdateScore(int score)
    {
        if (IsOwner)
            scoreText.text = $"Score: {score}";
    }

    // 本地更新时间
    public void UpdateTimer(float time)
    {
        if (IsOwner)
            timerText.text = $"Time: {Mathf.FloorToInt(time)}s";
    }
}
