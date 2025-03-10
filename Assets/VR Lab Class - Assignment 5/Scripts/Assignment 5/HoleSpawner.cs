using Unity.Netcode;
using UnityEngine;

public class HoleSpawner : NetworkBehaviour
{
    // 用于可视化地洞位置（场景中手动调整）
    public Transform holeVisual;

    // 地洞位置偏移量（用于地鼠生成）
    public Vector3 moleSpawnOffset = new Vector3(0, 0.5f, 0);

    void Start()
    {
        // 初始化时隐藏可视化对象
        if (holeVisual != null)
            holeVisual.gameObject.SetActive(false);
    }

    public Vector3 GetSpawnPosition()
    {
        return transform.position + moleSpawnOffset;
    }

    // 网络生成时同步位置（可选）
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // 确保位置同步
            GetComponent<NetworkObject>().TrySetParent(transform.parent);
        }
    }
}