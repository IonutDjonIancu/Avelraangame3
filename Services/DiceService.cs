using Models;

namespace Services;

public interface IDiceService
{
    /// <summary>
    /// 1d20 roll with reroll.
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    int Rolld20(int total = 0);

    /// <summary>
    /// 1d20 roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int Rolld20NoReroll();

    /// <summary>
    /// 1dN roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int Roll1dN(int n);

    /// <summary>
    /// 1d4 roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int Roll1d4();

    /// <summary>
    /// MdN roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int RollMdN(int m, int n);

    /// <summary>
    /// Character roll d20 using Actuals.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="stat"></param>
    /// <param name="canLevelup"></param>
    /// <returns></returns>
    int RollForCharacter(Character character, string stat, bool canLevelup = false);

    /// <summary>
    /// Character roll d20 using Fights.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="stat"></param>
    /// <param name="canLevelup"></param>
    /// <returns></returns>
    int RollForCharacterVm(CharacterVm character, string stat, bool canLevelup = false);

    int RollForEffort(Board board, string stat);

}

public class DiceService : IDiceService
{
    private readonly IValidatorService _validator;

    static readonly Random random = new();

    public DiceService(IValidatorService validator)
    {
        _validator = validator; 
    }

    public int RollForCharacter(Character character, string stat, bool canLevelup = false)
    {
        var roll = Rolld20();

        if (canLevelup && roll >= 20)
        {
            UpgradeEntityLevel(character, roll);
            LevelUp(character);
        }

        return stat switch
        {
            Statics.Stats.Strength      => roll + character.Stats.Actuals.Strength,
            Statics.Stats.Constitution  => roll + character.Stats.Actuals.Constitution,
            Statics.Stats.Agility       => roll + character.Stats.Actuals.Agility,
            Statics.Stats.Willpower     => roll + character.Stats.Actuals.Willpower,
            Statics.Stats.Abstract      => roll + character.Stats.Actuals.Abstract,
            Statics.Stats.Melee         => roll + character.Stats.Actuals.Melee,
            Statics.Stats.Arcane        => roll + character.Stats.Actuals.Arcane,
            Statics.Stats.Psionics      => roll + character.Stats.Actuals.Psionics,
            Statics.Stats.Social        => roll + character.Stats.Actuals.Social,
            Statics.Stats.Hide          => roll + character.Stats.Actuals.Hide,
            Statics.Stats.Survival      => roll + character.Stats.Actuals.Survival,
            Statics.Stats.Tactics       => roll + character.Stats.Actuals.Tactics,
            Statics.Stats.Aid           => roll + character.Stats.Actuals.Aid,
            Statics.Stats.Crafting      => roll + character.Stats.Actuals.Crafting,
            Statics.Stats.Perception    => roll + character.Stats.Actuals.Perception,
            Statics.Stats.Defense       => roll + character.Stats.Actuals.Defense,
            Statics.Stats.Actions       => roll + character.Stats.Actuals.Actions,
            Statics.Stats.Endurance     => roll + character.Stats.Actuals.Endurance,
            Statics.Stats.Accretion     => roll + character.Stats.Actuals.Accretion,
            _ => throw new Exception("Wrong stat provided.")
        };
    }

    public int RollForCharacterVm(CharacterVm characterVm, string stat, bool canLevelup = false)
    {
        var roll = Rolld20();

        if (canLevelup && roll >= 20)
        {
            UpgradeEntityLevel(characterVm, roll);
            LevelUp(characterVm);
        }

        return stat switch
        {
            Statics.Stats.Strength      => roll + characterVm.Stats.Fights.Strength,
            Statics.Stats.Constitution  => roll + characterVm.Stats.Fights.Constitution,
            Statics.Stats.Agility       => roll + characterVm.Stats.Fights.Agility,
            Statics.Stats.Willpower     => roll + characterVm.Stats.Fights.Willpower,
            Statics.Stats.Abstract      => roll + characterVm.Stats.Fights.Abstract,
            Statics.Stats.Melee         => roll + characterVm.Stats.Fights.Melee,
            Statics.Stats.Arcane        => roll + characterVm.Stats.Fights.Arcane,
            Statics.Stats.Psionics      => roll + characterVm.Stats.Fights.Psionics,
            Statics.Stats.Social        => roll + characterVm.Stats.Fights.Social,
            Statics.Stats.Hide          => roll + characterVm.Stats.Fights.Hide,
            Statics.Stats.Survival      => roll + characterVm.Stats.Fights.Survival,
            Statics.Stats.Tactics       => roll + characterVm.Stats.Fights.Tactics,
            Statics.Stats.Aid           => roll + characterVm.Stats.Fights.Aid,
            Statics.Stats.Crafting      => roll + characterVm.Stats.Fights.Crafting,
            Statics.Stats.Perception    => roll + characterVm.Stats.Fights.Perception,
            Statics.Stats.Defense       => roll + characterVm.Stats.Fights.Defense,
            Statics.Stats.Actions       => roll + characterVm.Stats.Fights.Actions,
            Statics.Stats.Endurance     => roll + characterVm.Stats.Fights.Endurance,
            Statics.Stats.Accretion     => roll + characterVm.Stats.Fights.Accretion,
            _ => throw new Exception("Wrong stat provided.")
        };
    }

    public int RollForEffort(Board board, string stat)
    {
        if (string.IsNullOrWhiteSpace(board.EffortLevelName) || board.EffortLevelName == Statics.EffortLevelNames.Easy)
        {
            return Rolld20NoReroll();
        }
        else if (board.EffortLevelName == Statics.EffortLevelNames.Normal)
        {
            return stat switch
            {
                Statics.Stats.Strength     => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength    ).Sum()),
                Statics.Stats.Constitution => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution).Sum()),
                Statics.Stats.Agility      => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility     ).Sum()),
                Statics.Stats.Willpower    => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower   ).Sum()),
                Statics.Stats.Abstract     => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract    ).Sum()),
                Statics.Stats.Melee        => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee       ).Sum()),
                Statics.Stats.Arcane       => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane      ).Sum()),
                Statics.Stats.Psionics     => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics    ).Sum()),
                Statics.Stats.Social       => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social      ).Sum()),
                Statics.Stats.Hide         => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide        ).Sum()),
                Statics.Stats.Survival     => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival    ).Sum()),
                Statics.Stats.Tactics      => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics     ).Sum()),
                Statics.Stats.Aid          => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid         ).Sum()),
                Statics.Stats.Crafting     => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting    ).Sum()),
                Statics.Stats.Perception   => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception  ).Sum()),
                Statics.Stats.Defense      => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense     ).Sum()),
                Statics.Stats.Actions      => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions     ).Sum()),
                Statics.Stats.Endurance    => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance   ).Sum()),
                Statics.Stats.Accretion    => Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion   ).Sum()),
                _ => throw new Exception("Wrong stat provided."),
            };
        }
        else // core rules
        {
            return stat switch
            {
                Statics.Stats.Strength     => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength     ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength     + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength     ).Sum() / 2).Sum() ),
                Statics.Stats.Constitution => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution ).Sum() / 2).Sum() ),
                Statics.Stats.Agility      => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility      ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility      + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility      ).Sum() / 2).Sum() ),
                Statics.Stats.Willpower    => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower    ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower    + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower    ).Sum() / 2).Sum() ),
                Statics.Stats.Abstract     => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract     ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract     + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract     ).Sum() / 2).Sum() ),
                Statics.Stats.Melee        => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee        ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee        + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee        ).Sum() / 2).Sum() ),
                Statics.Stats.Arcane       => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane       ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane       + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane       ).Sum() / 2).Sum() ),
                Statics.Stats.Psionics     => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics     ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics     + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics     ).Sum() / 2).Sum() ),
                Statics.Stats.Social       => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social       ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social       + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social       ).Sum() / 2).Sum() ),
                Statics.Stats.Hide         => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide         ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide         + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide         ).Sum() / 2).Sum() ),
                Statics.Stats.Survival     => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival     ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival     + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival     ).Sum() / 2).Sum() ),
                Statics.Stats.Tactics      => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics      ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics      + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics      ).Sum() / 2).Sum() ),
                Statics.Stats.Aid          => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid          ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid          + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid          ).Sum() / 2).Sum() ),
                Statics.Stats.Crafting     => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting     ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting     + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting     ).Sum() / 2).Sum() ),
                Statics.Stats.Perception   => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception   ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception   + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception   ).Sum() / 2).Sum() ),
                Statics.Stats.Defense      => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense      ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense      + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense      ).Sum() / 2).Sum() ),
                Statics.Stats.Actions      => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions      ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions      + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions      ).Sum() / 2).Sum() ),
                Statics.Stats.Endurance    => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance    ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance    + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance    ).Sum() / 2).Sum() ),
                Statics.Stats.Accretion    => RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion    ).Sum() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion    + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion    ).Sum() / 2).Sum() ),
                _ => throw new Exception("Wrong stat provided."),
            };
        }
    }

    public int Roll1d4()
    {
        return random.Next(1, 5);
    }

    public int Roll1dN(int n)
    {
        return random.Next(1, n + 1);   
    }

    public int RollMdN(int m, int n)
    {
        _validator.ValidateGreaterNumber(m, n);

        return random.Next(m, n + 1);
    }

    public int Rolld20NoReroll()
    {
        return random.Next(1, 21);
    }

    public int Rolld20(int total = 0)
    {
        total += random.Next(1, 21);

        if (total % 20 == 0)
        {
            return Rolld20(total);
        }

        return total;
    }

    #region private methods
    private static void UpgradeEntityLevel(Character character, int roll)
    {
        if (roll >= 20 && character.Details.EntityLevel < 2)
        {
            character.Details.EntityLevel = 2;
        }
        else if (roll >= 40 && character.Details.EntityLevel < 3)
        {
            character.Details.EntityLevel = 3;
        }
        else if (roll >= 60 && character.Details.EntityLevel < 4)
        {
            character.Details.EntityLevel = 4;
        }
        else if (roll >= 80 && character.Details.EntityLevel < 5)
        {
            character.Details.EntityLevel = 5;
        }
        else if (roll >= 100 && character.Details.EntityLevel < 6)
        {
            character.Details.EntityLevel = 6;
        }
    }

    private static void LevelUp(Character character)
    {
        character.Details.Levelup += 2 * character.Details.EntityLevel;
    }

    private static void UpgradeEntityLevel(CharacterVm character, int roll)
    {
        if (roll >= 20 && character.Details.EntityLevel < 2)
        {
            character.Details.EntityLevel = 2;
        }
        else if (roll >= 40 && character.Details.EntityLevel < 3)
        {
            character.Details.EntityLevel = 3;
        }
        else if (roll >= 60 && character.Details.EntityLevel < 4)
        {
            character.Details.EntityLevel = 4;
        }
        else if (roll >= 80 && character.Details.EntityLevel < 5)
        {
            character.Details.EntityLevel = 5;
        }
        else if (roll >= 100 && character.Details.EntityLevel < 6)
        {
            character.Details.EntityLevel = 6;
        }
    }

    private static void LevelUp(CharacterVm character)
    {
        character.Details.Levelup += 2 * character.Details.EntityLevel;
    }

    #endregion
}
