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
        npc.Actuals.Strength       = _diceService.RollMdN(characters.Select(s => s.Actuals.Strength).Min() / 2, characters.Select(s => s.Actuals.Strength).Max() + characters.Select(s => s.Actuals.Strength).Max() / 2);
        npc.Actuals.Constitution   = _diceService.RollMdN(characters.Select(s => s.Actuals.Constitution).Min() / 2, characters.Select(s => s.Actuals.Constitution).Max() + characters.Select(s => s.Actuals.Constitution).Max() / 2);
        npc.Actuals.Agility        = _diceService.RollMdN(characters.Select(s => s.Actuals.Agility).Min() / 2, characters.Select(s => s.Actuals.Agility).Max() + characters.Select(s => s.Actuals.Agility).Max() / 2);
        npc.Actuals.Willpower      = _diceService.RollMdN(characters.Select(s => s.Actuals.Willpower).Min() / 2, characters.Select(s => s.Actuals.Willpower).Max() + characters.Select(s => s.Actuals.Willpower).Max() / 2);
        npc.Actuals.Abstract       = _diceService.RollMdN(characters.Select(s => s.Actuals.Abstract).Min() / 2, characters.Select(s => s.Actuals.Abstract).Max() + characters.Select(s => s.Actuals.Abstract).Max() / 2);
        // skills
        npc.Actuals.Melee          = _diceService.RollMdN(characters.Select(s => s.Actuals.Melee).Min() / 2, characters.Select(s => s.Actuals.Melee).Max() + characters.Select(s => s.Actuals.Melee).Max() / 2);
        npc.Actuals.Arcane         = _diceService.RollMdN(characters.Select(s => s.Actuals.Arcane).Min() / 2, characters.Select(s => s.Actuals.Arcane).Max() + characters.Select(s => s.Actuals.Arcane).Max() / 2);
        npc.Actuals.Psionics       = _diceService.RollMdN(characters.Select(s => s.Actuals.Psionics).Min() / 2, characters.Select(s => s.Actuals.Psionics).Max() + characters.Select(s => s.Actuals.Psionics).Max() / 2);
        npc.Actuals.Social         = _diceService.RollMdN(characters.Select(s => s.Actuals.Social).Min() / 2, characters.Select(s => s.Actuals.Social).Max() + characters.Select(s => s.Actuals.Social).Max() / 2);
        npc.Actuals.Hide           = _diceService.RollMdN(characters.Select(s => s.Actuals.Hide).Min() / 2, characters.Select(s => s.Actuals.Hide).Max() + characters.Select(s => s.Actuals.Hide).Max() / 2);
        npc.Actuals.Survival       = _diceService.RollMdN(characters.Select(s => s.Actuals.Survival).Min() / 2, characters.Select(s => s.Actuals.Survival).Max() + characters.Select(s => s.Actuals.Survival).Max() / 2);
        npc.Actuals.Tactics        = _diceService.RollMdN(characters.Select(s => s.Actuals.Tactics).Min() / 2, characters.Select(s => s.Actuals.Tactics).Max() + characters.Select(s => s.Actuals.Tactics).Max() / 2);
        npc.Actuals.Aid            = _diceService.RollMdN(characters.Select(s => s.Actuals.Aid).Min() / 2, characters.Select(s => s.Actuals.Aid).Max() + characters.Select(s => s.Actuals.Aid).Max() / 2);
        npc.Actuals.Crafting       = _diceService.RollMdN(characters.Select(s => s.Actuals.Crafting).Min() / 2, characters.Select(s => s.Actuals.Crafting).Max() + characters.Select(s => s.Actuals.Crafting).Max() / 2);
        npc.Actuals.Spot           = _diceService.RollMdN(characters.Select(s => s.Actuals.Spot).Min() / 2, characters.Select(s => s.Actuals.Spot).Max() + characters.Select(s => s.Actuals.Spot).Max() / 2);
        // assets
        npc.Actuals.Defense        = _diceService.RollMdN(characters.Select(s => s.Actuals.Defense).Min() / 2, characters.Select(s => s.Actuals.Defense).Max() + characters.Select(s => s.Actuals.Defense).Max() / 2);
        npc.Actuals.Resist         = _diceService.RollMdN(characters.Select(s => s.Actuals.Resist).Min() / 2, characters.Select(s => s.Actuals.Resist).Max() + characters.Select(s => s.Actuals.Resist).Max() / 2);
        npc.Actuals.Actions        = _diceService.RollMdN(characters.Select(s => s.Actuals.Actions).Min() / 2, characters.Select(s => s.Actuals.Actions).Max() + characters.Select(s => s.Actuals.Actions).Max() / 2);
        npc.Actuals.Endurance      = _diceService.RollMdN(characters.Select(s => s.Actuals.Endurance).Min() / 2, characters.Select(s => s.Actuals.Endurance).Max() + characters.Select(s => s.Actuals.Endurance).Max() / 2);
        npc.Actuals.Accretion      = _diceService.RollMdN(characters.Select(s => s.Actuals.Accretion).Min() / 2, characters.Select(s => s.Actuals.Accretion).Max() + characters.Select(s => s.Actuals.Accretion).Max() / 2);
    }

    private void SetNpcActualsToNormal(Character npc, List<Character> characters)
    {
        // main
        npc.Actuals.Strength       = _diceService.Roll1dN(characters.Select(s => s.Actuals.Strength).Sum() / characters.Count);
        npc.Actuals.Constitution   = _diceService.Roll1dN(characters.Select(s => s.Actuals.Constitution).Sum() / characters.Count);
        npc.Actuals.Agility        = _diceService.Roll1dN(characters.Select(s => s.Actuals.Agility).Sum() / characters.Count);
        npc.Actuals.Willpower      = _diceService.Roll1dN(characters.Select(s => s.Actuals.Willpower).Sum() / characters.Count);
        npc.Actuals.Abstract       = _diceService.Roll1dN(characters.Select(s => s.Actuals.Abstract).Sum() / characters.Count);
        // skills
        npc.Actuals.Melee          = _diceService.Roll1dN(characters.Select(s => s.Actuals.Melee).Sum() / characters.Count);
        npc.Actuals.Arcane         = _diceService.Roll1dN(characters.Select(s => s.Actuals.Arcane).Sum() / characters.Count);
        npc.Actuals.Psionics       = _diceService.Roll1dN(characters.Select(s => s.Actuals.Psionics).Sum() / characters.Count);
        npc.Actuals.Social         = _diceService.Roll1dN(characters.Select(s => s.Actuals.Social).Sum() / characters.Count);
        npc.Actuals.Hide           = _diceService.Roll1dN(characters.Select(s => s.Actuals.Hide).Sum() / characters.Count);
        npc.Actuals.Survival       = _diceService.Roll1dN(characters.Select(s => s.Actuals.Survival).Sum() / characters.Count);
        npc.Actuals.Tactics        = _diceService.Roll1dN(characters.Select(s => s.Actuals.Tactics).Sum() / characters.Count);
        npc.Actuals.Aid            = _diceService.Roll1dN(characters.Select(s => s.Actuals.Aid).Sum() / characters.Count);
        npc.Actuals.Crafting       = _diceService.Roll1dN(characters.Select(s => s.Actuals.Crafting).Sum() / characters.Count);
        npc.Actuals.Spot           = _diceService.Roll1dN(characters.Select(s => s.Actuals.Spot).Sum() / characters.Count);
        // assets
        npc.Actuals.Defense        = _diceService.Roll1dN(characters.Select(s => s.Actuals.Defense).Sum() / characters.Count);
        npc.Actuals.Resist         = _diceService.Roll1dN(characters.Select(s => s.Actuals.Resist).Sum() / characters.Count);
        npc.Actuals.Actions        = _diceService.Roll1dN(characters.Select(s => s.Actuals.Actions).Sum() / characters.Count); ;
        npc.Actuals.Endurance      = _diceService.Roll1dN(characters.Select(s => s.Actuals.Endurance).Sum() / characters.Count);
        npc.Actuals.Accretion      = _diceService.Roll1dN(characters.Select(s => s.Actuals.Accretion).Sum() / characters.Count);
    }

    private void SetNpcActualsToEasy(Character npc)
    {
        // main
        npc.Actuals.Strength       = _diceService.Roll1dN(20);
        npc.Actuals.Constitution   = _diceService.Roll1dN(20);
        npc.Actuals.Agility        = _diceService.Roll1dN(20);
        npc.Actuals.Willpower      = _diceService.Roll1dN(20);
        npc.Actuals.Abstract       = _diceService.Roll1dN(20);
        // skills
        npc.Actuals.Melee          = _diceService.Roll1dN(20);
        npc.Actuals.Arcane         = _diceService.Roll1dN(20);
        npc.Actuals.Psionics       = _diceService.Roll1dN(20);
        npc.Actuals.Social         = _diceService.Roll1dN(20);
        npc.Actuals.Hide           = _diceService.Roll1dN(20);
        npc.Actuals.Survival       = _diceService.Roll1dN(20);
        npc.Actuals.Tactics        = _diceService.Roll1dN(20);
        npc.Actuals.Aid            = _diceService.Roll1dN(20);
        npc.Actuals.Crafting       = _diceService.Roll1dN(20);
        npc.Actuals.Spot           = _diceService.Roll1dN(20);
        // assets
        npc.Actuals.Defense        = _diceService.Roll1dN(20);
        npc.Actuals.Resist         = _diceService.Roll1dN(20);
        npc.Actuals.Actions        = 1;
        npc.Actuals.Endurance      = _diceService.Roll1dN(20);
        npc.Actuals.Accretion      = _diceService.Roll1dN(20);
    }


    #endregion
}
