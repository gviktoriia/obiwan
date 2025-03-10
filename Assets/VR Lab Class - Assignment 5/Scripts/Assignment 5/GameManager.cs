using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    // 网络同步变量
    public NetworkVariable<bool> IsGameActive = new NetworkVariable<bool>(false);
    public NetworkVariable<float> RemainingTime = new NetworkVariable<float>(0f);
    public NetworkVariable<GameMode> CurrentMode = new NetworkVariable<GameMode>(GameMode.Easy);
    public NetworkVariable<int> activeMoles = new NetworkVariable<int>();

    [Header("Game Elements")]
    public Transform[] holePositions; // 拖入场景中所有地洞的位置
    public GameObject molePrefab;     // 地鼠预制体

    // 存储所有地鼠实例
    private List<MoleController> moles = new List<MoleController>();

    // 游戏设置
    [System.Serializable]
    public class GameSettings
    {
        public float easyModeDuration = 120f;
        public float hardModeDuration = 60f;
        public int maxEasyMoles = 3;
        public int maxHardMoles = 1;
    }
    public GameSettings settings;

    // UI引用
    public TMP_Text timerText;
    public TMP_Text scoreText;

    public enum GameMode { Easy, Hard }

    void Awake() => Instance = this;

    void Start()
    {
        if (IsServer)
        {
            // 服务器初始化地鼠
            InitializeMoles();
        }
        // 在 GameManager.cs 的 Start 方法中添加

        Debug.Log("GameManager 已初始化"); // 强制输出
        if (holePositions == null) Debug.LogError("Hole Positions 未绑定!");
        if (molePrefab == null) Debug.LogError("Mole Prefab 未绑定!");
    }

    // 初始化地鼠实例
    private void InitializeMoles()
    {
        foreach (Transform hole in holePositions)
        {
            GameObject mole = Instantiate(molePrefab, hole.position, Quaternion.identity);
            mole.GetComponent<NetworkObject>().Spawn();
            moles.Add(mole.GetComponent<MoleController>());
        }
    }

    // 新增地鼠生成方法
    [ServerRpc]
    private void TrySpawnMoleServerRpc()
    {
        if (activeMoles.Value < GetMaxMoles())
        {
            Debug.Log($"Try to create Moles, activeMoles are:{activeMoles.Value}");
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < moles.Count; i++)
            {
                if (!moles[i].IsActive.Value)
                    availableIndices.Add(i);
                    Debug.Log($"Availiable holes:{i}");
            }

            if (availableIndices.Count > 0)
            {
                int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
                moles[randomIndex].PopUpServerRpc();
                activeMoles.Value++;
                Debug.Log($"Moles created, activeMoles are:{activeMoles.Value}");
            }
        }
    }

    private int GetMaxMoles()
    {
        return (CurrentMode.Value == GameMode.Easy) ?
            settings.maxEasyMoles :
            settings.maxHardMoles;
    }

    [ServerRpc]
    public void StartGameServerRpc(GameMode mode)
    {
        if (!IsGameActive.Value)
        {
            CurrentMode.Value = mode;
            IsGameActive.Value = true;
            RemainingTime.Value = (mode == GameMode.Easy) ?
                settings.easyModeDuration : settings.hardModeDuration;

            StartCoroutine(GameTimer());
            StartCoroutine(MoleSpawnCoroutine());
        }
    }

    // 玩家分数字典（服务器端存储）
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    // 分数同步方法
    [ServerRpc]
    public void AddScoreServerRpc(ulong clientId, int points)
    {
        if (!playerScores.ContainsKey(clientId))
            playerScores[clientId] = 0;

        playerScores[clientId] += points;
        UpdateScoresClientRpc(clientId, playerScores[clientId]);
    }

    [ClientRpc]
    private void UpdateScoresClientRpc(ulong clientId, int newScore)
    {
        // 更新本地UI显示
        if (clientId == NetworkManager.LocalClientId)
        {
            scoreText.text = $"Your Score: {newScore}";
        }
    }

    [ServerRpc]
    public void RestartGameServerRpc() {
        /* 重置地鼠和分数逻辑 */
        /* 重置地鼠和分数逻辑 */
    }

    private IEnumerator GameTimer()
    {
        while (RemainingTime.Value > 0)
        {
            RemainingTime.Value -= Time.deltaTime;
            UpdateTimerClientRpc(RemainingTime.Value);
            yield return null;
        }
        EndGame();
    }

    private IEnumerator MoleSpawnCoroutine()
    {
        while (IsGameActive.Value)
        {
            TrySpawnMoleServerRpc();
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float time)
    {
        timerText.text = $"Time: {Mathf.FloorToInt(time)}s";
    }

    public void EndGame()
    {
        IsGameActive.Value = false;
        // 清理逻辑
    }
}