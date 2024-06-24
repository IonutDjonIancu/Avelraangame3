using Models;

namespace Services;

public interface INpcService
{
    CharacterNpc GenerateNpcForDuel(CharacterIdentity character);
}

public class NpcService : INpcService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _diceService;
    private readonly ICharacterService _characterService;

    public NpcService(
        ISnapshot snapshot,
        IDiceService diceService,
        ICharacterService characterService)
    {
        _snapshot = snapshot;
        _diceService = diceService;
        _characterService = characterService;  
    }

    public CharacterNpc GenerateNpcForDuel(CharacterIdentity characterIdentity)
    {
        var character = Validators.ValidateCharacterOnNpcGenerate(characterIdentity, _snapshot);
        CharacterNpc npc;

        if (character.Details.BattleboardId != Guid.Empty)
        {
            npc = _snapshot.Npcs.First(s => s.Details.BattleboardId == character.Details.BattleboardId);
            return npc;
        }

        var battleboardId = Guid.NewGuid();

        character.Details.IsLocked = true;
        character.Details.BattleboardId = battleboardId;
        character.Details.BattleboardType = Statics.Battleboards.Types.Duel;

        var charActual = _characterService.GetCharacter(character.Identity.Id, character.Identity.SessionId);

        npc = new CharacterNpc
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
        SetNpcActuals(npc, charActual.Actuals);

        _snapshot.Npcs.Add(npc);

        return npc;
    }

    #region private methods
    private void SetNpcSpec(CharacterNpc npc)
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

    private void SetNpcWealth(CharacterNpc npc)
    {
        npc.Details.Wealth += _diceService.Roll_1dn(100);
    }

    private void SetNpcActuals(CharacterNpc npc, CharacterActuals character)
    {
        // stats
        npc.Actuals.Stats.Defense = _diceService.Roll_mdn(character.Stats.Defense / 2, character.Stats.Defense + character.Stats.Defense / 2);
        if (npc.Actuals.Stats.Defense >= 90)
            npc.Actuals.Stats.Defense = 90;
        npc.Actuals.Stats.Resist = _diceService.Roll_mdn(character.Stats.Resist / 2, character.Stats.Resist + character.Stats.Resist / 2);
        if (npc.Actuals.Stats.Resist >= 100)
            npc.Actuals.Stats.Resist = 100;
        npc.Actuals.Stats.Actions = _diceService.Roll_mdn(character.Stats.Actions / 2, character.Stats.Actions + character.Stats.Actions / 2);
        npc.Actuals.Stats.Endurance = _diceService.Roll_mdn(character.Stats.Endurance / 2, character.Stats.Endurance + character.Stats.Endurance / 2);
        npc.Actuals.Stats.Accretion = _diceService.Roll_mdn(character.Stats.Accretion / 2, character.Stats.Accretion + character.Stats.Accretion / 2);
        // feats
        npc.Actuals.Feats.Combat = _diceService.Roll_mdn(character.Feats.Combat / 2, character.Feats.Combat + character.Feats.Combat / 2);
        npc.Actuals.Feats.Strength = _diceService.Roll_mdn(character.Feats.Strength / 2, character.Feats.Strength + character.Feats.Strength / 2);
        npc.Actuals.Feats.Tactics = _diceService.Roll_mdn(character.Feats.Tactics / 2, character.Feats.Tactics + character.Feats.Tactics / 2);
        npc.Actuals.Feats.Athletics = _diceService.Roll_mdn(character.Feats.Athletics / 2, character.Feats.Athletics + character.Feats.Athletics / 2);
        npc.Actuals.Feats.Survival = _diceService.Roll_mdn(character.Feats.Survival / 2, character.Feats.Survival + character.Feats.Survival / 2);
        npc.Actuals.Feats.Social = _diceService.Roll_mdn(character.Feats.Social / 2, character.Feats.Social + character.Feats.Social / 2);
        npc.Actuals.Feats.Abstract = _diceService.Roll_mdn(character.Feats.Abstract / 2, character.Feats.Abstract + character.Feats.Abstract / 2);
        npc.Actuals.Feats.Psionic = _diceService.Roll_mdn(character.Feats.Psionic / 2, character.Feats.Psionic + character.Feats.Psionic / 2);
        npc.Actuals.Feats.Crafting = _diceService.Roll_mdn(character.Feats.Crafting / 2, character.Feats.Crafting + character.Feats.Crafting / 2);
        npc.Actuals.Feats.Medicine = _diceService.Roll_mdn(character.Feats.Medicine / 2, character.Feats.Medicine + character.Feats.Medicine / 2);
        // featseff
        npc.Actuals.Feats.CombatEff = _diceService.Roll_mdn(character.Feats.CombatEff / 2, character.Feats.CombatEff + character.Feats.CombatEff / 2);
        npc.Actuals.Feats.StrengthEff = _diceService.Roll_mdn(character.Feats.StrengthEff / 2, character.Feats.StrengthEff + character.Feats.StrengthEff / 2);
        npc.Actuals.Feats.TacticsEff = _diceService.Roll_mdn(character.Feats.TacticsEff / 2, character.Feats.TacticsEff + character.Feats.TacticsEff / 2);
        npc.Actuals.Feats.AthleticsEff = _diceService.Roll_mdn(character.Feats.AthleticsEff / 2, character.Feats.AthleticsEff + character.Feats.AthleticsEff / 2);
        npc.Actuals.Feats.SurvivalEff = _diceService.Roll_mdn(character.Feats.SurvivalEff / 2, character.Feats.SurvivalEff + character.Feats.SurvivalEff / 2);
        npc.Actuals.Feats.SocialEff = _diceService.Roll_mdn(character.Feats.SocialEff / 2, character.Feats.SocialEff + character.Feats.SocialEff / 2);
        npc.Actuals.Feats.AbstractEff = _diceService.Roll_mdn(character.Feats.AbstractEff / 2, character.Feats.AbstractEff + character.Feats.AbstractEff / 2);
        npc.Actuals.Feats.PsionicEff = _diceService.Roll_mdn(character.Feats.PsionicEff / 2, character.Feats.PsionicEff + character.Feats.PsionicEff / 2);
        npc.Actuals.Feats.CraftingEff = _diceService.Roll_mdn(character.Feats.CraftingEff / 2, character.Feats.CraftingEff + character.Feats.CraftingEff / 2);
        npc.Actuals.Feats.MedicineEff = _diceService.Roll_mdn(character.Feats.MedicineEff / 2, character.Feats.MedicineEff + character.Feats.MedicineEff / 2);
    }



    #endregion
}
