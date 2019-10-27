using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwordEquipIcon : MonoBehaviour
{
    public Image currentlyEquipped;
    public Image targetedSwordIcon;
    public Sprite emptyIcon;
    public Sprite zanbatoIcon;
    public Sprite broadswordIcon;
    public Sprite katanaIcon;

    public void UpdateCurrentlyEquipped(int swordType)
    {
        targetedSwordIcon.gameObject.SetActive(false);
        switch (swordType)
        {
            default:
                currentlyEquipped.sprite = emptyIcon;
                break;
            case 1:
                currentlyEquipped.sprite = zanbatoIcon;
                break;
            case 2:
                currentlyEquipped.sprite = broadswordIcon;
                break;
            case 3:
                currentlyEquipped.sprite = katanaIcon;
                break;
        }
    }
    public void ShowTargetedSwordType(int swordType)
    {
        targetedSwordIcon.gameObject.SetActive(true);
        switch (swordType)
        {
            default:
                targetedSwordIcon.gameObject.SetActive(false);
                break;
            case 1:
                targetedSwordIcon.sprite = zanbatoIcon;
                break;
            case 2:
                targetedSwordIcon.sprite = broadswordIcon;
                break;
            case 3:
                targetedSwordIcon.sprite = katanaIcon;
                break;
        }
    }
}
