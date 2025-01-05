using Models;

namespace Services;

public interface INpcService
{
    Character GenerateNpc(string effortLevelName, List<Character>? characters = null);
}

public class NpcService : INpcService
{
    private readonly IDiceService _diceService;

    public NpcService(IDiceService diceService)
    {
        _diceService = diceService;
    }

    public Character GenerateNpc(string effortLevelName, List<Character>? characters = null)
    {
        var npc = new Character
        {
            Identity = new CharacterIdentity
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.Empty, // empty session id represents the npc is owned by the Ai
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
                Portrait = "https://i.pinimg.com/originals/a7/73/d6/a773d6556319fa93b90385e96da2a0d5.gif", // TODO: need to change this portrait to be dynamic
                Worth = 0,
            },
        };

        SetNpcSpec(npc);

        if (effortLevelName == Statics.EffortLevelNames.Core)
        {
            throw new NotImplementedException();   
        } 
        else
        {
            SetNpcActualsAndFights_nonCore(effortLevelName, npc);
        }

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
    
    private void SetNpcActualsAndFights_nonCore(string effortLevelName, Character npc)
    {
        var effortLvl = effortLevelName == Statics.EffortLevelNames.Medium ? Statics.EffortLevels.Medium : Statics.EffortLevels.Easy;

        // main
        npc.Actuals.Strength = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Constitution = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Agility = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Willpower = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Abstract = _diceService.Roll1dN(effortLvl);
        // skills
        npc.Actuals.Psionics = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Social = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Hide = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Survival = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Tactics = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Aid = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Crafting = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Spot = _diceService.Roll1dN(effortLvl);
        // assets
        npc.Actuals.Defense = _diceService.Roll1dN(effortLvl);
        npc.Actuals.Resist = _diceService.Roll1dN(effortLvl); 
        npc.Actuals.Defense = _diceService.Roll1dN(effortLvl);

        npc.Actuals.Actions = effortLevelName == Statics.EffortLevelNames.Medium ? 2 : 1;

        var skillRoll1 = _diceService.Roll1dN(effortLvl);
        var skillRoll2 = _diceService.Roll1dN(effortLvl);
        if (npc.Details.Spec == Statics.Specs.Sorcery)
        {
            npc.Actuals.Arcane = skillRoll1 >= skillRoll2 ? skillRoll1 : skillRoll2;
            npc.Actuals.Melee = skillRoll1 >= skillRoll2 ? skillRoll2 : skillRoll1;
            npc.Actuals.Endurance = _diceService.Roll1dN(50);
            npc.Actuals.Accretion = _diceService.Roll1dN(100);
        }
        else
        {
            npc.Actuals.Melee = skillRoll1 >= skillRoll2 ? skillRoll1 : skillRoll2;
            npc.Actuals.Arcane = skillRoll1 >= skillRoll2 ? skillRoll2 : skillRoll1;

            if (npc.Details.Spec == Statics.Specs.Warring)
            {
                npc.Actuals.Endurance = _diceService.Roll1dN(100);
                npc.Actuals.Accretion = 0;
            }
            else if (npc.Details.Spec == Statics.Specs.Tracking)
            {
                npc.Actuals.Endurance = _diceService.Roll1dN(75);
                npc.Actuals.Accretion = _diceService.Roll1dN(50);
            }
        }

        if (npc.Actuals.Defense >= 90)
        {
            npc.Actuals.Defense = 90;
        }
        if (npc.Actuals.Resist >= 100)
        {
            npc.Actuals.Resist = 100;
        }

        // main
        npc.Fights.Strength = npc.Actuals.Strength;
        npc.Fights.Constitution = npc.Actuals.Constitution;
        npc.Fights.Agility = npc.Actuals.Agility;
        npc.Fights.Willpower = npc.Actuals.Willpower;
        npc.Fights.Abstract = npc.Actuals.Abstract;
        // skills
        npc.Fights.Melee = npc.Actuals.Melee;
        npc.Fights.Arcane = npc.Actuals.Arcane;
        npc.Fights.Psionics = npc.Actuals.Psionics;
        npc.Fights.Social = npc.Actuals.Social;
        npc.Fights.Hide = npc.Actuals.Hide;
        npc.Fights.Survival = npc.Actuals.Survival;
        npc.Fights.Tactics = npc.Actuals.Tactics;
        npc.Fights.Aid = npc.Actuals.Aid;
        npc.Fights.Crafting = npc.Actuals.Crafting;
        npc.Fights.Spot = npc.Actuals.Spot;
        // assets
        npc.Fights.Defense = npc.Actuals.Defense;
        npc.Fights.Resist  = npc.Actuals.Resist ;
        npc.Fights.Actions = npc.Actuals.Actions;
        npc.Fights.Endurance = npc.Actuals.Endurance;
        npc.Fights.Accretion = npc.Actuals.Accretion;
    }
    #endregion
}
