using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleSpawner : MonoBehaviour
{
    public GameObject molePrefab; // Префаб крота
    public Transform[] spawnPoints; // Масив точок, де можуть з'являтися кроти
    public float minSpawnInterval = 1f; // Мінімальний інтервал між появами кротів
    public float maxSpawnInterval = 3f; // Максимальний інтервал між появами кротів

    private List<MoleController> moles = new List<MoleController>();
    private bool isSpawing = false;

    void Start()
    {
        // Створюємо кротів для кожної дірки
        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject mole = Instantiate(molePrefab, spawnPoint.position, spawnPoint.rotation);
            mole.transform.SetParent(spawnPoint);
            MoleController moleController = mole.GetComponent<MoleController>();
            moles.Add(moleController);

            moleController.Hide();
        }

        // Запускаємо корутину для випадкової появи кротів
        StartCoroutine(SpawnMoles());
    }

    IEnumerator SpawnMoles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            bool hasActiveMole = false;
            foreach(MoleController mole in moles)
            {
                if (mole.isActive)
                {
                    hasActiveMole = true;
                    break;
                }
            }

            if (!hasActiveMole)
            {
                List<int> availableIndices = new List<int>();
                for (int i = 0; i < moles.Count; i++)
                {
                    if (!moles[i].isActive)
                    {
                        availableIndices.Add(i);
                    }
                }

                if (availableIndices.Count > 0)
                {
                    int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
                    moles[randomIndex].PopUp();
                }
            }
        }
    }
}