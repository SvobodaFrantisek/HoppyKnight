using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BloomingLilypadManager : MonoBehaviour
{
    // --------------------
    // Napojeni z Inspectoru
    // --------------------
    public TMP_Text progressText;
    public List<BloomingLilypad> lilypads = new List<BloomingLilypad>();
    public List<GameObject> rewardCoins = new List<GameObject>();

    // --------------------
    // Nastaveni
    // --------------------
    public string textPrefix = "Lekniny: ";
    public bool autoFindLilypadsIfListIsEmpty = true;
    public bool hideRewardCoinsOnStart = true;

    // --------------------
    // Stav behem hry
    // --------------------
    private List<BloomingLilypad> countedLilypads = new List<BloomingLilypad>();
    private bool rewardCoinsShown;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        PrepareLilypads();

        if (hideRewardCoinsOnStart)
        {
            SetRewardCoinsActive(false);
        }

        RefreshProgressText();
    }

    // --------------------
    // API pro lekniny
    // --------------------
    public void RegisterLilypad(BloomingLilypad lilypad)
    {
        if (lilypad == null)
        {
            return;
        }

        if (!lilypads.Contains(lilypad))
        {
            Debug.LogWarning(lilypad.gameObject.name + " neni v seznamu manageru.");
            return;
        }

        if (countedLilypads.Contains(lilypad))
        {
            return;
        }

        countedLilypads.Add(lilypad);
        RefreshProgressText();

        if (AllLilypadsCounted())
        {
            ShowRewardCoins();
        }
    }

    // --------------------
    // Priprava sceny
    // --------------------
    void PrepareLilypads()
    {
        RemoveEmptyLilypadSlots();

        if (lilypads.Count == 0 && autoFindLilypadsIfListIsEmpty)
        {
            lilypads.AddRange(FindObjectsByType<BloomingLilypad>(FindObjectsSortMode.None));
        }

        int i = 0;

        while (i < lilypads.Count)
        {
            if (lilypads[i] != null)
            {
                lilypads[i].SetManager(this);
            }

            i++;
        }
    }

    void RemoveEmptyLilypadSlots()
    {
        int i = lilypads.Count - 1;

        while (i >= 0)
        {
            if (lilypads[i] == null)
            {
                lilypads.RemoveAt(i);
            }

            i--;
        }
    }

    // --------------------
    // Text v canvasu
    // --------------------
    void RefreshProgressText()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.text = textPrefix + countedLilypads.Count + "/" + lilypads.Count;
    }

    // --------------------
    // Odmena po splneni
    // --------------------
    bool AllLilypadsCounted()
    {
        if (lilypads.Count == 0)
        {
            return false;
        }

        return countedLilypads.Count >= lilypads.Count;
    }

    void ShowRewardCoins()
    {
        if (rewardCoinsShown)
        {
            return;
        }

        rewardCoinsShown = true;
        SetRewardCoinsActive(true);
    }

    void SetRewardCoinsActive(bool activeState)
    {
        int i = 0;

        while (i < rewardCoins.Count)
        {
            if (rewardCoins[i] != null)
            {
                rewardCoins[i].SetActive(activeState);
            }

            i++;
        }
    }
}
