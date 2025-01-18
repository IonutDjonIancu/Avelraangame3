using Models;

namespace Services;

public interface INpcService
{
    Character GenerateNpc(Board board);
}

public class NpcService : INpcService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _diceService;

    public NpcService(
        ISnapshot snapshot,
        IDiceService diceService)
    {
        _snapshot = snapshot;
        _diceService = diceService;
    }

    public Character GenerateNpc(Board board)
    {
        var npc = new Character
        {
            Identity = new CharacterIdentity
            {
                CharacterId = Guid.NewGuid(),
                PlayerId = Guid.Empty,
            },
            Details = new CharacterDetails
            {
                IsAlive = true,
                Race = "npc",
                Culture = "npc",
                IsHidden = false,
                EntityLevel = 1,
                IsLocked = true,
                IsNpc = true,
                Levelup = 0,
                Name = $"npc_{DateTime.Now.Ticks.ToString()[..5]}",
                Worth = 0,
                BoardId = board.Id,
                BoardType = board.Type,
            },
        };

        SetNpcSpec(npc);
        SetNpcWealth(npc);
        SetNpcActuals(npc, board);

        _snapshot.Characters.Add(npc);

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
                npc.Details.Portrait = "https://i.pinimg.com/originals/1f/cb/7e/1fcb7efbb5bda0240001dd290243dae9.gif";
                break;
            case 2:
                npc.Details.Spec = Statics.Specs.Sorcery;
                npc.Details.Portrait = "https://i.pinimg.com/originals/1f/cb/7e/1fcb7efbb5bda0240001dd290243dae9.gif";
                break;
            case 3:
                npc.Details.Spec = Statics.Specs.Tracking;
                npc.Details.Portrait = "https://i.pinimg.com/originals/56/8f/65/568f655c5c863d407dac3de435c8c242.gif";
                break;
            default:
                break;
        }
    }

    private void SetNpcWealth(Character npc)
    {
        npc.Details.Wealth += _diceService.Roll1dN(10);
    }

    private void SetNpcActuals(Character npc, Board board)
    {
        if (board.EffortLevelName == Statics.EffortLevelNames.Easy)
        {
            SetNpcActualsToEasy(npc);
        }
        else if (board.EffortLevelName == Statics.EffortLevelNames.Normal)
        {
            SetNpcActualsToNormal(npc, board);
        }
        else
        {
            SetNpcActualsToCore(npc, board);
        }
    }
    
    private void SetNpcActualsToCore(Character npc, Board board)
    {
        // main
        npc.Stats.Actuals.Strength      = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength).Max() / 2);
        npc.Stats.Actuals.Constitution  = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution).Max() / 2);
        npc.Stats.Actuals.Agility       = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility).Max() / 2);
        npc.Stats.Actuals.Willpower     = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower).Max() / 2);
        npc.Stats.Actuals.Abstract      = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract).Max() / 2);
        // skills                                                                    
        npc.Stats.Actuals.Melee         = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee).Max() / 2);
        npc.Stats.Actuals.Arcane        = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane).Max() / 2);
        npc.Stats.Actuals.Psionics      = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics).Max() / 2);
        npc.Stats.Actuals.Social        = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social).Max() / 2);
        npc.Stats.Actuals.Hide          = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide).Max() / 2);
        npc.Stats.Actuals.Survival      = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival).Max() / 2);
        npc.Stats.Actuals.Tactics       = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics).Max() / 2);
        npc.Stats.Actuals.Aid           = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid).Max() / 2);
        npc.Stats.Actuals.Crafting      = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting).Max() / 2);
        npc.Stats.Actuals.Perception    = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception).Max() / 2);
        // assets                                                                     
        npc.Stats.Actuals.Defense       = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense).Max() / 2);
        npc.Stats.Actuals.Actions       = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions).Max() / 2);
        npc.Stats.Actuals.Endurance     = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance).Max() / 2);
        npc.Stats.Actuals.Accretion     = _diceService.RollMdN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion).Min() / 2, board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion).Max() + board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion).Max() / 2);
    }

    private void SetNpcActualsToNormal(Character npc, Board board)
    {
        // main
        npc.Stats.Actuals.Strength      = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Strength).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Constitution  = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Constitution).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Agility       = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Agility).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Willpower     = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Willpower).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Abstract      = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Abstract).Sum() / board.GetAllBoardCharacters().Count);
        // skills                                                                     
        npc.Stats.Actuals.Melee         = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Melee).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Arcane        = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Arcane).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Psionics      = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Psionics).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Social        = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Social).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Hide          = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Hide).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Survival      = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Survival).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Tactics       = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Tactics).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Aid           = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Aid).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Crafting      = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Crafting).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Perception    = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Perception).Sum() / board.GetAllBoardCharacters().Count);
        // assets                                                                      
        npc.Stats.Actuals.Defense       = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Defense).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Actions       = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Actions).Sum() / board.GetAllBoardCharacters().Count); 
        npc.Stats.Actuals.Endurance     = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Endurance).Sum() / board.GetAllBoardCharacters().Count);
        npc.Stats.Actuals.Accretion     = _diceService.Roll1dN(board.GetAllBoardCharacters().Select(s => s.Stats.Actuals.Accretion).Sum() / board.GetAllBoardCharacters().Count);
    }

    private void SetNpcActualsToEasy(Character npc)
    {
        // main
        npc.Stats.Actuals.Strength      = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Constitution  = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Agility       = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Willpower     = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Abstract      = _diceService.Roll1dN(10);
        // skills                                            
        npc.Stats.Actuals.Melee         = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Arcane        = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Psionics      = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Social        = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Hide          = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Survival      = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Tactics       = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Aid           = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Crafting      = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Perception    = _diceService.Roll1dN(10);
        // assets                                            
        npc.Stats.Actuals.Defense       = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Actions       = 1;                   
        npc.Stats.Actuals.Endurance     = _diceService.Roll1dN(10);
        npc.Stats.Actuals.Accretion     = _diceService.Roll1dN(10);
    }


    #endregion
}
