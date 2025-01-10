namespace Models;

public static class Statics
{
    public static class Boards
    {
        public static class Types
        {
            public const string Duel    = "Duel";
            public const string Tourney = "Tourney";
            public const string Quest   = "Quest";
            public const string Tavern  = "Tavern";
            public const string Market  = "Market";

            public static readonly List<string> All =
            [
                Duel, Tourney, Quest, Tavern, Market
            ];
        }

        public static class ActionTypes
        {
            public const string Melee       = "Melee";
            public const string Arcane      = "Arcane";
            public const string Psionics    = "Psionics";
            public const string Aid         = "Aid";
            public const string Hide        = "Hide";
            public const string Traps       = "Traps";
            
            public static readonly List<string> All =
            [
                Melee, Arcane, Psionics, Aid, Hide, Traps
            ];
        }
    }

    public static class EffortLevels
    {
        public const int Easy   = 20; // up to
        public const int Normal = 40; // up to
    }

    public static class EffortLevelNames
    {
        public const string Easy    = "Easy";
        public const string Normal  = "Medium";
        public const string Core    = "Core";

        public static readonly List<string> All =
        [
            Easy, Normal, Core
        ];
    }

    public static class Stats
    {
        // main
        public const string Strength        = "Strength";
        public const string Constitution    = "Constitution";
        public const string Agility         = "Agility";
        public const string Willpower       = "Willpower";
        public const string Abstract        = "Abstract";
        // skills
        public const string Melee           = "Melee";
        public const string Arcane          = "Arcane";
        public const string Psionics        = "Psionics";
        public const string Social          = "Social";
        public const string Hide            = "Hide";
        public const string Survival        = "Survival";
        public const string Tactics         = "Tactics";
        public const string Aid             = "Aid";
        public const string Crafting        = "Crafting";
        public const string Perception      = "Perception";
        // assets
        public const string Defense         = "Defense";
        public const string Actions         = "Actions";
        public const string Hitpoints       = "Hitpoints";
        public const string Mana            = "Mana";


        public static readonly List<string> All =
        [
            Strength,
            Constitution,
            Agility,
            Willpower,
            Abstract,
            Melee,
            Arcane,
            Psionics,
            Social,
            Hide,
            Survival,
            Tactics,
            Aid,
            Crafting,
            Perception,
            Defense,
            Actions,
            Hitpoints,
            Mana
        ];
    }

    public static class SpecialSkills
    {
        public const string Overwhelm   = "Overwhelm";
        public const string Brace       = "Brace";
        public const string Force       = "Force";
        public const string Prepare     = "Prepare";

        public static readonly List<string> All =
        [
            Overwhelm, Brace, Force, Prepare, // Combat
            // TODO: add the rest of the special skills
        ];
    }

    public static class Items
    {
        public static class Weapons 
        {
            public const string Sword = "Arming sword";
            public const string LongSword = "Long sword";
            public const string ShortSword = "Short sword";
            public const string CurvedBlade = "Curved blade";
            public const string Falx = "Falx";
            public const string Axe = "Axe";
            public const string DaneAxe = "Dane axe";
            public const string Mace = "Mace";
            public const string Warhammer = "Warhammer";
            public const string Spear = "Spear";
            public const string Polearm = "Polearm";
            public const string Staff = "Staff";
            public const string Dagger = "Dagger";
            public const string Seax = "Seax";

            public static readonly List<string> All =
            [
                Sword, LongSword, ShortSword, CurvedBlade, Falx, Axe, DaneAxe, Mace, Warhammer, Spear, Polearm, Staff, Dagger, Seax
            ];
        }

        public static class Shields
        {
            public const string Buckler = "Buckler";
            public const string RoundShield = "Round shield";
            public const string KiteShield = "Kite shield";
            public const string NormanShield = "Norman shield";
            public const string Pavise = "Pavise";

            public static readonly List<string> All =
            [
                Buckler, RoundShield, KiteShield, NormanShield, Pavise
            ];
        }

        public static class Armours
        {
            public const string Linothorax = "Linothorax";
            public const string Chainmail = "Chain mail";
            public const string Scalemail = "Scale mail";
            public const string Halfplate = "Halfplate armour";
            public const string Fullplate = "Fullplate armour";
            public const string Brigandine = "Brigandine armour";

            public static readonly List<string> All =
            [
                Linothorax, Chainmail, Scalemail, Halfplate, Fullplate, Brigandine
            ];
        }

        public static class Trinkets
        {
            public const string OrbOfStars = "Orb of stars";
            public const string AmuletOfBlackraveny = "Amulet of blackraveny";
            public const string RingOfTheDjinn = "Ring of the djinn";
            public const string SashOfDeirmaidun = "Sash of deirmaidun";
            public const string SealOfLotharius = "Seal of lotharius";
            public const string SealOfSojourn = "Seal of sojourn";
            public const string ArmbandOfAsriedor = "Armband of asriendor";
            public const string ScarfOfFearuinar = "Scarf of fearuinar";
            public const string OakleafOfCedricon = "Oakleaf of cedricon";
            public const string RuneOfTheSouthernStar = "Rune of the southern star";
            public const string ShardOfSepteracorium = "Shard of septeracorium";
            public const string PommelOfTheMarquis = "Pommel of the marquis";

            public static readonly List<string> All =
            [
                OrbOfStars, AmuletOfBlackraveny, RingOfTheDjinn, SashOfDeirmaidun, SealOfLotharius, SealOfSojourn, RuneOfTheSouthernStar, ShardOfSepteracorium, ArmbandOfAsriedor, ScarfOfFearuinar, OakleafOfCedricon, PommelOfTheMarquis
            ];
        }

        public static class Types
        {
            public const string Weapon = "Weapon";
            public const string Shield = "Shield";
            public const string Armour = "Armour";
            public const string Trinket = "Trinket";

            public static readonly List<string> All =
            [
                Weapon, Shield, Armour, Trinket
            ];
        }
    }

    public static class Races
    {
        public const string Human = "Human";
        public static class Humans
        {
            // stats
            public const int Strength       = 3;
            public const int Constitution   = 3;
            public const int Agility        = 3;
            public const int Willpower      = 4;
            public const int Abstract       = 2;
            // skills
            public const int Melee          = 3;     
            public const int Arcane         = 1;
            public const int Psionics       = 5;
            public const int Social         = 5;
            public const int Hide           = 1;
            public const int Survival       = 3;
            public const int Tactics        = 3;
            public const int Aid            = 3;
            public const int Crafting       = 3;
            public const int Perception     = 2;
            // assets
            public const int Defense        = 1;
            public const int Actions        = 1;
            public const int Hitpoints      = 3;
            public const int Mana           = 0;
        }

        public const string Elf = "Elf";
        public static class Elfs
        {
            // stats
            public const int Strength       = 2;
            public const int Constitution   = 5;
            public const int Agility        = 5;
            public const int Willpower      = 4;
            public const int Abstract       = 5;
            // skills
            public const int Melee          = 4;     
            public const int Arcane         = 5;
            public const int Psionics       = 1;
            public const int Social         = 2;
            public const int Hide           = 5;
            public const int Survival       = 4;
            public const int Tactics        = 2;
            public const int Aid            = 4;
            public const int Crafting       = 2;
            public const int Perception     = 5;
            // assets
            public const int Defense        = 1;
            public const int Actions        = 1;
            public const int Hitpoints      = 2;
            public const int Mana           = 5;
        }

        public const string Dwarf = "Dwarf";
        public static class Dwarfs
        {
            // stats
            public const int Strength       = 5;
            public const int Constitution   = 5;
            public const int Agility        = 2;
            public const int Willpower      = 5;
            public const int Abstract       = 4;
            // skills
            public const int Melee          = 5;    
            public const int Arcane         = 0;
            public const int Psionics       = 1;
            public const int Social         = 0;
            public const int Hide           = 0;
            public const int Survival       = 4;
            public const int Tactics        = 4;
            public const int Aid            = 3;
            public const int Crafting       = 5;
            public const int Perception     = 1;
            // assets
            public const int Defense        = 3;
            public const int Actions        = 1;
            public const int Hitpoints      = 4;
            public const int Mana           = 0;
        }

        public static readonly List<string> All =
        [
            Human,
            Elf,
            Dwarf
        ];
    }

    public static class Cultures
    {
        public const string Danarian = "Danarian";
        public static class Danarians
        {
            // stats
            public const int Strength       = 1;
            public const int Constitution   = 2;
            public const int Agility        = 0;
            public const int Willpower      = 3;
            public const int Abstract       = 1;
            // skills
            public const int Melee          = 5;    
            public const int Arcane         = 0;
            public const int Psionics       = 4;
            public const int Social         = 4;
            public const int Hide           = 0;
            public const int Survival       = 1;
            public const int Tactics        = 4;
            public const int Aid            = 2;
            public const int Crafting       = 3;
            public const int Perception     = 0;
            // assets
            public const int Defense        = 2;
            public const int Actions        = 0;
            public const int Hitpoints      = 1;
            public const int Mana           = 0;
        }

        public const string Highborn = "Highborn";
        public static class Highborns
        {
            // stats
            public const int Strength       = 0;
            public const int Constitution   = 0;
            public const int Agility        = 3;
            public const int Willpower      = 1;
            public const int Abstract       = 5;
            // skills
            public const int Melee          = 5;    
            public const int Arcane         = 5;
            public const int Psionics       = 0;
            public const int Social         = 1;
            public const int Hide           = 2;
            public const int Survival       = 3;
            public const int Tactics        = 0;
            public const int Aid            = 1;
            public const int Crafting       = 2;
            public const int Perception     = 4;
            // assets
            public const int Defense        = 0;
            public const int Actions        = 0;
            public const int Hitpoints      = 2;
            public const int Mana           = 3;
        }

        public const string Undermountain = "Undermountain";
        public static class Undermountains
        {
            // stats
            public const int Strength       = 5;
            public const int Constitution   = 3;
            public const int Agility        = 0;
            public const int Willpower      = 3;
            public const int Abstract       = 2;
            public const int Melee          = 5;   
            // skills
            public const int Arcane         = 0;
            public const int Psionics       = 3;
            public const int Social         = 1;
            public const int Hide           = 0;
            public const int Survival       = 4;
            public const int Tactics        = 3;
            public const int Aid            = 3;
            public const int Crafting       = 5;
            public const int Perception     = 0;
            // assets
            public const int Defense        = 5;
            public const int Actions        = 0;
            public const int Hitpoints      = 3;
            public const int Mana           = 0;
        }

        public static readonly List<string> All =
        [
            Danarian,
            Highborn,
            Undermountain
        ];
    }

    public static class Specs
    {
        public const string Warring = "Warring";
        public static class Warrings
        {
            // stats
            public const int Strength       = 5;
            public const int Constitution   = 3;
            public const int Agility        = 2;
            public const int Willpower      = 3;
            public const int Abstract       = 0;
            // skills
            public const int Melee          = 5;   
            public const int Arcane         = 0;
            public const int Psionics       = 2;
            public const int Social         = 0;
            public const int Hide           = 2;
            public const int Survival       = 1;
            public const int Tactics        = 1;
            public const int Aid            = 2;
            public const int Crafting       = 1;
            public const int Perception     = 3;
            // assets
            public const int Defense        = 5;
            public const int Actions        = 1;
            public const int Hitpoints      = 5;
            public const int Mana           = 0;
        }

        public const string Sorcery = "Sorcery";
        public static class Sorcerys
        {
            // stats
            public const int Strength       = 0;
            public const int Constitution   = 3;
            public const int Agility        = 1;
            public const int Willpower      = 2;
            public const int Abstract       = 5;
            // skills
            public const int Melee          = 0;   
            public const int Arcane         = 5;
            public const int Psionics       = 0;
            public const int Social         = 0;
            public const int Hide           = 2;
            public const int Survival       = 1;
            public const int Tactics        = 0;
            public const int Aid            = 1;
            public const int Crafting       = 0;
            public const int Perception     = 3;
            // assets
            public const int Defense        = 0;
            public const int Actions        = 0;
            public const int Hitpoints      = 2;
            public const int Mana           = 5;
        }

        public const string Tracking = "Tracking";
        public static class Trackings
        {
            // stats
            public const int Strength       = 2;
            public const int Constitution   = 3;
            public const int Agility        = 4;
            public const int Willpower      = 1;
            public const int Abstract       = 1;
            // skills
            public const int Melee          = 2;   
            public const int Arcane         = 1;
            public const int Psionics       = 0;
            public const int Social         = 0;
            public const int Hide           = 5;
            public const int Survival       = 5;
            public const int Tactics        = 1;
            public const int Aid            = 4;
            public const int Crafting       = 4;
            public const int Perception     = 5;
            // assets
            public const int Defense        = 2;
            public const int Actions        = 0;
            public const int Hitpoints      = 3;
            public const int Mana           = 0;
        }

        public static readonly List<string> All =
        [
            Warring,
            Sorcery,
            Tracking
        ];
    }
}
