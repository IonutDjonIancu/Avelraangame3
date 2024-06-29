using Models;

namespace Services;

public interface ITownhallService
{
    CharacterDuel GetOrGenerateDuel(CharacterIdentity characterIdentity);
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

    public CharacterDuel GetOrGenerateDuel(CharacterIdentity characterIdentity)
    {
        var character = Validators.ValidateCharacterOnDuel(characterIdentity, _snapshot);

        return character.Details.BattleboardId == Guid.Empty ? GenerateDuel(character) : GetDuel(character);
    }


    #region private methods
    public CharacterDuel GetDuel(Character characterBase)
    {
        var character = _characterService.GetCharacter(characterBase.Identity);
        var npc = _snapshot.Npcs.First(s => s.Details.BattleboardId == character.Details.BattleboardId);

        var duel = new CharacterDuel
        {
            Resultboard = "Session restored.",
            RoundNr = 1,
            Character = character,
            Npc = npc
        };

        if (!npc.Details.IsAlive)
        {
            duel.Resultboard = "You have won this fight.";
        }

        return duel;
    }

    public CharacterDuel GenerateDuel(Character characterBase)
    {
        var character = _characterService.GetCharacter(characterBase.Identity);
        Character npc;

        var battleboardId = Guid.NewGuid();

        character.Details.IsLocked = true;
        character.Details.BattleboardId = battleboardId;
        character.Details.BattleboardType = Statics.Battleboards.Types.Duel;

        npc = new Character
        {
            Identity = new CharacterIdentity
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.Empty,
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
                BattleboardId = battleboardId,
                BattleboardType = Statics.Battleboards.Types.Duel,
            },
        };

        SetNpcSpec(npc);
        SetNpcWealth(npc);
        SetNpcActuals(character, npc);

        SetFights(character);
        SetFights(npc);

        _snapshot.Npcs.Add(npc);

        var charDuel = new CharacterDuel
        {
            Character = character,
            Npc = npc
        };

        SetBattlequeue(charDuel);

        return charDuel;
    }

    private void SetBattlequeue(CharacterDuel duel)
    {

        throw new NotImplementedException();

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
        npc.Actuals.Resist    = _diceService.Roll_mdn(character.Actuals.Resist / 2,     character.Actuals.Resist    + character.Actuals.Resist / 2);
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
