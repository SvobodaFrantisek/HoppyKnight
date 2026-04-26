using UnityEngine;

public class ShopkeeperScript : MonoBehaviour
{
    public GameObject shopUI;
    public GameManager gameManager;
    public PotionInventory potionInventory;

    public int healthCost = 0;
    public int speedCost = 0;
    public int jumpBoostCost = 0;
    public int doubleJumpCost = 0;
    public int strengthCost = 0;

    public bool playerInRange = false;

    void Start()
    {
        CacheReferences();

        if (shopUI != null)
        {
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

    bool TryBuyPotion(PotionType potionType, int cost)
    {
        CacheReferences();

        if (!HasRequiredReferences())
        {
            return false;
        }

        if (!potionInventory.HasFreeSlot())
        {
            Debug.Log("Inventar potionu je plny.");
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

    void SetShopVisible(bool visibleState)
    {
        if (shopUI == null)
        {
            return;
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
}
