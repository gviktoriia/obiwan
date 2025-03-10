using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ScoreboardUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text[] playerScoreTexts;
    [SerializeField] private TMP_Text timerText;

    void Update()
    {
        // 更新倒计时
        timerText.text = $"Time: {Mathf.FloorToInt(GameManager.Instance.RemainingTime.Value)}s";

        // 更新玩家分数（需扩展玩家数据存储）
        /* foreach (var player in PlayerManager.Players)
           playerScoreTexts[player.id].text = player.score.ToString(); */
    }
}