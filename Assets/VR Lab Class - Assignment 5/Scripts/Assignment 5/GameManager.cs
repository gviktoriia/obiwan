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
        public float gameDuration = 60f;
        public float easyMoleStayDuration = 2f;
        public float hardMoleStayDuration = 0.5f;
        public int maxEasyMoles = 3;
        public int maxHardMoles = 1;
    }
    public GameSettings settings;

    // UI引用
    //public TMP_Text timerText;
    //public TMP_Text scoreText;

    public enum GameMode { Easy, Hard }

    void Awake() => Instance = this;

    void Start()
    {
        Debug.Log("[GameManager] Start method called.");
        Debug.Log($"[GameManager] IsServer = {IsServer}");

    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("[GameManager] Initializing moles on server....");
            // 服务器初始化地鼠
            InitializeMoles();
        }
        else
        {
            Debug.Log("This instance is running as a client.");
            // 在客户端上执行的初始化逻辑
        }

        Debug.Log("GameManager 已初始化"); // 强制输出
        if (holePositions == null) Debug.LogError("Hole Positions are null 未绑定!");
        if (molePrefab == null) Debug.LogError("Mole Prefab is null 未绑定!");

    }

    // 初始化地鼠实例
    private void InitializeMoles()
    {
        foreach (Transform hole in holePositions)
        {
            GameObject mole = Instantiate(molePrefab, hole.position, Quaternion.Euler(-90, 0, 0));
            mole.GetComponent<NetworkObject>().Spawn();
            moles.Add(mole.GetComponent<MoleController>());
            Debug.Log($"[GameManager] Mole instantiated at {hole.position}, NetworkObject spawned: {mole.GetComponent<NetworkObject>().IsSpawned}");
        }
    }

    // 新增地鼠生成方法
    [ServerRpc]
    private void TrySpawnMoleServerRpc()
    {
        if (activeMoles.Value >= GetMaxMoles()) return;

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < moles.Count; i++)
        {
            if (!moles[i].IsActive.Value)
                availableIndices.Add(i);
        }

        if (availableIndices.Count > 0)
        {
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            moles[randomIndex].PopUpServerRpc();
        }
    }

    public void IncreaseActiveMoles()
    {
        if (IsServer)
        {
            activeMoles.Value++;
            Debug.Log($"[GameManager] Mole appeared. Active moles: {activeMoles.Value}");
        }
    }

    public void DecreaseActiveMoles()
    {
        if (IsServer && activeMoles.Value > 0) // 避免 activeMoles 变成负数
        {
            activeMoles.Value--;
            Debug.Log($"[GameManager] Mole disappeared. Active moles: {activeMoles.Value}");
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
        Debug.Log($"[Server] StartGameServerRpc called. IsServer: {IsServer}");
        if (!IsGameActive.Value)
        {
            CurrentMode.Value = mode;
            IsGameActive.Value = true;
            RemainingTime.Value = settings.gameDuration;


            // 确保初始生成的地鼠符合模式
            int initialMoles = (mode == GameMode.Easy) ? 3 : 1;
            for (int i = 0; i < initialMoles; i++)
            {
                TrySpawnMoleServerRpc();
            }
            foreach (var mole in moles)
            {
                mole.SetStayDuration(GetMoleStayDuration());
            }

            StartCoroutine(GameTimer());
            StartCoroutine(MoleSpawnCoroutine());
        }
    }

    // 新增获取停留时间的方法
    private float GetMoleStayDuration()
    {
        return (CurrentMode.Value == GameMode.Easy) ?
            settings.easyMoleStayDuration :
            settings.hardMoleStayDuration;
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
        foreach (var player in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (player.PlayerObject.GetComponent<PlayerUIController>() != null)
            {
                player.PlayerObject.GetComponent<PlayerUIController>().UpdateScore(newScore);
            }
        }
    }

    // 修改 RestartGameServerRpc 方法
    [ServerRpc]
    public void RestartGameServerRpc(GameMode modeToKeep)
    {
        // 1. 重置游戏状态
        IsGameActive.Value = false;
        RemainingTime.Value = settings.gameDuration;
        activeMoles.Value = 0;

        // 2. 清空现有地鼠
        foreach (var mole in moles)
        {
            mole.Hide();
        }

        // 3. 保持当前模式
        CurrentMode.Value = modeToKeep;

        // 4. 重新初始化地鼠停留时间
        foreach (var mole in moles)
        {
            mole.SetStayDuration(GetMoleStayDuration());
        }

        // 5. 重启游戏（延迟一帧避免冲突）
        StartCoroutine(DelayedRestart());
    }


    private IEnumerator DelayedRestart()
    {
        yield return null;
        StartGameServerRpc(CurrentMode.Value);
    }

    [ServerRpc]
    public void SetModeServerRpc(GameMode newMode)
    {
        CurrentMode.Value = newMode;
        Debug.Log($"切换模式到: {newMode}");
        // 立即更新所有地鼠的停留时间
        foreach (var mole in moles)
        {
            mole.SetStayDuration(GetMoleStayDuration());
        }
    }

    [ClientRpc]
    private void SyncModeClientRpc(GameMode mode)
    {
        CurrentMode.Value = mode;
        // UpdateUI(); // 更新UI显示当前模式
    }

    private IEnumerator GameTimer()
    {
        Debug.Log("[GameManager] Game timer started.");
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
        Debug.Log("[GameManager] Mole spawn coroutine started.");

        while (IsGameActive.Value)
        {
            Debug.Log("[GameManager] Trying to spawn mole..");
            TrySpawnMoleServerRpc();
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
        Debug.Log("[GameManager] Mole spawn coroutine ended.");
    }


    [ClientRpc]
    private void UpdateTimerClientRpc(float time)
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (player.PlayerObject.GetComponent<PlayerUIController>() != null)
            {
                player.PlayerObject.GetComponent<PlayerUIController>().UpdateTimer(time);
            }
        }
    }
    public void EndGame()
    {
        IsGameActive.Value = false;
        // 清理逻辑
    }
}