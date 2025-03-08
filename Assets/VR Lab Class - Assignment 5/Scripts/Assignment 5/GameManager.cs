using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    // 游戏难度模式
    public enum GameMode { Easy, Hard }

    // 网络变量：同步给所有客户端
    public NetworkVariable<GameMode> CurrentMode = new NetworkVariable<GameMode>(GameMode.Easy);
    public NetworkVariable<bool> IsGameActive = new NetworkVariable<bool>(false);
    public NetworkVariable<int> TotalScore = new NetworkVariable<int>(0);

    [Header("Game Settings")]
    public MoleController[] holes;     // 在Inspector手动拖拽所有地鼠进来
    public float easySpawnInterval = 2f;
    public float hardSpawnInterval = 1f;
    public int easyMaxMoles = 3;       // Easy模式时，一次最多弹出的地鼠数量
    public float moleShowDuration = 2f;// Easy模式地鼠弹出后停留时间

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text modeDisplayText;
    public GameObject startPanel;
    public GameObject modeSelectionPanel;
    public GameObject inGamePanel;

    private Coroutine spawnCoroutine;

    private void Awake()
    {
        // 单例模式
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 当网络真正初始化后，再去更新 UI
        UpdateAllUI();

        // 监听网络变量的变化
        CurrentMode.OnValueChanged += OnGameModeChanged;
        TotalScore.OnValueChanged += OnScoreChanged;
    }

    private void Start()
    {
        // 本地初始化UI
        InitializeGame();
    }

    private void InitializeGame()
    {
        // 默认显示开始面板
        UpdateUIPanelsClientRpc(showStart: true, showInGame: false);
        UpdateModeDisplay();
    }

    #region ========= 游戏流程控制 =========

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (!IsGameActive.Value)
        {
            IsGameActive.Value = true;
            // 启动服务器端协程，不断随机弹地鼠
            spawnCoroutine = StartCoroutine(SpawnMoles());
            UpdateUIPanelsClientRpc(false, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RestartGameServerRpc()
    {
        // 停止旧协程
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        // 分数清零
        TotalScore.Value = 0;
        // 先隐藏所有地鼠
        ResetAllMoles();
        // 重新开始游戏
        IsGameActive.Value = false;
        StartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGameModeServerRpc(GameMode newMode)
    {
        CurrentMode.Value = newMode;
        AdjustMoleParameters();
    }

    #endregion

    #region ========= 随机弹地鼠逻辑 =========

    private IEnumerator SpawnMoles()
    {
        while (IsGameActive.Value)
        {
            // 等待一个随机或固定的间隔
            yield return new WaitForSeconds(GetCurrentSpawnInterval());

            // 根据模式决定一次弹出单个或多个
            if (CurrentMode.Value == GameMode.Easy)
                SpawnMultipleMoles();
            else
                SpawnSingleMole();
        }
    }

    private void SpawnSingleMole()
    {
        int index = Random.Range(0, holes.Length);
        // 只对未激活的地鼠做 PopUp
        if (!holes[index].IsActive.Value)
        {
            holes[index].PopUpServerRpc();
        }
    }

    private void SpawnMultipleMoles()
    {
        // 在 [1, easyMaxMoles] 范围内随机出一个数量
        int count = Random.Range(1, easyMaxMoles + 1);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, holes.Length);
            if (!holes[index].IsActive.Value)
            {
                holes[index].PopUpServerRpc();
            }
        }
    }

    private void ResetAllMoles()
    {
        foreach (var mole in holes)
        {
            if (mole.IsActive.Value)
                mole.HideServerRpc();
        }
    }

    private void AdjustMoleParameters()
    {
        // 简单模式：弹出时间长
        // 困难模式：时间缩短
        foreach (var mole in holes)
        {
            if (CurrentMode.Value == GameMode.Easy)
                mole.StayDuration.Value = moleShowDuration;
            else
                mole.StayDuration.Value = moleShowDuration * 0.6f;
        }
    }

    public float GetCurrentSpawnInterval()
    {
        return (CurrentMode.Value == GameMode.Easy) ? easySpawnInterval : hardSpawnInterval;
    }

    #endregion

    #region ========= UI 显示更新 =========

    [ClientRpc]
    private void UpdateUIPanelsClientRpc(bool showStart, bool showInGame)
    {
        if (startPanel != null) startPanel.SetActive(showStart);
        if (inGamePanel != null) inGamePanel.SetActive(showInGame);
        if (modeSelectionPanel != null) modeSelectionPanel.SetActive(false);

        UpdateModeDisplay();
    }

    private void UpdateAllUI()
    {
        if (scoreText != null) scoreText.text = $"SCORE: {TotalScore.Value}";
        UpdateModeDisplay();
    }

    private void UpdateModeDisplay()
    {
        if (modeDisplayText == null) return;
        modeDisplayText.text = $"MODE: {CurrentMode.Value}";
        modeDisplayText.color = (CurrentMode.Value == GameMode.Easy) ? Color.green : Color.red;
    }

    private void OnGameModeChanged(GameMode previous, GameMode current)
    {
        UpdateModeDisplay();
    }

    private void OnScoreChanged(int previous, int current)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {current}";
    }

    #endregion

    #region ========= 分数管理 =========

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int points)
    {
        TotalScore.Value += points;
    }

    public int GetMaxConcurrentMoles()
    {
        if (CurrentMode.Value == GameMode.Easy)
            return easyMaxMoles; // 这可能是3
        else
            return 5; // 你可改成别的数字
    }

    #endregion
}