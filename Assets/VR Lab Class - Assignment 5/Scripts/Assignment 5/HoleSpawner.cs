using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleSpawner : MonoBehaviour
{
    public GameObject molePrefab; // create a mole
    public Transform spawnPoint;
    private GameObject currentMole;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        // create original Mole then hide
        currentMole = Instantiate(molePrefab, spawnPoint.position, Quaternion.identity);
        currentMole.transform.SetParent(spawnPoint);
        currentMole.transform.localPosition = Vector3.down * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
