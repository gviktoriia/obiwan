using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class HammerHitDetector : NetworkBehaviour
{
    [Header("Settings")]
    public int hitScore = 10;        // The scores user get for hit 1 mole successfully
    public float hapticAmplitude = 0.5f; // Haptic when hit
    public float hapticDuration = 0.1f;  // haptic time when hit

    //References
    private XRBaseController controller;

    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
        // To Check whether hit a hole
        if (other.CompareTag("Mole"))
        {
            MoleController mole = other.GetComponent<MoleController>();
            if (mole != null) // To make sure Mole active
            {
                mole.HitServerRpc(NetworkManager.LocalClientId);
            }
        }
    }

    void Update()
    {

    }

}
