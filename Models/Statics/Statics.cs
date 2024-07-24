namespace Models;

public static class Statics
{
    public static class Battleboards
    {
        public static class Types
        {
            public const string Duel = "Duel";
            public const string Tourney = "Tourney";
            public const string Quest = "Quest";
            public const string Tavern = "Tavern";
            public const string Market = "Market";

            public static readonly List<string> All =
            [
                Duel, Tourney, Quest, Tavern, Market
            ];
        }

        public static class ActionTypes
        {
            public const string Attack = "Attack";
            public const string Brace = "Brace";
            public const string Force = "Force";
            public const string Prepare = "Prepare";
            public const string Wrestle = "Wrestle";
            public const string Staunch = "Staunch";
            public const string Lift = "Lift";
            public const string Destroy = "Destroy";
            public const string Charge = "Charge";
            public const string Hold = "Hold";
            public const string Formation = "Formation";
            public const string Inline = "Inline";
            
            public static readonly List<string> All =
            [
                Attack, Brace, Force, Prepare, // combat
                Wrestle, Staunch, Lift, Destroy, // strength
                Charge, Hold, Formation, Inline, // tactics


            ];
        }
    }

    public static class EffortLevels
    {
        public const int Easy = 20; // up to
        public const int Medium = 40; // up to
        public const int Hard = 80; // up to
    }

    public static class Stats
    {
        public const string Defense = "Defense";
        public const string Resist = "Resist ";
        public const string Actions = "Actions";
        public const string Endurance = "Endurance";
        public const string Accretion = "Accretion";
        public const string Combat = "Combat";
        public const string Strength = "Strength";
        public const string Tactics = "Tactics";
        public const string Athletics = "Athletics";
        public const string Survival = "Survival";
        public const string Social = "Social";
        public const string Abstract = "Abstract";
        public const string Psionic = "Psionic";
        public const string Crafting = "Crafting";
        public const string Medicine = "Medicine";

        public static readonly List<string> All =
        [
            Defense,
            Resist,
            Actions,
            Endurance,
            Accretion,
            Combat,
            Strength,
            Tactics,
            Athletics,
            Survival,
            Social,
            Abstract,
            Psionic,
            Crafting,
            Medicine
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
            public const string AmuletOfStars = "Amulet of stars";
            public const string RingOfTheMadji = "Ring of the madji";
            public const string SashOfTenebre = "Sash of tenebre";
            public const string SealOfSojourn = "Seal of sojourn";
            public const string RuneOfTheSouthernStar = "Rune of the southern star";
            public const string OrbOfSepteracorium = "Orb of septeracorium";

            public static readonly List<string> All =
            [
                AmuletOfStars, RingOfTheMadji, SashOfTenebre, SealOfSojourn, RuneOfTheSouthernStar, OrbOfSepteracorium
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
            // states
            public const int Defense        = 1;
            public const int Resist         = 1;
            public const int Actions        = 1;
            public const int Endurance      = 15;
            public const int Accretion      = 5;
            // rolls and effects
            public const int Combat         = 1;
            public const int CombatEff      = 5;
            public const int Strength       = 1;
            public const int StrengthEff    = 2;
            public const int Tactics        = 2;
            public const int TacticsEff     = 5;
            public const int Athletics      = 1;
            public const int AthleticsEff   = 2;
            public const int Survival       = 1;
            public const int SurvivalEff    = 3;
            public const int Social         = 2;
            public const int SocialEff      = 5;
            public const int Abstract       = 1;
            public const int AbstractEff    = 1;
            public const int Psionic        = 3;
            public const int PsionicEff     = 3;
            public const int Crafting       = 3;
            public const int CraftingEff    = 3;
            public const int Medicine       = 2;
            public const int MedicineEff    = 2;
        }

        public const string Elf = "Elf";
        public static class Elfs
        {
            // states
            public const int Defense        = 1;
            public const int Resist         = 1;
            public const int Actions        = 1;
            public const int Endurance      = 10;
            public const int Accretion      = 10;
            // rolls and effects
            public const int Combat         = 1;
            public const int CombatEff      = 3;
            public const int Strength       = 2;
            public const int StrengthEff    = 1;
            public const int Tactics        = 1;
            public const int TacticsEff     = 0;
            public const int Athletics      = 2;
            public const int AthleticsEff   = 5;
            public const int Survival       = 1;
            public const int SurvivalEff    = 5;
            public const int Social         = 1;
            public const int SocialEff      = 3;
            public const int Abstract       = 5;
            public const int AbstractEff    = 5;
            public const int Psionic        = 1;
            public const int PsionicEff     = 0;
            public const int Crafting       = 2;
            public const int CraftingEff    = 5;
            public const int Medicine       = 5;
            public const int MedicineEff    = 3;
        }

        public const string Dwarf = "Dwarf";
        public static class Dwarfs
        {
            // states
            public const int Defense        = 3;
            public const int Resist         = 3;
            public const int Actions        = 1;
            public const int Endurance      = 20;
            public const int Accretion      = 1;
            // rolls and effects
            public const int Combat         = 2;
            public const int CombatEff      = 5;
            public const int Strength       = 1;
            public const int StrengthEff    = 5;
            public const int Tactics        = 2;
            public const int TacticsEff     = 2;
            public const int Athletics      = 1;
            public const int AthleticsEff   = -2;
            public const int Survival       = 3;
            public const int SurvivalEff    = 5;
            public const int Social         = 1;
            public const int SocialEff      = 1;
            public const int Abstract       = 5;
            public const int AbstractEff    = 1;
            public const int Psionic        = 1;
            public const int PsionicEff     = 3;
            public const int Crafting       = 5;
            public const int CraftingEff    = 5;
            public const int Medicine       = 1;
            public const int MedicineEff    = 2;
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
            // states
            public const int Defense        = 2;
            public const int Resist         = 0;
            public const int Actions        = 0;
            public const int Endurance      = 3;
            public const int Accretion      = 0;
            // rolls and effects
            public const int Combat         = 1;
            public const int CombatEff      = 1;
            public const int Strength       = 0;
            public const int StrengthEff    = 0;
            public const int Tactics        = 3;
            public const int TacticsEff     = 5;
            public const int Athletics      = 0;
            public const int AthleticsEff   = 0;
            public const int Survival       = -1;
            public const int SurvivalEff    = -2;
            public const int Social         = 2;
            public const int SocialEff      = 2;
            public const int Abstract       = 0;
            public const int AbstractEff    = 0;
            public const int Psionic        = 1;
            public const int PsionicEff     = 1;
            public const int Crafting       = 1;
            public const int CraftingEff    = 0;
            public const int Medicine       = 1;
            public const int MedicineEff    = 1;
        }

        public const string Highborn = "Highborn";
        public static class Highborns
        {
            // states
            public const int Defense        = 0;
            public const int Resist         = 0;
            public const int Actions        = 0;
            public const int Endurance      = 1;
            public const int Accretion      = 10;
            // rolls and effects
            public const int Combat         = 2;
            public const int CombatEff      = 2;
            public const int Strength       = 2;
            public const int StrengthEff    = 2;
            public const int Tactics        = 3;
            public const int TacticsEff     = 3;
            public const int Athletics      = 0;
            public const int AthleticsEff   = 3;
            public const int Survival       = -1;
            public const int SurvivalEff    = -1;
            public const int Social         = 2;
            public const int SocialEff      = 1;
            public const int Abstract       = 1;
            public const int AbstractEff    = 1;
            public const int Psionic        = 2;
            public const int PsionicEff     = 2;
            public const int Crafting       = 5;
            public const int CraftingEff    = 4;
            public const int Medicine       = 4;
            public const int MedicineEff    = 4;
        }

        public const string Undermountain = "Undermountain";
        public static class Undermountains
        {
            // states
            public const int Defense        = 5;
            public const int Resist         = 5;
            public const int Actions        = 0;
            public const int Endurance      = 5;
            public const int Accretion      = 0;
            // rolls and effects
            public const int Combat         = 3;
            public const int CombatEff      = 3;
            public const int Strength       = 3;
            public const int StrengthEff    = 3;
            public const int Tactics        = 1;
            public const int TacticsEff     = 0;
            public const int Athletics      = -1;
            public const int AthleticsEff   = -1;
            public const int Survival       = 3;
            public const int SurvivalEff    = 5;
            public const int Social         = 1;
            public const int SocialEff      = 0;
            public const int Abstract       = 3;
            public const int AbstractEff    = 0;
            public const int Psionic        = 1;
            public const int PsionicEff     = 2;
            public const int Crafting       = 5;
            public const int CraftingEff    = 5;
            public const int Medicine       = 2;
            public const int MedicineEff    = 2;
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
            // states
            public const int Defense        = 5;
            public const int Resist         = 0;
            public const int Actions        = 1;
            public const int Endurance      = 15;
            public const int Accretion      = 0;
            // rolls and effects
            public const int Combat         = 5;
            public const int CombatEff      = 5;
            public const int Strength       = 4;
            public const int StrengthEff    = 4;
            public const int Tactics        = 4;
            public const int TacticsEff     = 1;
            public const int Athletics      = 2;
            public const int AthleticsEff   = 1;
            public const int Survival       = 2;
            public const int SurvivalEff    = 2;
            public const int Social         = 1;
            public const int SocialEff      = -1;
            public const int Abstract       = -1;
            public const int AbstractEff    = -1;
            public const int Psionic        = 0;
            public const int PsionicEff     = 0;
            public const int Crafting       = 1;
            public const int CraftingEff    = 1;
            public const int Medicine       = 2;
            public const int MedicineEff    = 2;
        }

        public const string Sorcery = "Sorcery";
        public static class Sorcerys
        {
            // states
            public const int Defense        = 0;
            public const int Resist         = 5;
            public const int Actions        = 0;
            public const int Endurance      = 5;
            public const int Accretion      = 15;
            // rolls and effects
            public const int Combat         = 1;
            public const int CombatEff      = 1;
            public const int Strength       = -1;
            public const int StrengthEff    = -2;
            public const int Tactics        = 0;
            public const int TacticsEff     = 1;
            public const int Athletics      = 1;
            public const int AthleticsEff   = 1;
            public const int Survival       = 2;
            public const int SurvivalEff    = 1;
            public const int Social         = 2;
            public const int SocialEff      = 1;
            public const int Abstract       = 5;
            public const int AbstractEff    = 5;
            public const int Psionic        = 0;
            public const int PsionicEff     = 0;
            public const int Crafting       = 0;
            public const int CraftingEff    = 0;
            public const int Medicine       = 2;
            public const int MedicineEff    = 3;
        }

        public const string Tracking = "Tracking";
        public static class Trackings
        {
            // states
            public const int Defense        = 2;
            public const int Resist         = 2;
            public const int Actions        = 0;
            public const int Endurance      = 10;
            public const int Accretion      = 5;
            // rolls and effects
            public const int Combat         = 2;
            public const int CombatEff      = 2;
            public const int Strength       = 3;
            public const int StrengthEff    = 3;
            public const int Tactics        = 2;
            public const int TacticsEff     = 2;
            public const int Athletics      = 3;
            public const int AthleticsEff   = 2;
            public const int Survival       = 5;
            public const int SurvivalEff    = 5;
            public const int Social         = 0;
            public const int SocialEff      = -2;
            public const int Abstract       = 1;
            public const int AbstractEff    = 1;
            public const int Psionic        = 1;
            public const int PsionicEff     = 0;
            public const int Crafting       = 4;
            public const int CraftingEff    = 4;
            public const int Medicine       = 4;
            public const int MedicineEff    = 5;
        }

        public static readonly List<string> All =
        [
            Warring,
            Sorcery,
            Tracking
        ];
    }
}
