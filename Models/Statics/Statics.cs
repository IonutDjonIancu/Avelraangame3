namespace Models;

public static class Statics
{
    public static class Items
    {
        public static class Weapons 
        {
            public const string Sword = "Arming sword";
            public const string LongSword = "Long sword";
            public const string ShortSword = "Short sword";
            public const string Uchigatana = "Uchigatana";
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
                Sword, LongSword, ShortSword, Uchigatana, Falx, Axe, DaneAxe, Mace, Warhammer, Spear, Polearm, Staff, Dagger, Seax
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
    }

    public enum ItemType
    {
        Weapon,
        Shield,
        Armour
    }

    public static class Races
    {
        public const string Human = "human";
        public static class Humans
        {
            // stats
            public const int Strength   = 1;
            public const int Athletics  = 1;
            public const int Willpower  = 2;
            public const int Abstract   = 1;
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
        }

        public const string Elf = "elf";
        public static class Elves
        {
            // stats
            public const int Strength   = 1;
            public const int Athletics  = 2;
            public const int Willpower  = 1;
            public const int Abstract   = 2;
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
        }

        public const string Dwarf = "dwarf";
        public static class Dwarves
        {
            // stats
            public const int Strength   = 2;
            public const int Athletics  = 1;
            public const int Willpower  = 2;
            public const int Abstract   = 1;
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
        public const string Danarian = "danarian";
        public static class Danarians
        {
            // stats
            public const int Strength   = 0;
            public const int Athletics  = 0;
            public const int Willpower  = 1;
            public const int Abstract   = 0;
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
        }

        public const string Highborn = "highborn";
        public static class Highborns
        {
            // stats
            public const int Strength   = 0;
            public const int Athletics  = 0;
            public const int Willpower  = 0;
            public const int Abstract   = 1;
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
        }

        public const string Undermountain = "undermountain";
        public static class Undermountains
        {
            // stats
            public const int Strength   = 1;
            public const int Athletics  = 0;
            public const int Willpower  = 1;
            public const int Abstract   = 0;
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
        public const string Warring = "warring";
        public static class Warrings
        {
            // stats
            public const int Strength   = 1;
            public const int Athletics  = 0;
            public const int Willpower  = 0;
            public const int Abstract   = 0;
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
        }

        public const string Sorcery = "sorcery";
        public static class Sorcerys
        {
            // stats
            public const int Strength   = 0;
            public const int Athletics  = 0;
            public const int Willpower  = 0;
            public const int Abstract   = 2;
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
        }

        public const string Tracker = "tracker";
        public static class Trackers
        {
            // stats
            public const int Strength   = 0;
            public const int Athletics  = 1;
            public const int Willpower  = 0;
            public const int Abstract   = 0;
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
        }

        public static readonly List<string> All =
        [
            Warring,
            Sorcery,
            Tracker
        ];
    }

}
