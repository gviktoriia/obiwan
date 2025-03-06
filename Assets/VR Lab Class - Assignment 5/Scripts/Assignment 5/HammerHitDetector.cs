using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HammerHitDetector : MonoBehaviour
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
            if (mole != null && mole.isActive) // To make sure Mole active
            {
                mole.OnHit();
                

                // active the haptic on controller
                if (controller != null)
                {
                    controller.SendHapticImpulse(hapticAmplitude, hapticDuration);
                }

                GameManager.Instance.AddScore(hitScore);
            }
        }
    }

    void Update()
    {

    }

}
