using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopkeeperScript : MonoBehaviour
{
    // --------------------
    // Napojeni z Inspectoru
    // --------------------
    public GameObject shopUI;
    public GameManager gameManager;
    public PotionInventory potionInventory;
    public FPSController playerController;

    [Header("Shop Lock")]
    public bool lockPlayerAndCameraWhileShopOpen = true;

    public int healthCost = 0;
    public int speedCost = 0;
    public int jumpBoostCost = 0;
    public int doubleJumpCost = 0;
    public int strengthCost = 0;

    public bool playerInRange = false;

    private readonly List<Behaviour> disabledCameraInputBehaviours = new List<Behaviour>();
    private bool shopLockApplied;
    private bool hadPlayerControllerState;
    private bool previousInputLocked;
    private bool previousMovementLocked;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        CacheReferences();

        if (shopUI != null)
        {
            RefreshShopPriceTexts();
            SetShopVisible(false);
        }
        else
        {
            Debug.LogError("shop panel neni prirazen");
        }
    }

    void Update()
    {
        TryToggleShopFromInput();
    }

    void OnDisable()
    {
        ReleaseShopLock();
    }

    // --------------------
    // Trigger zony shopu
    // --------------------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;

        if (shopUI != null && shopUI.activeSelf)
        {
            SetShopVisible(false);
        }
    }

    // --------------------
    // UI tlacitka shopu
    // --------------------
    public void OpenShop()
    {
        if (shopUI == null)
        {
            return;
        }

        SetShopVisible(!shopUI.activeSelf);
    }

    public void BuyHealth()
    {
        TryBuyPotion(PotionType.Heal, healthCost);
    }

    public void BuySpeed()
    {
        TryBuyPotion(PotionType.Speed, speedCost);
    }

    public void BuyJumpBoost()
    {
        TryBuyPotion(PotionType.JumpBoost, jumpBoostCost);
    }

    public void BuyDoubleJump()
    {
        TryBuyPotion(PotionType.DoubleJump, doubleJumpCost);
    }

    public void BuyStrength()
    {
        TryBuyPotion(PotionType.Strength, strengthCost);
    }

    // --------------------
    // Nakup potionu
    // --------------------
    bool TryBuyPotion(PotionType potionType, int cost)
    {
        CacheReferences();

        if (!HasRequiredReferences())
        {
            return false;
        }

        if (!potionInventory.HasFreeSlot())
        {
            return false;
        }

        if (!gameManager.TrySpendCoins(cost))
        {
            return false;
        }

        if (!potionInventory.TryAddPotion(potionType))
        {
            Debug.LogWarning("Potion se nepodarilo vlozit do inventare.");
            return false;
        }

        return true;
    }

    // --------------------
    // Ovladani a reference
    // --------------------
    void TryToggleShopFromInput()
    {
        if (!playerInRange)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
    }

    void CacheReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (potionInventory == null)
        {
            potionInventory = FindAnyObjectByType<PotionInventory>();
        }

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FPSController>();
        }
    }

    bool HasRequiredReferences()
    {
        if (potionInventory == null)
        {
            Debug.LogWarning("PotionInventory nebyl nalezen.");
            return false;
        }

        if (gameManager == null)
        {
            Debug.LogWarning("GameManager nebyl nalezen.");
            return false;
        }

        return true;
    }

    // --------------------
    // Zobrazeni shopu
    // --------------------
    void SetShopVisible(bool visibleState)
    {
        if (shopUI == null)
        {
            return;
        }

        if (visibleState)
        {
            RefreshShopPriceTexts();
            ApplyShopLock();
        }
        else
        {
            ReleaseShopLock();
        }

        shopUI.SetActive(visibleState);

        if (visibleState)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ApplyShopLock()
    {
        if (!lockPlayerAndCameraWhileShopOpen || shopLockApplied)
        {
            return;
        }

        CacheReferences();

        if (playerController != null)
        {
            hadPlayerControllerState = true;
            previousInputLocked = playerController.inputLocked;
            previousMovementLocked = playerController.movementLocked;
            playerController.SetInputLocked(true);
            playerController.SetMovementLocked(true);
        }
        else
        {
            hadPlayerControllerState = false;
        }

        DisableCameraInputBehaviours();
        shopLockApplied = true;
    }

    void ReleaseShopLock()
    {
        if (!shopLockApplied)
        {
            return;
        }

        if (playerController != null && hadPlayerControllerState)
        {
            playerController.SetInputLocked(previousInputLocked);
            playerController.SetMovementLocked(previousMovementLocked);
        }

        RestoreCameraInputBehaviours();
        shopLockApplied = false;
    }

    void DisableCameraInputBehaviours()
    {
        disabledCameraInputBehaviours.Clear();

        Behaviour[] behaviours = FindObjectsByType<Behaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];

            if (behaviour == null || !behaviour.enabled)
            {
                continue;
            }

            if (!IsCinemachineInputBehaviour(behaviour))
            {
                continue;
            }

            disabledCameraInputBehaviours.Add(behaviour);
            behaviour.enabled = false;
        }
    }

    void RestoreCameraInputBehaviours()
    {
        for (int i = 0; i < disabledCameraInputBehaviours.Count; i++)
        {
            Behaviour behaviour = disabledCameraInputBehaviours[i];

            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        disabledCameraInputBehaviours.Clear();
    }

    bool IsCinemachineInputBehaviour(Behaviour behaviour)
    {
        string typeName = behaviour.GetType().Name;

        return typeName.Contains("Cinemachine") && typeName.Contains("Input");
    }

    void RefreshShopPriceTexts()
    {
        if (shopUI == null)
        {
            return;
        }

        TMP_Text[] shopTexts = shopUI.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < shopTexts.Length; i++)
        {
            RefreshPotionPriceText(shopTexts[i]);
        }
    }

    void RefreshPotionPriceText(TMP_Text shopText)
    {
        if (shopText == null)
        {
            return;
        }

        string potionLabel = GetPotionLabel(shopText.text);

        if (string.IsNullOrEmpty(potionLabel))
        {
            return;
        }

        int cost = GetPotionCost(potionLabel);
        shopText.text = potionLabel + "\n" + GetCoinPriceText(cost);
    }

    string GetPotionLabel(string currentText)
    {
        if (string.IsNullOrEmpty(currentText))
        {
            return "";
        }

        string normalizedText = currentText.Trim().ToUpperInvariant();

        if (normalizedText.StartsWith("DOUBLE JUMP"))
        {
            return "DOUBLE JUMP";
        }

        if (normalizedText.StartsWith("JUMP BOOST"))
        {
            return "JUMP BOOST";
        }

        if (normalizedText.StartsWith("STRENGTH"))
        {
            return "STRENGTH";
        }

        if (normalizedText.StartsWith("SPEED"))
        {
            return "SPEED";
        }

        if (normalizedText.StartsWith("HEAL"))
        {
            return "HEAL";
        }

        return "";
    }

    int GetPotionCost(string potionLabel)
    {
        if (potionLabel == "HEAL")
        {
            return healthCost;
        }

        if (potionLabel == "SPEED")
        {
            return speedCost;
        }

        if (potionLabel == "JUMP BOOST")
        {
            return jumpBoostCost;
        }

        if (potionLabel == "DOUBLE JUMP")
        {
            return doubleJumpCost;
        }

        if (potionLabel == "STRENGTH")
        {
            return strengthCost;
        }

        return 0;
    }

    string GetCoinPriceText(int cost)
    {
        if (cost == 1)
        {
            return "1 COIN";
        }

        return cost + " COINS";
    }
}
