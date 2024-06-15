namespace Models;

public static class Statics
{
    public static class EffortLevels
    {
        public const int Easy = 20;
        public const int Medium = 40;
        public const int Hard = 80;
    }

    public static class Crafts
    {
        public const string Combat = "Combat";
        public const string Arcane = "Arcane";
        public const string Alchemy = "Alchemy";
        public const string Psionics = "Psionics";
        public const string Hunting = "Hunting";
        public const string Advocacy = "Advocacy";
        public const string Mercantile = "Mercantile";
        public const string Tactics = "Tactics";
        public const string Travelling = "Travelling";
        public const string Sailing = "Sailing";
        public const string Medicine = "Medicine";

        public static readonly List<string> All =
        [
            Combat, 
            Arcane, 
            Alchemy, 
            Psionics, 
            Hunting, 
            Advocacy, 
            Mercantile, 
            Tactics, 
            Travelling, 
            Sailing, 
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
            public const int Strength   = 1;
            public const int Athletics  = 1;
            public const int Willpower  = 2;
            public const int Abstract   = 1;
            public const int Harm       = 1;
            public const int Fortitude  = 2;
            public const int Accretion  = 0;
            public const int Guile      = 2;
            public const int Awareness  = 1;
            public const int Charm      = 2;
            public const int Hitpoints  = 10;
            public const int Mana       = 0;
            public const int Actions    = 1;
            public const int Defense    = 0;
            public const int Resist     = 0;
            // crafts
            public const int Combat     = 1;
            public const int Arcane     = 1;
            public const int Alchemy    = 1;
            public const int Psionics   = 1;
            public const int Hunting    = 1;
            public const int Advocacy   = 2;
            public const int Mercantile = 2;
            public const int Tactics    = 1;
            public const int Travelling = 2;
            public const int Medicine   = 1;
            public const int Sail       = 1;
        }

        public const string Elf = "Elf";
        public static class Elves
        {
            // stats
            public const int Strength   = 1;
            public const int Athletics  = 2;
            public const int Willpower  = 1;
            public const int Abstract   = 2;
            public const int Harm       = 1;
            public const int Fortitude  = 1;
            public const int Accretion  = 3;
            public const int Guile      = 1;
            public const int Awareness  = 2;
            public const int Charm      = 0;
            public const int Hitpoints  = 8;
            public const int Mana       = 5;
            public const int Actions    = 1;
            public const int Defense    = 0;
            public const int Resist     = 0;
            // crafts
            public const int Combat     = 2;
            public const int Arcane     = 2;
            public const int Alchemy    = 0;
            public const int Psionics   = 1;
            public const int Hunting    = 2;
            public const int Advocacy   = 1;
            public const int Mercantile = 1;
            public const int Tactics    = 1;
            public const int Travelling = 1;
            public const int Medicine   = 1;
            public const int Sail       = 1;
        }

        public const string Dwarf = "Dwarf";
        public static class Dwarves
        {
            // stats
            public const int Strength   = 2;
            public const int Athletics  = 1;
            public const int Willpower  = 2;
            public const int Abstract   = 1;
            public const int Harm       = 3;
            public const int Fortitude  = 3;
            public const int Accretion  = 0;
            public const int Guile      = 0;
            public const int Awareness  = 0;
            public const int Charm      = -1;
            public const int Hitpoints  = 15;
            public const int Mana       = 0;
            public const int Actions    = 1;
            public const int Defense    = 0;
            public const int Resist     = 0;
            // crafts
            public const int Combat     = 2;
            public const int Arcane     = 0;
            public const int Alchemy    = 0;
            public const int Psionics   = 1;
            public const int Hunting    = 2;
            public const int Advocacy   = 1;
            public const int Mercantile = 2;
            public const int Tactics    = 1;
            public const int Travelling = 0;
            public const int Medicine   = 2;
            public const int Sail       = 0;
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
            public const int Strength   = 0;
            public const int Athletics  = 0;
            public const int Willpower  = 1;
            public const int Abstract   = 0;
            public const int Harm       = 0;
            public const int Fortitude  = 0;
            public const int Accretion  = 0;
            public const int Guile      = 0;
            public const int Awareness  = 0;
            public const int Charm      = 1;
            public const int Hitpoints  = 2;
            public const int Mana       = 0;
            public const int Actions    = 0;
            public const int Defense    = 2;
            public const int Resist     = 0;
            // crafts
            public const int Combat     = 1;
            public const int Arcane     = 0;
            public const int Alchemy    = 0;
            public const int Psionics   = 0;
            public const int Hunting    = 0;
            public const int Advocacy   = 1;
            public const int Mercantile = 1;
            public const int Tactics    = 1;
            public const int Travelling = 0;
            public const int Medicine   = 0;
            public const int Sail       = 0;
        }

        public const string Highborn = "Highborn";
        public static class Highborns
        {
            // stats
            public const int Strength   = 0;
            public const int Athletics  = 0;
            public const int Willpower  = 0;
            public const int Abstract   = 1;
            public const int Harm       = 0;
            public const int Fortitude  = 0;
            public const int Accretion  = 2;
            public const int Guile      = 0;
            public const int Awareness  = 0;
            public const int Charm      = 1;
            public const int Hitpoints  = 1;
            public const int Mana       = 10;
            public const int Actions    = 0;
            public const int Defense    = 0;
            public const int Resist     = 0;
            // crafts
            public const int Combat     = 1;
            public const int Arcane     = 1;
            public const int Alchemy    = 0;
            public const int Psionics   = 0;
            public const int Hunting    = 0;
            public const int Advocacy   = 1;
            public const int Mercantile = 0;
            public const int Tactics    = 1;
            public const int Travelling = 0;
            public const int Medicine   = 0;
            public const int Sail       = 0;
        }

        public const string Undermountain = "Undermountain";
        public static class Undermountains
        {
            // stats
            public const int Strength   = 1;
            public const int Athletics  = 0;
            public const int Willpower  = 1;
            public const int Abstract   = 0;
            public const int Harm       = 1;
            public const int Fortitude  = 1;
            public const int Accretion  = 0;
            public const int Guile      = 0;
            public const int Awareness  = 0;
            public const int Charm      = -1;
            public const int Hitpoints  = 5;
            public const int Mana       = 0;
            public const int Actions    = 0;
            public const int Defense    = 5;
            public const int Resist     = 5;
            // crafts
            public const int Combat     = 1;
            public const int Arcane     = 0;
            public const int Alchemy    = 0;
            public const int Psionics   = 0;
            public const int Hunting    = 1;
            public const int Advocacy   = 0;
            public const int Mercantile = 0;
            public const int Tactics    = 1;
            public const int Travelling = 0;
            public const int Medicine   = 0;
            public const int Sail       = 0;
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
            public const int Strength   = 1;
            public const int Athletics  = 0;
            public const int Willpower  = 0;
            public const int Abstract   = 0;
            public const int Harm       = 3;
            public const int Fortitude  = 3;
            public const int Accretion  = 0;
            public const int Guile      = 0;
            public const int Awareness  = 1;
            public const int Charm      = 0;
            public const int Hitpoints  = 15;
            public const int Mana       = 0;
            public const int Actions    = 0;
            public const int Defense    = 5;
            public const int Resist     = 0;
            // crafts
            public const int Combat     = 1;
            public const int Arcane     = 0;
            public const int Alchemy    = 0;
            public const int Psionics   = 0;
            public const int Hunting    = 0;
            public const int Advocacy   = 0;
            public const int Mercantile = 0;
            public const int Tactics    = 1;
            public const int Travelling = 0;
            public const int Medicine   = 1;
            public const int Sail       = 1;
        }

        public const string Sorcery = "Sorcery";
        public static class Sorcerys
        {
            // stats
            public const int Strength   = 0;
            public const int Athletics  = 0;
            public const int Willpower  = 0;
            public const int Abstract   = 2;
            public const int Harm       = 0;
            public const int Fortitude  = 2;
            public const int Accretion  = 4;
            public const int Guile      = 0;
            public const int Awareness  = 0;
            public const int Charm      = 1;
            public const int Hitpoints  = 2;
            public const int Mana       = 15;
            public const int Actions    = 0;
            public const int Defense    = 0;
            public const int Resist     = 5;
            // crafts
            public const int Combat     = 0;
            public const int Arcane     = 2;
            public const int Alchemy    = 1;
            public const int Psionics   = 0;
            public const int Hunting    = 0;
            public const int Advocacy   = 0;
            public const int Mercantile = 0;
            public const int Tactics    = 0;
            public const int Travelling = 0;
            public const int Medicine   = 0;
            public const int Sail       = 0;
        }

        public const string Tracking = "Tracking";
        public static class Trackings
        {
            // stats
            public const int Strength   = 0;
            public const int Athletics  = 1;
            public const int Willpower  = 0;
            public const int Abstract   = 0;
            public const int Harm       = 1;
            public const int Fortitude  = 2;
            public const int Accretion  = 0;
            public const int Guile      = 1;
            public const int Awareness  = 3;
            public const int Charm      = 0;
            public const int Hitpoints  = 5;
            public const int Mana       = 0;
            public const int Actions    = 0;
            public const int Defense    = 1;
            public const int Resist     = 0;
            // crafts
            public const int Combat     = 0;
            public const int Arcane     = 0;
            public const int Alchemy    = 0;
            public const int Psionics   = 0;
            public const int Hunting    = 1;
            public const int Advocacy   = 0;
            public const int Mercantile = 0;
            public const int Tactics    = 0;
            public const int Travelling = 1;
            public const int Medicine   = 1;
            public const int Sail       = 1;
        }

        public static readonly List<string> All =
        [
            Warring,
            Sorcery,
            Tracking
        ];
    }
}
