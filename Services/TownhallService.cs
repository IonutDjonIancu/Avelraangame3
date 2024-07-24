using Models;

namespace Services;

public interface ITownhallService
{
    Duel GetOrGenerateDuel(CharacterIdentity characterIdentity);
}

public class TownhallService : ITownhallService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _diceService;
    private readonly ICharacterService _characterService;

    public TownhallService(
        ISnapshot snapshot,
        IDiceService diceService,
        ICharacterService characterService)
    {
        _snapshot = snapshot;
        _diceService = diceService;
        _characterService = characterService;  
    }

    public Duel GetOrGenerateDuel(CharacterIdentity characterIdentity)
    {
        var character = Validators.ValidateCharacterOnDuel(characterIdentity, _snapshot);

        return character.Details.BattleboardId == Guid.Empty ? GenerateDuel(character) : GetDuel(character);
    }


    #region private methods
    private Duel GetDuel(Character character)
    {
        return (Duel)_snapshot.Battleboards.First(s => s.Id == character.Details.BattleboardId);
    }

    private Duel GenerateDuel(Character character)
    {
        var battleboardId = Guid.NewGuid();

        character.Details.IsLocked = true;
        character.Details.BattleboardId = battleboardId;
        character.Details.BattleboardType = Statics.Battleboards.Types.Duel;

        var npc = new Character
        {
            Identity = new CharacterIdentity
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.Empty, // session id represents the npc is owned by the Ai
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
                // Portrait = "https://i.pinimg.com/originals/51/a0/2d/51a02d38c57a32ab0035e2608f32ca82.gif", // TODO: need to change this portrait to be dynamic
                Worth = 0,
                BattleboardId = battleboardId,
                BattleboardType = Statics.Battleboards.Types.Duel,
            },
        };

        SetNpcSpec(npc);

        SetNpcWealth(npc);
        
        SetNpcActuals(character, npc);

        SetFights(character);
        SetFights(npc);

        _snapshot.Characters.Add(npc);

        var duel = new Duel
        {
            Id = battleboardId,
            GoodGuys =
            [
                character,
            ],
            BadGuys = 
            [
                npc,
            ],
            RoundNr = 1,
            Type = Statics.Battleboards.Types.Duel
        };

        PrepareForDuel(duel);

        _snapshot.Battleboards.Add(duel);

        return duel;
    }

    private void PrepareForDuel(Duel duel)
    {
        var effort = _diceService.Roll_1dn(Statics.EffortLevels.Easy); // TODO: remove hardcoding of effort level

        var character = duel.GoodGuys.First();
        var npc = duel.BadGuys.First();

        var charRoll = (int)(character.Actuals.TacticsEff * _diceService.Roll_effort_dice(character, Statics.Stats.Tactics, effort));
        var npcRoll = (int)(npc.Actuals.TacticsEff * _diceService.Roll_effort_dice(npc, Statics.Stats.Tactics, effort));

        if (charRoll > npcRoll)
        {
            npc.Fights.Endurance -= charRoll - npcRoll;
            npc.Fights.Accretion -= charRoll - npcRoll;

            duel.Result = "Tactical advantage.";
        }
        else if (charRoll < npcRoll)
        {
            character.Fights.Endurance -= npcRoll - charRoll;
            character.Fights.Accretion -= npcRoll - charRoll;

            duel.Result = "Tactical disadvantage.";
        }
        else
        {
            duel.Result = "Tactical draw.";
        }

        duel.Battlequeue = duel.GoodGuys.Union(duel.BadGuys).OrderByDescending(s => s.Actuals.Actions).Select(s => s.Identity.Id).ToList();
    }
    
    private void SetNpcSpec(Character npc)
    {
        var roll = _diceService.Roll_1dn(3);

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
        npc.Details.Wealth += _diceService.Roll_1dn(100);
    }

    private static void SetFights(Character character)
    {
        // stats
        character.Fights.Combat     = character.Actuals.Combat;
        character.Fights.Strength   = character.Actuals.Strength;
        character.Fights.Tactics    = character.Actuals.Tactics;    
        character.Fights.Athletics  = character.Actuals.Athletics;  
        character.Fights.Survival   = character.Actuals.Survival;   
        character.Fights.Social     = character.Actuals.Social;     
        character.Fights.Abstract   = character.Actuals.Abstract;   
        character.Fights.Psionic    = character.Actuals.Psionic;    
        character.Fights.Crafting   = character.Actuals.Crafting;   
        character.Fights.Medicine   = character.Actuals.Medicine;   
        // effects
        character.Fights.CombatEff      = character.Actuals.CombatEff;
        character.Fights.StrengthEff    = character.Actuals.StrengthEff;
        character.Fights.TacticsEff     = character.Actuals.TacticsEff;    
        character.Fights.AthleticsEff   = character.Actuals.AthleticsEff;  
        character.Fights.SurvivalEff    = character.Actuals.SurvivalEff;   
        character.Fights.SocialEff      = character.Actuals.SocialEff;     
        character.Fights.AbstractEff    = character.Actuals.AbstractEff;   
        character.Fights.PsionicEff     = character.Actuals.PsionicEff;    
        character.Fights.CraftingEff    = character.Actuals.CraftingEff;   
        character.Fights.MedicineEff    = character.Actuals.MedicineEff;   
        // attributes
        character.Fights.Defense   = character.Actuals.Defense;
        character.Fights.Resist    = character.Actuals.Resist;
        character.Fights.Actions   = character.Actuals.Actions;
        character.Fights.Endurance = character.Actuals.Endurance;
        character.Fights.Accretion = character.Actuals.Accretion;
    }

    private void SetNpcActuals(Character character, Character npc)
    {
        // states
        npc.Actuals.Defense   = _diceService.Roll_mdn(character.Actuals.Defense / 2,    character.Actuals.Defense   + character.Actuals.Defense / 2);
        npc.Actuals.Resist    = character.Actuals.Resist > 0 ? _diceService.Roll_mdn(character.Actuals.Resist / 2, character.Actuals.Resist + character.Actuals.Resist / 2) : 0;
        npc.Actuals.Actions   = _diceService.Roll_mdn(character.Actuals.Actions / 2,    character.Actuals.Actions   + character.Actuals.Actions / 2);
        npc.Actuals.Endurance = _diceService.Roll_mdn(character.Actuals.Endurance / 2,  character.Actuals.Endurance + character.Actuals.Endurance / 2);
        npc.Actuals.Accretion = _diceService.Roll_mdn(character.Actuals.Accretion / 2,  character.Actuals.Accretion + character.Actuals.Accretion / 2);
        // rolls
        npc.Actuals.Combat    = _diceService.Roll_mdn(character.Actuals.Combat / 2,     character.Actuals.Combat    + character.Actuals.Combat / 2);
        npc.Actuals.Strength  = _diceService.Roll_mdn(character.Actuals.Strength / 2,   character.Actuals.Strength  + character.Actuals.Strength / 2);
        npc.Actuals.Tactics   = _diceService.Roll_mdn(character.Actuals.Tactics / 2,    character.Actuals.Tactics   + character.Actuals.Tactics / 2);
        npc.Actuals.Athletics = _diceService.Roll_mdn(character.Actuals.Athletics / 2,  character.Actuals.Athletics + character.Actuals.Athletics / 2);
        npc.Actuals.Survival  = _diceService.Roll_mdn(character.Actuals.Survival / 2,   character.Actuals.Survival  + character.Actuals.Survival / 2);
        npc.Actuals.Social    = _diceService.Roll_mdn(character.Actuals.Social / 2,     character.Actuals.Social    + character.Actuals.Social / 2);
        npc.Actuals.Abstract  = _diceService.Roll_mdn(character.Actuals.Abstract / 2,   character.Actuals.Abstract  + character.Actuals.Abstract / 2);
        npc.Actuals.Psionic   = _diceService.Roll_mdn(character.Actuals.Psionic / 2,    character.Actuals.Psionic   + character.Actuals.Psionic / 2);
        npc.Actuals.Crafting  = _diceService.Roll_mdn(character.Actuals.Crafting / 2,   character.Actuals.Crafting  + character.Actuals.Crafting / 2);
        npc.Actuals.Medicine  = _diceService.Roll_mdn(character.Actuals.Medicine / 2,   character.Actuals.Medicine  + character.Actuals.Medicine / 2);
        // effects
        npc.Actuals.CombatEff     = _diceService.Roll_mdn(character.Actuals.CombatEff / 2,    character.Actuals.CombatEff    + character.Actuals.CombatEff / 2);
        npc.Actuals.StrengthEff   = _diceService.Roll_mdn(character.Actuals.StrengthEff / 2,  character.Actuals.StrengthEff  + character.Actuals.StrengthEff / 2);
        npc.Actuals.TacticsEff    = _diceService.Roll_mdn(character.Actuals.TacticsEff / 2,   character.Actuals.TacticsEff   + character.Actuals.TacticsEff / 2);
        npc.Actuals.AthleticsEff  = _diceService.Roll_mdn(character.Actuals.AthleticsEff / 2, character.Actuals.AthleticsEff + character.Actuals.AthleticsEff / 2);
        npc.Actuals.SurvivalEff   = _diceService.Roll_mdn(character.Actuals.SurvivalEff / 2,  character.Actuals.SurvivalEff  + character.Actuals.SurvivalEff / 2);
        npc.Actuals.SocialEff     = _diceService.Roll_mdn(character.Actuals.SocialEff / 2,    character.Actuals.SocialEff    + character.Actuals.SocialEff / 2);
        npc.Actuals.AbstractEff   = _diceService.Roll_mdn(character.Actuals.AbstractEff / 2,  character.Actuals.AbstractEff  + character.Actuals.AbstractEff / 2);
        npc.Actuals.PsionicEff    = _diceService.Roll_mdn(character.Actuals.PsionicEff / 2,   character.Actuals.PsionicEff   + character.Actuals.PsionicEff / 2);
        npc.Actuals.CraftingEff   = _diceService.Roll_mdn(character.Actuals.CraftingEff / 2,  character.Actuals.CraftingEff  + character.Actuals.CraftingEff / 2);
        npc.Actuals.MedicineEff   = _diceService.Roll_mdn(character.Actuals.MedicineEff / 2,  character.Actuals.MedicineEff  + character.Actuals.MedicineEff / 2);
        
        if (npc.Actuals.Defense >= 90)
            npc.Actuals.Defense = 90;

        if (npc.Actuals.Resist >= 100)
            npc.Actuals.Resist = 100;
    }
    #endregion
}
