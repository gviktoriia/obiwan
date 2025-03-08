using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class HoleSpawner : NetworkBehaviour
{
    public GameObject molePrefab;
    public Transform[] spawnPoints;

    private MoleController[] moles;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeMoles();
            StartCoroutine(SpawnMoles());
        }
    }

    void InitializeMoles()
    {
        moles = new MoleController[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject mole = Instantiate(molePrefab, spawnPoints[i].position, Quaternion.identity);
            mole.GetComponent<NetworkObject>().Spawn();
            moles[i] = mole.GetComponent<MoleController>();
            moles[i].transform.SetParent(spawnPoints[i]);
            moles[i].HideServerRpc();
        }
    }

    IEnumerator SpawnMoles()
    {
        while (GameManager.Instance.IsGameActive.Value)
        {
            yield return new WaitForSeconds(GameManager.Instance.GetCurrentSpawnInterval());

            if (GameManager.Instance.CurrentMode.Value == GameManager.GameMode.Easy)
                SpawnMultipleMoles();
            else
                SpawnSingleMole();
        }
    }

    void SpawnSingleMole()
    {
        int index = Random.Range(0, moles.Length);
        if (!moles[index].IsActive.Value)
            moles[index].PopUpServerRpc();
    }

    void SpawnMultipleMoles()
    {
        int count = Random.Range(1, GameManager.Instance.GetMaxConcurrentMoles() + 1);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, moles.Length);
            if (!moles[index].IsActive.Value)
                moles[index].PopUpServerRpc();
        }
    }
}