using UnityEngine;

public class PlayerOutfitManager : MonoBehaviour
{
    public GameObject PlayerOutfitWhenMaskOn;
    public GameObject PlayerOutfitWhenMaskOff;
    
    public void SetPlayerMaskOutfit(bool isMaskOn)
    {
        PlayerOutfitWhenMaskOn.SetActive(isMaskOn);
        PlayerOutfitWhenMaskOff.SetActive(!isMaskOn);
    }
}
