using UnityEngine;

public class GameManager : MonoBehaviour
{
    // --------------------
    // Nastaveni z Inspectoru
    // --------------------
    public HUDmanager hudManager;
    public int coins = 0;
    public int gems = 0;
    public int remainGems = 10;
    public int health = 100;
    public int maxHealth = 100;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        CacheHudManager();

        if (hudManager != null)
        {
            RefreshAllHud();
        }
    }

    // --------------------
    // Meny a sbirani
    // --------------------
    public void AddCoin(int amount)
    {
        coins += amount;
        RefreshCoinsHud();
    }

    public void AddGem(int amount)
    {
        gems += amount;
        RefreshGemsHud();
    }

    public bool TrySpendCoins(int cost)
    {
        if (coins < cost)
        {
            return false;
        }

        coins -= cost;
        RefreshCoinsHud();
        return true;
    }

    // --------------------
    // Zdravi hrace
    // --------------------
    public void Heal(int amount)
    {
        health += amount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        RefreshHealthHud();
    }

    public void TakeDamage(int amount)
    {
        if (health <= 0)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        health -= amount;

        if (health < 0)
        {
            health = 0;
        }

        RefreshHealthHud();
    }

    // --------------------
    // HUD helper metody
    // --------------------
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
            hudManager.UpdateHealth(health, maxHealth);
            return;
        }

        Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni zdravi.");
    }
}
