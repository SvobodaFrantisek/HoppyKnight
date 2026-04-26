using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PotionInventory : MonoBehaviour
{
    [Header("Sloty")]
    public PotionType[] slots = new PotionType[3];

    [Header("Objekty v ruce")]
    public GameObject swordModel;
    public GameObject speedPotionModel;
    public GameObject healPotionModel;
    public GameObject strengthPotionModel;
    public GameObject jumpBoostPotionModel;
    public GameObject doubleJumpPotionModel;

    [Header("Piti")]
    public float drinkDuration = 1.2f;
    public string drinkTriggerName = "Drink";

    [Header("Efekty potionu")]
    public float speedPotionDuration = 5f;
    public float speedPotionMoveBonus = 4f;
    public float jumpBoostDuration = 5f;
    public float jumpBoostValue = 4f;
    public float strengthPotionDuration = 5f;
    public float strengthAttackCooldown = 0.25f;
    public float doubleJumpDuration = 8f;
    public int healAmount = 25;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private HUDmanager hudManager;
    private GameManager gameManager;
    private FPSController fpsController;
    private CombatScript combatScript;
    private Animator animator;

    private int selectedSlotIndex = -1;
    private bool isDrinking;

    private float baseMoveSpeed;
    private float baseJumpSpeed;
    private float baseAttackCooldown;
    private bool baseHasDoubleJump;

    private Coroutine speedCoroutine;
    private Coroutine jumpCoroutine;
    private Coroutine strengthCoroutine;
    private Coroutine doubleJumpCoroutine;

    void Awake()
    {
        CacheReferences();
        CacheBaseStats();
        EnsureSlotsArray();
    }

    void Start()
    {
        EquipSword();
        RefreshHud();
    }

    void OnEnable()
    {
        if (playerInput == null)
        {
            return;
        }

        attackAction = playerInput.actions.FindAction("Attack");

        if (attackAction != null)
        {
            attackAction.Enable();
        }
    }

    void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.Disable();
        }
    }

    void Update()
    {
        if (!CanReadInventoryInput())
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        HandleSlotSelectionInput(keyboard);

        if (HandleReturnToSwordInput(keyboard))
        {
            return;
        }

        HandlePotionUseInput();
    }

    public bool HasFreeSlot()
    {
        return FindFirstFreeSlotIndex() >= 0;
    }

    public bool TryAddPotion(PotionType potionType)
    {
        if (potionType == PotionType.None)
        {
            return false;
        }

        int freeSlotIndex = FindFirstFreeSlotIndex();

        if (freeSlotIndex < 0)
        {
            return false;
        }

        slots[freeSlotIndex] = potionType;
        RefreshHud();
        return true;
    }

    public PotionType[] GetSlots()
    {
        return slots;
    }

    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }

    public bool IsPotionSelected()
    {
        if (selectedSlotIndex < 0)
        {
            return false;
        }

        if (selectedSlotIndex >= slots.Length)
        {
            return false;
        }

        if (slots[selectedSlotIndex] == PotionType.None)
        {
            return false;
        }

        return true;
    }

    void HandleSlotSelectionInput(Keyboard keyboard)
    {
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            SelectSlot(0);
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            SelectSlot(1);
        }

        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            SelectSlot(2);
        }
    }

    bool HandleReturnToSwordInput(Keyboard keyboard)
    {
        if (!keyboard.qKey.wasPressedThisFrame)
        {
            return false;
        }

        EquipSword();
        return true;
    }

    void HandlePotionUseInput()
    {
        if (selectedSlotIndex < 0)
        {
            return;
        }

        if (attackAction == null)
        {
            return;
        }

        if (!attackAction.WasPressedThisFrame())
        {
            return;
        }

        StartCoroutine(DrinkSelectedPotion());
    }

    void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            return;
        }

        if (slots[slotIndex] == PotionType.None)
        {
            return;
        }

        selectedSlotIndex = slotIndex;

        if (combatScript != null)
        {
            combatScript.SetCombatBlocked(true);
        }

        ShowHeldItem(slots[selectedSlotIndex]);
        RefreshHud();
    }

    void EquipSword()
    {
        selectedSlotIndex = -1;

        if (combatScript != null)
        {
            combatScript.SetCombatBlocked(false);
        }

        ShowHeldItem(PotionType.None);
        RefreshHud();
    }

    IEnumerator DrinkSelectedPotion()
    {
        if (!IsPotionSelected())
        {
            yield break;
        }

        int usedSlotIndex = selectedSlotIndex;
        PotionType usedPotion = slots[usedSlotIndex];

        BeginDrinking();

        yield return new WaitForSeconds(drinkDuration);

        ApplyPotionEffect(usedPotion);
        slots[usedSlotIndex] = PotionType.None;

        FinishDrinking();
        EquipSword();
        RefreshHud();
    }

    void BeginDrinking()
    {
        isDrinking = true;

        if (fpsController != null)
        {
            fpsController.SetMovementLocked(true);
        }

        if (combatScript != null)
        {
            combatScript.SetCombatBlocked(true);
        }

        if (animator != null)
        {
            animator.ResetTrigger(drinkTriggerName);
            animator.SetTrigger(drinkTriggerName);
        }
    }

    void FinishDrinking()
    {
        if (fpsController != null)
        {
            fpsController.SetMovementLocked(false);
        }

        isDrinking = false;
    }

    void ApplyPotionEffect(PotionType potionType)
    {
        if (potionType == PotionType.Speed)
        {
            RestartSpeedPotion();
            return;
        }

        if (potionType == PotionType.Heal)
        {
            ApplyHealPotion();
            return;
        }

        if (potionType == PotionType.Strength)
        {
            RestartStrengthPotion();
            return;
        }

        if (potionType == PotionType.JumpBoost)
        {
            RestartJumpPotion();
            return;
        }

        if (potionType == PotionType.DoubleJump)
        {
            RestartDoubleJumpPotion();
        }
    }

    void RestartSpeedPotion()
    {
        if (speedCoroutine != null)
        {
            StopCoroutine(speedCoroutine);
        }

        speedCoroutine = StartCoroutine(SpeedPotionRoutine());
    }

    void RestartJumpPotion()
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
        }

        jumpCoroutine = StartCoroutine(JumpPotionRoutine());
    }

    void RestartStrengthPotion()
    {
        if (strengthCoroutine != null)
        {
            StopCoroutine(strengthCoroutine);
        }

        strengthCoroutine = StartCoroutine(StrengthPotionRoutine());
    }

    void RestartDoubleJumpPotion()
    {
        if (doubleJumpCoroutine != null)
        {
            StopCoroutine(doubleJumpCoroutine);
        }

        doubleJumpCoroutine = StartCoroutine(DoubleJumpPotionRoutine());
    }

    void ApplyHealPotion()
    {
        if (gameManager != null)
        {
            gameManager.Heal(healAmount);
        }
    }

    IEnumerator SpeedPotionRoutine()
    {
        if (fpsController == null)
        {
            yield break;
        }

        fpsController.moveSpeed = baseMoveSpeed + speedPotionMoveBonus;
        yield return new WaitForSeconds(speedPotionDuration);
        fpsController.moveSpeed = baseMoveSpeed;
        speedCoroutine = null;
    }

    IEnumerator JumpPotionRoutine()
    {
        if (fpsController == null)
        {
            yield break;
        }

        fpsController.jumpSpeed = baseJumpSpeed + jumpBoostValue;
        yield return new WaitForSeconds(jumpBoostDuration);
        fpsController.jumpSpeed = baseJumpSpeed;
        jumpCoroutine = null;
    }

    IEnumerator StrengthPotionRoutine()
    {
        if (combatScript == null)
        {
            yield break;
        }

        combatScript.attackCooldown = strengthAttackCooldown;
        yield return new WaitForSeconds(strengthPotionDuration);
        combatScript.attackCooldown = baseAttackCooldown;
        strengthCoroutine = null;
    }

    IEnumerator DoubleJumpPotionRoutine()
    {
        if (fpsController == null)
        {
            yield break;
        }

        fpsController.hasDoubleJump = true;
        yield return new WaitForSeconds(doubleJumpDuration);
        fpsController.hasDoubleJump = baseHasDoubleJump;
        doubleJumpCoroutine = null;
    }

    void ShowHeldItem(PotionType potionType)
    {
        SetAllHeldItemsInactive();

        if (potionType == PotionType.None)
        {
            SetObjectActive(swordModel, true);
            return;
        }

        if (potionType == PotionType.Speed)
        {
            SetObjectActive(speedPotionModel, true);
            return;
        }

        if (potionType == PotionType.Heal)
        {
            SetObjectActive(healPotionModel, true);
            return;
        }

        if (potionType == PotionType.Strength)
        {
            SetObjectActive(strengthPotionModel, true);
            return;
        }

        if (potionType == PotionType.JumpBoost)
        {
            SetObjectActive(jumpBoostPotionModel, true);
            return;
        }

        if (potionType == PotionType.DoubleJump)
        {
            SetObjectActive(doubleJumpPotionModel, true);
        }
    }

    void SetAllHeldItemsInactive()
    {
        SetObjectActive(swordModel, false);
        SetObjectActive(speedPotionModel, false);
        SetObjectActive(healPotionModel, false);
        SetObjectActive(strengthPotionModel, false);
        SetObjectActive(jumpBoostPotionModel, false);
        SetObjectActive(doubleJumpPotionModel, false);
    }

    void SetObjectActive(GameObject target, bool activeState)
    {
        if (target != null)
        {
            target.SetActive(activeState);
        }
    }

    void RefreshHud()
    {
        if (hudManager == null)
        {
            hudManager = FindAnyObjectByType<HUDmanager>();
        }

        if (hudManager != null)
        {
            hudManager.UpdatePotionSlots(slots, selectedSlotIndex);
        }
    }

    bool CanReadInventoryInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return false;
        }

        if (isDrinking)
        {
            return false;
        }

        return Keyboard.current != null;
    }

    int FindFirstFreeSlotIndex()
    {
        int i = 0;

        while (i < slots.Length)
        {
            if (slots[i] == PotionType.None)
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    void CacheReferences()
    {
        playerInput = GetComponent<PlayerInput>();
        fpsController = GetComponent<FPSController>();
        combatScript = GetComponent<CombatScript>();
        animator = GetComponentInChildren<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();
        hudManager = FindAnyObjectByType<HUDmanager>();
    }

    void CacheBaseStats()
    {
        if (fpsController != null)
        {
            baseMoveSpeed = fpsController.moveSpeed;
            baseJumpSpeed = fpsController.jumpSpeed;
            baseHasDoubleJump = fpsController.hasDoubleJump;
        }

        if (combatScript != null)
        {
            baseAttackCooldown = combatScript.attackCooldown;
        }
    }

    void EnsureSlotsArray()
    {
        if (slots == null || slots.Length != 3)
        {
            slots = new PotionType[3];
        }
    }
}
