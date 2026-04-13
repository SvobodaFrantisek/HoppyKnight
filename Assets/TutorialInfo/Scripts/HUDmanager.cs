using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
    public TMP_Text coinText;
    public Slider healthBar;
    public TMP_Text gemText;

    [Header("Potion slot texty")]
    public TMP_Text potionSlot1Text;
    public TMP_Text potionSlot2Text;
    public TMP_Text potionSlot3Text;

    void Start()
    {
        PotionInventory potionInventory = FindAnyObjectByType<PotionInventory>();

        if (potionInventory != null)
        {
            UpdatePotionSlots(potionInventory.GetSlots(), potionInventory.GetSelectedSlotIndex());
        }
    }

    public void UpdateCoins(int coins)
    {
        if (coinText != null)
        {
            coinText.text = "coins: " + coins;
        }
    }

    public void UpdateGems(int gems, int remainGems)
    {
        if (gemText != null)
        {
            gemText.text = "gems: " + gems + "/" + remainGems;
        }
    }

    public void UpdateHealth(int health)
    {
        if (healthBar != null)
        {
            healthBar.value = health;
        }
    }

    public void UpdatePotionSlots(PotionType[] slots, int selectedSlotIndex)
    {
        if (slots == null)
        {
            return;
        }

        if (slots.Length > 0 && potionSlot1Text != null)
        {
            potionSlot1Text.text = "1: " + PotionTypeUtility.GetDisplayName(slots[0]);
        }

        if (slots.Length > 1 && potionSlot2Text != null)
        {
            potionSlot2Text.text = "2: " + PotionTypeUtility.GetDisplayName(slots[1]);
        }

        if (slots.Length > 2 && potionSlot3Text != null)
        {
            potionSlot3Text.text = "3: " + PotionTypeUtility.GetDisplayName(slots[2]);
        }
    }
}
