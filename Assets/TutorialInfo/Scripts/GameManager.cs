using UnityEngine;

public class GameManager : MonoBehaviour
{
    public HUDmanager hudManager;
    public int coins = 0;
    public int gems = 0;
    public int remainGems = 10;
    public int health = 100;
    public int maxHealth = 100;

    void Start()
    {
        CacheHudManager();

        if (hudManager != null)
        {
            RefreshAllHud();
        }
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        RefreshCoinsHud();
        Debug.Log("Coins: " + coins);
    }

    public void AddGem(int amount)
    {
        gems += amount;
        RefreshGemsHud();
        Debug.Log("Gems: " + gems);
    }

    public bool TrySpendCoins(int cost)
    {
        if (coins < cost)
        {
            Debug.Log("Nemas dostatek minci na koupi tohoto itemu.");
            return false;
        }

        coins -= cost;
        RefreshCoinsHud();
        Debug.Log("Item koupen, zustatek minci: " + coins);
        return true;
    }

    public void Heal(int amount)
    {
        health += amount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        RefreshHealthHud();
    }

    // Legacy wrapper ponechany kvuli stavajicim referencim.
    public void addGem(int amount)
    {
        AddGem(amount);
    }

    // Legacy wrapper ponechany kvuli stavajicim referencim.
    public void buyItem(int cost)
    {
        TrySpendCoins(cost);
    }

    // Legacy wrapper ponechany kvuli stavajicim referencim.
    public int getCoins()
    {
        return GetCoins();
    }

    public int GetCoins()
    {
        return coins;
    }

    void CacheHudManager()
    {
        if (hudManager != null)
        {
            return;
        }

        hudManager = FindAnyObjectByType<HUDmanager>();
    }

    void RefreshAllHud()
    {
        RefreshCoinsHud();
        RefreshGemsHud();
        RefreshHealthHud();
    }

    void RefreshCoinsHud()
    {
        if (hudManager != null)
        {
            hudManager.UpdateCoins(coins);
            return;
        }

        Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni minci.");
    }

    void RefreshGemsHud()
    {
        if (hudManager != null)
        {
            hudManager.UpdateGems(gems, remainGems);
            return;
        }

        Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni gemu.");
    }

    void RefreshHealthHud()
    {
        if (hudManager != null)
        {
            hudManager.UpdateHealth(health);
            return;
        }

        Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni zdravi.");
    }
}
