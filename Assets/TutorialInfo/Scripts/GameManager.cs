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
        hudManager = FindAnyObjectByType<HUDmanager>();

        if (hudManager != null)
        {
            hudManager.UpdateCoins(coins);
            hudManager.UpdateGems(gems, remainGems);
            hudManager.UpdateHealth(health);
        }
    }

    public void AddCoin(int amount)
    {
        coins += amount;

        if (hudManager != null)
        {
            hudManager.UpdateCoins(coins);
        }
        else
        {
            Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni minci.");
        }

        Debug.Log("Coins: " + coins);
    }

    public void addGem(int amount)
    {
        gems += amount;

        if (hudManager != null)
        {
            hudManager.UpdateGems(gems, remainGems);
        }
        else
        {
            Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni gemu.");
        }

        Debug.Log("Gems: " + gems);
    }

    public bool TrySpendCoins(int cost)
    {
        if (coins >= cost)
        {
            coins -= cost;

            if (hudManager != null)
            {
                hudManager.UpdateCoins(coins);
            }
            else
            {
                Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni minci.");
            }

            Debug.Log("Item koupen, zustatek minci: " + coins);
            return true;
        }

        Debug.Log("Nemas dostatek minci na koupi tohoto itemu.");
        return false;
    }

    public void buyItem(int cost)
    {
        TrySpendCoins(cost);
    }

    public int getCoins()
    {
        return coins;
    }

    public void Heal(int amount)
    {
        health += amount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        if (hudManager != null)
        {
            hudManager.UpdateHealth(health);
        }
    }
}
