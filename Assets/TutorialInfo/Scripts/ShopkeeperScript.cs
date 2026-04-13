using UnityEngine;

public class ShopkeeperScript : MonoBehaviour
{
    public GameObject shopUI;
    public GameManager gameManager;
    public PotionInventory potionInventory;
    public bool playerInRange = false;

    public int healthCost = 0;
    public int speedCost = 0;
    public int jumpBoostCost = 0;
    public int doubleJumpCost = 0;
    public int strengthCost = 0;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (potionInventory == null)
        {
            potionInventory = FindAnyObjectByType<PotionInventory>();
        }

        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }
        else
        {
            Debug.LogError("shop panel neni prirazen");
        }

    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInRange == true)
        {
            OpenShop();
        }

    }

    public void OpenShop()
    {
        if (shopUI == null)
        {
            return;
        }
        shopUI.SetActive(!shopUI.activeSelf);
        if (shopUI.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (shopUI != null && shopUI.activeSelf)
            {
                OpenShop();
            }
        }
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

    private bool TryBuyPotion(PotionType potionType, int cost)
    {
        if (potionInventory == null)
        {
            potionInventory = FindAnyObjectByType<PotionInventory>();
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

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

        if (!potionInventory.HasFreeSlot())
        {
            Debug.Log("Inventar potionu je plny.");
            return false;
        }

        bool spendSuccess = gameManager.TrySpendCoins(cost);

        if (!spendSuccess)
        {
            return false;
        }

        bool addSuccess = potionInventory.TryAddPotion(potionType);

        if (!addSuccess)
        {
            Debug.LogWarning("Potion se nepodarilo vlozit do inventare.");
            return false;
        }

        return true;
    }
}

