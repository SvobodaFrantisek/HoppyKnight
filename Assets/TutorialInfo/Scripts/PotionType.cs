// Vsechny typy potionu, ktere muze inventar drzet ve slotech.
public enum PotionType
{
    None = 0,
    Speed = 1,
    Heal = 2,
    Strength = 3,
    JumpBoost = 4,
    DoubleJump = 5
}

public static class PotionTypeUtility
{
    // Text, ktery se ukazuje v jednoduchem HUD slotu.
    public static string GetDisplayName(PotionType potionType)
    {
        if (potionType == PotionType.Speed)
        {
            return "SPEED";
        }

        if (potionType == PotionType.Heal)
        {
            return "HEAL";
        }

        if (potionType == PotionType.Strength)
        {
            return "STRENGTH";
        }

        if (potionType == PotionType.JumpBoost)
        {
            return "JUMP";
        }

        if (potionType == PotionType.DoubleJump)
        {
            return "DOUBLE";
        }

        return "-";
    }
}
