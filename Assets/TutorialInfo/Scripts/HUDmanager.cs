using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
    // --------------------
    // Zakladni HUD reference
    // --------------------
    public TMP_Text coinText;
    public Slider healthBar;
    public TMP_Text gemText;

    [Header("Potion slot texty")]
    public TMP_Text potionSlot1Text;
    public TMP_Text potionSlot2Text;
    public TMP_Text potionSlot3Text;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        PotionInventory potionInventory = FindAnyObjectByType<PotionInventory>();

        if (potionInventory != null)
        {
            UpdatePotionSlots(potionInventory.GetSlots());
        }
    }

    // --------------------
    // Zakladni hodnoty HUDu
    // --------------------
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

    public void UpdateHealth(int health, int maxHealth)
    {
        if (healthBar == null)
        {
            return;
        }

        if (maxHealth <= 0)
        {
            maxHealth = 1;
        }

        if (health < 0)
        {
            health = 0;
        }

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

    // --------------------
    // Texty potion slotu
    // --------------------
    public void UpdatePotionSlots(PotionType[] slots)
    {
        if (slots == null)
        {
            return;
        }

        UpdatePotionSlotText(potionSlot1Text, slots, 0, "1: ");
        UpdatePotionSlotText(potionSlot2Text, slots, 1, "2: ");
        UpdatePotionSlotText(potionSlot3Text, slots, 2, "3: ");
    }

    void UpdatePotionSlotText(TMP_Text slotText, PotionType[] slots, int slotIndex, string prefix)
    {
        if (slotText == null)
        {
            return;
        }

        if (slots.Length <= slotIndex)
        {
            return;
        }

        slotText.text = prefix + PotionTypeUtility.GetDisplayName(slots[slotIndex]);
    }
}
