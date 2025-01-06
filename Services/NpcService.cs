using Models;

namespace Services;

public interface INpcService
{
    Character GenerateNpc(List<Character> characters);
}

public class NpcService : INpcService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _diceService;
    private readonly IValidatorService _validatorService;

    public NpcService(
        ISnapshot snapshot,
        IDiceService diceService,
        IValidatorService validatorService)
    {
        _snapshot = snapshot;
        _diceService = diceService;
        _validatorService = validatorService;
    }

    public Character GenerateNpc(List<Character> characters)
    {
        _validatorService.ValidateListOfCharacters(characters);

        var character = _validatorService.ValidateCharacterExists(characters[0].Identity);

        var npc = new Character
        {
            Identity = new CharacterIdentity
            {
                Id = Guid.NewGuid(),
                PlayerId = Guid.Empty,
            },
            Details = new CharacterDetails
            {
                IsAlive = true,
                Race = "npc",
                Culture = "npc",
                IsHidden = false,
                Entitylevel = 1,
                IsLocked = true,
                IsNpc = true,
                Levelup = 0,
                Name = $"npc_{DateTime.Now.Ticks.ToString()[..5]}",
                Portrait = "https://i.pinimg.com/originals/29/32/63/293263670b8780146ab0c4e40a2ea890.gif",
                Worth = 0,
                BoardId = character.Details.BoardId,
                BoardType = character.Details.BoardType,
            },
        };

        SetNpcSpec(npc);
        SetNpcWealth(npc);
        SetNpcActuals(npc, characters);

        _snapshot.Npcs.Add(npc);

        return npc;
    }

    #region private methods
    private void SetNpcSpec(Character npc)
    {
        var roll = _diceService.Roll1dN(3);

        switch (roll)
        {
            case 1:
                npc.Details.Spec = Statics.Specs.Warring;
                break;
            case 2:
                npc.Details.Spec = Statics.Specs.Sorcery;
                break;
            case 3:
                npc.Details.Spec = Statics.Specs.Tracking;
                break;
            default:
                break;
        }
    }

    private void SetNpcWealth(Character npc)
    {
        npc.Details.Wealth += _diceService.Roll1dN(50);
    }

    private void SetNpcActuals(Character npc, List<Character> characters)
    {
        var board = _snapshot.Boards.Find(s => s.Id == npc.Details.BoardId)!;

        if (board.EffortLevelName == Statics.EffortLevelNames.Easy)
        {
            SetNpcActualsToEasy(npc);
        }
        else if (board.EffortLevelName == Statics.EffortLevelNames.Normal)
        {
            SetNpcActualsToNormal(npc, characters);
        }
        else
        {
            SetNpcActualsToCore(npc, characters);
        }
    }
    
    private void SetNpcActualsToCore(Character npc, List<Character> characters)
    {
        // main
        npc.Stats.Actual.Strength       = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Strength).Min() / 2, characters.Select(s => s.Stats.Actual.Strength).Max() + characters.Select(s => s.Stats.Actual.Strength).Max() / 2);
        npc.Stats.Actual.Constitution   = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Constitution).Min() / 2, characters.Select(s => s.Stats.Actual.Constitution).Max() + characters.Select(s => s.Stats.Actual.Constitution).Max() / 2);
        npc.Stats.Actual.Agility        = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Agility).Min() / 2, characters.Select(s => s.Stats.Actual.Agility).Max() + characters.Select(s => s.Stats.Actual.Agility).Max() / 2);
        npc.Stats.Actual.Willpower      = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Willpower).Min() / 2, characters.Select(s => s.Stats.Actual.Willpower).Max() + characters.Select(s => s.Stats.Actual.Willpower).Max() / 2);
        npc.Stats.Actual.Abstract       = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Abstract).Min() / 2, characters.Select(s => s.Stats.Actual.Abstract).Max() + characters.Select(s => s.Stats.Actual.Abstract).Max() / 2);
        // skills                                                                       Stats.Actual
        npc.Stats.Actual.Melee          = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Melee).Min() / 2, characters.Select(s => s.Stats.Actual.Melee).Max() + characters.Select(s => s.Stats.Actual.Melee).Max() / 2);
        npc.Stats.Actual.Arcane         = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Arcane).Min() / 2, characters.Select(s => s.Stats.Actual.Arcane).Max() + characters.Select(s => s.Stats.Actual.Arcane).Max() / 2);
        npc.Stats.Actual.Psionics       = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Psionics).Min() / 2, characters.Select(s => s.Stats.Actual.Psionics).Max() + characters.Select(s => s.Stats.Actual.Psionics).Max() / 2);
        npc.Stats.Actual.Social         = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Social).Min() / 2, characters.Select(s => s.Stats.Actual.Social).Max() + characters.Select(s => s.Stats.Actual.Social).Max() / 2);
        npc.Stats.Actual.Hide           = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Hide).Min() / 2, characters.Select(s => s.Stats.Actual.Hide).Max() + characters.Select(s => s.Stats.Actual.Hide).Max() / 2);
        npc.Stats.Actual.Survival       = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Survival).Min() / 2, characters.Select(s => s.Stats.Actual.Survival).Max() + characters.Select(s => s.Stats.Actual.Survival).Max() / 2);
        npc.Stats.Actual.Tactics        = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Tactics).Min() / 2, characters.Select(s => s.Stats.Actual.Tactics).Max() + characters.Select(s => s.Stats.Actual.Tactics).Max() / 2);
        npc.Stats.Actual.Aid            = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Aid).Min() / 2, characters.Select(s => s.Stats.Actual.Aid).Max() + characters.Select(s => s.Stats.Actual.Aid).Max() / 2);
        npc.Stats.Actual.Crafting       = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Crafting).Min() / 2, characters.Select(s => s.Stats.Actual.Crafting).Max() + characters.Select(s => s.Stats.Actual.Crafting).Max() / 2);
        npc.Stats.Actual.Perception           = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Perception).Min() / 2, characters.Select(s => s.Stats.Actual.Perception).Max() + characters.Select(s => s.Stats.Actual.Perception).Max() / 2);
        // assets                                                                       Stats.Actual
        npc.Stats.Actual.Defense        = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Defense).Min() / 2, characters.Select(s => s.Stats.Actual.Defense).Max() + characters.Select(s => s.Stats.Actual.Defense).Max() / 2);
        npc.Stats.Actual.Actions        = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Actions).Min() / 2, characters.Select(s => s.Stats.Actual.Actions).Max() + characters.Select(s => s.Stats.Actual.Actions).Max() / 2);
        npc.Stats.Actual.Hitpoints      = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Hitpoints).Min() / 2, characters.Select(s => s.Stats.Actual.Hitpoints).Max() + characters.Select(s => s.Stats.Actual.Hitpoints).Max() / 2);
        npc.Stats.Actual.Mana           = _diceService.RollMdN(characters.Select(s => s.Stats.Actual.Mana).Min() / 2, characters.Select(s => s.Stats.Actual.Mana).Max() + characters.Select(s => s.Stats.Actual.Mana).Max() / 2);
    }

    private void SetNpcActualsToNormal(Character npc, List<Character> characters)
    {
        // main
        npc.Stats.Actual.Strength       = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Strength).Sum() / characters.Count);
        npc.Stats.Actual.Constitution   = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Constitution).Sum() / characters.Count);
        npc.Stats.Actual.Agility        = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Agility).Sum() / characters.Count);
        npc.Stats.Actual.Willpower      = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Willpower).Sum() / characters.Count);
        npc.Stats.Actual.Abstract       = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Abstract).Sum() / characters.Count);
        // skills                                                                       Stats.Actual
        npc.Stats.Actual.Melee          = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Melee).Sum() / characters.Count);
        npc.Stats.Actual.Arcane         = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Arcane).Sum() / characters.Count);
        npc.Stats.Actual.Psionics       = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Psionics).Sum() / characters.Count);
        npc.Stats.Actual.Social         = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Social).Sum() / characters.Count);
        npc.Stats.Actual.Hide           = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Hide).Sum() / characters.Count);
        npc.Stats.Actual.Survival       = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Survival).Sum() / characters.Count);
        npc.Stats.Actual.Tactics        = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Tactics).Sum() / characters.Count);
        npc.Stats.Actual.Aid            = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Aid).Sum() / characters.Count);
        npc.Stats.Actual.Crafting       = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Crafting).Sum() / characters.Count);
        npc.Stats.Actual.Perception           = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Perception).Sum() / characters.Count);
        // assets                                                                       Stats.Actual
        npc.Stats.Actual.Defense        = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Defense).Sum() / characters.Count);
        npc.Stats.Actual.Actions        = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Actions).Sum() / characters.Count); 
        npc.Stats.Actual.Hitpoints      = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Hitpoints).Sum() / characters.Count);
        npc.Stats.Actual.Mana           = _diceService.Roll1dN(characters.Select(s => s.Stats.Actual.Mana).Sum() / characters.Count);
    }

    private void SetNpcActualsToEasy(Character npc)
    {
        // main
        npc.Stats.Actual.Strength       = _diceService.Roll1dN(5);
        npc.Stats.Actual.Constitution   = _diceService.Roll1dN(5);
        npc.Stats.Actual.Agility        = _diceService.Roll1dN(5);
        npc.Stats.Actual.Willpower      = _diceService.Roll1dN(5);
        npc.Stats.Actual.Abstract       = _diceService.Roll1dN(5);
        // skills                                              5
        npc.Stats.Actual.Melee          = _diceService.Roll1dN(5);
        npc.Stats.Actual.Arcane         = _diceService.Roll1dN(5);
        npc.Stats.Actual.Psionics       = _diceService.Roll1dN(5);
        npc.Stats.Actual.Social         = _diceService.Roll1dN(5);
        npc.Stats.Actual.Hide           = _diceService.Roll1dN(5);
        npc.Stats.Actual.Survival       = _diceService.Roll1dN(5);
        npc.Stats.Actual.Tactics        = _diceService.Roll1dN(5);
        npc.Stats.Actual.Aid            = _diceService.Roll1dN(5);
        npc.Stats.Actual.Crafting       = _diceService.Roll1dN(5);
        npc.Stats.Actual.Perception           = _diceService.Roll1dN(5);
        // assets                                              5
        npc.Stats.Actual.Defense        = _diceService.Roll1dN(5);
        npc.Stats.Actual.Actions        = 1;                   
        npc.Stats.Actual.Hitpoints      = _diceService.Roll1dN(5);
        npc.Stats.Actual.Mana           = _diceService.Roll1dN(5);
    }


    #endregion
}
