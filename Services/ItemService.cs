using Models;

namespace Services;

public interface IItemService
{
    Item GenerateRandomItem();
    Item GenerateSpecificItem(string type);
    List<Item> GenerateRandomItems(int amount);
    
    Trinket GenerateRandomTrinket();
}

public class ItemService : IItemService
{
    private readonly IDiceService _diceService;

    public ItemService(IDiceService diceService)
    {
        _diceService = diceService;
    }

    public Trinket GenerateRandomTrinket()
    {
        var trinket = new Trinket
        {
            Id = Guid.NewGuid(),
            Type = Statics.Items.Types.Trinket
        };

        SetLevel(trinket);
        SetType(trinket);
        SetIcon(trinket);
        SetStats(trinket);
        SetValue(trinket);
        SetName(trinket);

        return trinket;
    }

    public Item GenerateRandomItem()
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            HasTaint = _diceService.Roll_1dn(2) % 2 == 0,
        };

        SetLevel(item);
        SetType(item);
        SetIcon(item);
        SetStats(item);
        SetCrafts(item);
        SetValue(item);
        SetName(item);
        
        return item;
    }

    public Item GenerateSpecificItem(string type)
    {
        if (!Statics.Items.Types.All.Contains(type))
        {
            throw new Exception("Wrong item type provided.");
        }

        var item = new Item()
        {
            Id = Guid.NewGuid(),
            HasTaint = _diceService.Roll_1dn(2) % 2 == 0,
            Type = type
        };

        SetLevel(item);
        SetIcon(item);
        SetStats(item);
        SetCrafts(item);
        SetValue(item);
        SetName(item);

        return item;
    }

    public List<Item> GenerateRandomItems(int amount)
    {
        var items = new List<Item>();

        for (int i = 0; i < amount; i++)
        {
            items.Add(GenerateRandomItem());
        }

        return items;
    }

    #region private methods
    private void SetValue(Trinket trinket)
    {
        if (trinket.IsPermanent)
        {
            trinket.Value = 1 + trinket.Level * _diceService.Roll_mdn(100, 300);
        } 
        else
        {
            trinket.Value = 1 + trinket.Level * _diceService.Roll_mdn(1, 10);
        }
    }

    private void SetValue(Item item)
    {
        item.Value += 1 + _diceService.Roll_1dn(100) / 3;
    }

    private void SetStats(Trinket trinket)
    {
        var bonus = _diceService.Roll_1dn(trinket.Level) * trinket.Level;

        IncreaseRandomStat(bonus, trinket);
    }

    private void SetStats(Item item)
    {
        int bonusRoll;
        int times;

        #region set bonus and times
        if (item.Level == 1)
        {
            bonusRoll = _diceService.Roll_1dn(3);
            times = _diceService.Roll_1dn(3);
        }
        else if (item.Level == 2) // 1x20
        {
            bonusRoll = _diceService.Roll_1dn(6);
            times = _diceService.Roll_1dn(6);
        }
        else if (item.Level == 3) // 2x20
        {
            bonusRoll = _diceService.Roll_1dn(12);
            times = _diceService.Roll_1dn(12);
        }
        else if (item.Level == 4) // 3x20
        {
            bonusRoll = _diceService.Roll_1dn(24);
            times = _diceService.Roll_1dn(24);
        }
        else if (item.Level == 5) // 4x20
        {
            bonusRoll = _diceService.Roll_1dn(48);
            times = _diceService.Roll_1dn(48);
        }
        else // 5x20 and up
        {
            bonusRoll = _diceService.Roll_1dn(100);
            times = _diceService.Roll_1dn(100);
        }
        #endregion

        for (int i = 0; i < times; i++)
        {
            var bonus = _diceService.Roll_1dn(bonusRoll);

            switch (item.Type)
            {
                case Statics.Items.Types.Weapon:
                    item.Stats.Harm += 2 + bonus;
                    break;
                case Statics.Items.Types.Shield:
                    item.Stats.Harm += 1 + bonus / 3;
                    item.Stats.Defense += 1 + bonus / 2;
                    break;
                case Statics.Items.Types.Armour:
                    item.Stats.Defense += 2 + bonus;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (item.HasTaint)
            {
                IncreaseRandomStat(bonus, item);
            }
            else
            {
                switch (item.Type)
                {
                    case Statics.Items.Types.Weapon:
                        item.Stats.Harm += bonus;
                        item.Stats.Resist += bonus;
                        break;
                    case Statics.Items.Types.Shield:
                        item.Stats.Harm += bonus / 3;
                        item.Stats.Fortitude += bonus / 2;
                        item.Stats.Defense += bonus / 2;
                        item.Stats.Resist += bonus / 2;
                        break;
                    case Statics.Items.Types.Armour:
                        item.Stats.Fortitude += bonus;
                        item.Stats.Defense += bonus;
                        item.Stats.Resist += bonus / 2;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    private void SetCrafts(Item item)
    {
        int bonusRoll;
        int times;

        #region set bonus and times
        if (item.Level == 1)
        {
            bonusRoll = _diceService.Roll_1dn(3);
            times = _diceService.Roll_1dn(3);
        }
        else if (item.Level == 2) // 1x20
        {
            bonusRoll = _diceService.Roll_1dn(6);
            times = _diceService.Roll_1dn(6);
        }
        else if (item.Level == 3) // 2x20
        {
            bonusRoll = _diceService.Roll_1dn(12);
            times = _diceService.Roll_1dn(12);
        }
        else if (item.Level == 4) // 3x20
        {
            bonusRoll = _diceService.Roll_1dn(24);
            times = _diceService.Roll_1dn(24);
        }
        else if (item.Level == 5) // 4x20
        {
            bonusRoll = _diceService.Roll_1dn(48);
            times = _diceService.Roll_1dn(48);
        }
        else // 5x20 and up
        {
            bonusRoll = _diceService.Roll_1dn(100);
            times = _diceService.Roll_1dn(100);
        }
        #endregion

        for (int i = 0; i < times; i++)
        {
            var craft = _diceService.Roll_1dn(10); // accounts for nr of crafts
            var bonus = _diceService.Roll_1dn(bonusRoll);

            if (item.HasTaint)
            {
                switch (craft)
                {
                    case 1:
                        item.Crafts.Combat += bonus;
                        break;
                    case 2:
                        item.Crafts.Arcane += bonus;
                        break;
                    case 3:
                        item.Crafts.Alchemy += bonus;
                        break;
                    case 4:
                        item.Crafts.Psionics += bonus;
                        break;
                    case 5:
                        item.Crafts.Hunting += bonus;
                        break;
                    case 6:
                        item.Crafts.Advocacy += bonus;
                        break;
                    case 7:
                        item.Crafts.Mercantile += bonus;
                        break;
                    case 8:
                        item.Crafts.Tactics += bonus;
                        break;
                    case 9:
                        item.Crafts.Sailing += bonus;
                        break;
                    case 10:
                        item.Crafts.Medicine += bonus;
                        break;
                    case 11:
                        item.Crafts.Travelling += bonus;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                switch (item.Type)
                {
                    case Statics.Items.Types.Weapon:
                        item.Crafts.Combat += bonus / 3;
                        item.Crafts.Psionics += bonus / 3;
                        break;
                    case Statics.Items.Types.Shield:
                        item.Crafts.Combat += bonus / 3;
                        item.Stats.Fortitude += bonus / 3;
                        break;
                    case Statics.Items.Types.Armour:
                        item.Crafts.Combat += bonus / 3;
                        item.Stats.Hitpoints += bonus / 3;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    private static void SetIcon(Item item)
    {
        if (item.Type == Statics.Items.Types.Weapon)
        {
            item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_05.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_04.jpg";
        }
        else if (item.Type == Statics.Items.Types.Shield)
        {
            item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_06.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_shield_04.jpg";
        }
        else if (item.Type == Statics.Items.Types.Armour)
        {
            item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate09.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate04.jpg";
        }
        else
        {
            throw new NotImplementedException(); 
        }
    }

    private static void SetIcon(Trinket trinket)
    {
        if (trinket.IsPermanent)
        {
            trinket.Icon = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_vendor_witchberries.jpg";
        }
        else
        {
            trinket.Icon = trinket.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_04.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_02.jpg";
        }
    }

    private void SetType(Item item)
    {
        var roll = _diceService.Roll_d20_no_rr();

        if (roll <= 10)
        {
            item.Type = Statics.Items.Types.Weapon;
        }
        else if (roll > 10 && roll <= 16)
        {
            item.Type = Statics.Items.Types.Shield;
        }
        else
        {
            item.Type = Statics.Items.Types.Armour;
        }
    }

    private static void SetType(Trinket trinket)
    {
        trinket.Type = Statics.Items.Types.Trinket;
    }

    private void SetName(Item item)
    {
        if (item.Type == Statics.Items.Types.Weapon)
        {
            var index = _diceService.Roll_1dn(Statics.Items.Weapons.All.Count) - 1;
            item.Name = Statics.Items.Weapons.All[index];
        }
        else if (item.Type == Statics.Items.Types.Shield)
        {
            var index = _diceService.Roll_1dn(Statics.Items.Shields.All.Count) - 1;
            item.Name = Statics.Items.Shields.All[index];
        }
        else if (item.Type == Statics.Items.Types.Armour)
        {
            var index = _diceService.Roll_1dn(Statics.Items.Armours.All.Count) - 1;
            item.Name = Statics.Items.Armours.All[index];
        }
    }

    private static void SetName(Trinket trinket)
    {
        if (trinket!.Stats.Hitpoints > 0)
        {
            trinket!.Name = "Trinket of Life";
        }
        else if (trinket!.Stats.Mana > 0)
        {
            trinket!.Name = "Trinket of Stars";
        }
        else if (trinket!.Stats.Apcom > 0)
        {
            trinket!.Name = "Trinket of Swiftness";
        }
        else if (trinket!.Stats.Resist > 0)
        {
            trinket!.Name = "Trinket of Endure";
        }
        else if (trinket!.Stats.Defense > 0)
        {
            trinket!.Name = "Trinket of Royalty";
        }
        else
        {
            trinket!.Name = "Trinket of Crowns";
        }
    }

    private void SetLevel(Item item)
    {
        var roll = _diceService.Roll_d20();
        item.Level = 1 + roll / 20;
    }

    private void SetLevel(Trinket trinket)
    {
        var roll = _diceService.Roll_d20();
        trinket.Level = 1 + roll / 20;
        trinket.IsPermanent = roll >= 60;
    }

    private void IncreaseRandomStat(int bonus, Item item)
    {
        var stat = _diceService.Roll_1dn(15); // accounts for the nr of stats

        switch (stat)
        {
            case 1:
                item.Stats.Strength += bonus;
                break;
            case 2:
                item.Stats.Athletics += bonus;
                break;
            case 3:
                item.Stats.Willpower += bonus;
                break;
            case 4:
                item.Stats.Abstract += bonus;
                break;
            case 5:
                item.Stats.Harm += bonus;
                break;
            case 6:
                item.Stats.Fortitude += bonus;
                break;
            case 7:
                item.Stats.Accretion += bonus;
                break;
            case 8:
                item.Stats.Guile += bonus;
                break;
            case 9:
                item.Stats.Awareness += bonus;
                break;
            case 10:
                item.Stats.Charm += bonus;
                break;
            case 11:
                item.Stats.Apcom += bonus;
                break;
            case 12:
                item.Stats.Defense += bonus;
                break;
            case 13:
                item.Stats.Resist += bonus;
                break;
            case 14:
                item.Stats.Hitpoints += bonus;
                break;
            case 15:
                item.Stats.Mana += bonus;
                break;
            default:
                throw new NotImplementedException();
        }
    }
    #endregion
}