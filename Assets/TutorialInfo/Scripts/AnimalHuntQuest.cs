using UnityEngine;
using System.Collections.Generic;

public class AnimalHuntQuest : MonoBehaviour
{
    // --------------------
    // Nastaveni z Inspectoru
    // --------------------
    public bool findAnimalsAutomatically = true;
    public EnemyHealth[] animalsToKill;
    public GameObject[] objectsToHideWhenCompleted;
    public float completeDelay = 0f;

    // --------------------
    // Stav behem hry
    // --------------------
    public int killedAnimals;
    public int requiredAnimals;

    private bool questCompleted;
    private float completeTimer;
    private bool[] animalKilledStates;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        if (findAnimalsAutomatically)
        {
            FindAnimalsInScene();
        }

        PrepareAnimalTracking();
    }

    void Update()
    {
        if (questCompleted)
        {
            return;
        }

        killedAnimals = CountKilledAnimals();

        if (requiredAnimals <= 0 || killedAnimals < requiredAnimals)
        {
            return;
        }

        if (completeDelay > 0f)
        {
            completeTimer += Time.deltaTime;

            if (completeTimer < completeDelay)
            {
                return;
            }
        }

        CompleteQuest();
    }

    // --------------------
    // Automaticke hledani zvirat
    // --------------------
    void FindAnimalsInScene()
    {
        EnemyHealth[] allEnemyHealth = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        List<EnemyHealth> foundAnimals = new List<EnemyHealth>();

        for (int i = 0; i < allEnemyHealth.Length; i++)
        {
            if (IsQuestAnimal(allEnemyHealth[i]))
            {
                foundAnimals.Add(allEnemyHealth[i]);
            }
        }

        animalsToKill = foundAnimals.ToArray();
    }

    bool IsQuestAnimal(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null)
        {
            return false;
        }

        if (enemyHealth.isAnimal)
        {
            return true;
        }

        return enemyHealth.GetComponent<AnimalMovement>() != null;
    }

    void PrepareAnimalTracking()
    {
        requiredAnimals = CountRequiredAnimals();

        if (animalsToKill == null)
        {
            animalKilledStates = new bool[0];
            return;
        }

        animalKilledStates = new bool[animalsToKill.Length];
    }

    // --------------------
    // Kontrola zvirat
    // --------------------
    int CountRequiredAnimals()
    {
        if (animalsToKill == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < animalsToKill.Length; i++)
        {
            if (animalsToKill[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    int CountKilledAnimals()
    {
        if (animalsToKill == null)
        {
            return 0;
        }

        if (animalKilledStates == null || animalKilledStates.Length != animalsToKill.Length)
        {
            animalKilledStates = new bool[animalsToKill.Length];
        }

        int count = 0;

        for (int i = 0; i < animalsToKill.Length; i++)
        {
            if (animalKilledStates[i])
            {
                count++;
                continue;
            }

            EnemyHealth animalHealth = animalsToKill[i];

            if (animalHealth == null)
            {
                continue;
            }

            if (animalHealth.health <= 0)
            {
                animalKilledStates[i] = true;
                count++;
            }
        }

        return count;
    }

    // --------------------
    // Dokonceni questu
    // --------------------
    void CompleteQuest()
    {
        questCompleted = true;
        HideCompletedObjects();
        enabled = false;
    }

    void HideCompletedObjects()
    {
        if (objectsToHideWhenCompleted == null)
        {
            return;
        }

        for (int i = 0; i < objectsToHideWhenCompleted.Length; i++)
        {
            if (objectsToHideWhenCompleted[i] != null)
            {
                objectsToHideWhenCompleted[i].SetActive(false);
            }
        }
    }
}
