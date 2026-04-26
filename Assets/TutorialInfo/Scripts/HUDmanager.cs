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

        // selectedSlotIndex si nechavame pro pripadne budouci zvyrazneni aktivniho slotu.
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
