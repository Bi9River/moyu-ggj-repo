using System;
using UnityEngine;

public class DisableMTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        var manager = FindObjectOfType<MaskStateManager>();
        manager.PlayerEnteredDisableMZone();
    }

    public void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        var manager = FindObjectOfType<MaskStateManager>();
        manager.PlayerEnteredDisableMZone();
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        var manager = FindObjectOfType<MaskStateManager>();
        manager.PlayerExitedDisableMZone();
    }
}