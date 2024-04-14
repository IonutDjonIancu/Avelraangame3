using Models;

namespace Services;

public interface IItemService
{
    Item GenerateRandomItem();
    Item GenerateSpecificItem(string type);
    HashSet<Item> GenerateRandomItems(int amount);
    
    Trinket GenerateRandomTrinket();
}

public class ItemService(IDiceService diceService) : IItemService
{
    private readonly IDiceService _dice = diceService;

    public Trinket GenerateRandomTrinket()
    {
        var trinket = new Trinket
        {
            Id = Guid.NewGuid(),
        };

        SetLevel(null, trinket);
        SetName(null, trinket);
        SetTrinketAssets(trinket);
        SetTrinketValue(trinket);

        return trinket;
    }

    public Item GenerateRandomItem()
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            HasTaint = _dice.Roll_1dn(2) % 2 == 0,
        };

        SetLevel(item, null);
        SetType(item);
        SetName(item, null);
        SetCrafts(item);
        SetStats(item);
        SetItemAssets(item);
        SetItemValue(item);

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
            HasTaint = _dice.Roll_1dn(2) % 2 == 0,
            Type = type
        };

        SetLevel(item, null);
        SetName(item, null);
        SetCrafts(item);
        SetStats(item);
        SetItemAssets(item);
        SetItemValue(item);

        return item;
    }

    public HashSet<Item> GenerateRandomItems(int amount)
    {
        var items = new HashSet<Item>();

        for (int i = 0; i < amount; i++)
        {
            items.Add(GenerateRandomItem());
        }

        return items;
    }

    #region private methods
    private void SetTrinketValue(Trinket trinket)
    {
        if (trinket.IsPermanent)
        {
            trinket.Value = trinket.Level * _dice.Roll_mdn(100, 300);
        } 
        else
        {
            trinket.Value = trinket.Level / 10;
        }
    }

    private void SetItemValue(Item item)
    {
        if (item.HasTaint)
        {
            item.Value += _dice.Roll_1dn(item.Level) / 3;
        }
        else
        {
            item.Value += _dice.Roll_1dn(item.Level) / 2;
        }
    }

    private void SetTrinketAssets(Trinket trinket)
    {
        if (trinket.Level <= 40)
        {
            var oddEvenRoll = _dice.Roll_1dn(2);

            if (oddEvenRoll % 2 == 0)
            {
                trinket.Assets.HitpointsBase += _dice.Roll_1dn(trinket.Level);
            }
            else
            {
                trinket.Assets.ManaBase += _dice.Roll_1dn(trinket.Level);
            }

            return;
        }

        var stat = _dice.Roll_1dn(6); // accounts for the nr of assets
        
        switch (stat)
        {
            case 1:
                trinket.Assets.HitpointsBase += _dice.Roll_1dn(trinket.Level) * 2;
                break;
            case 2:
                trinket.Assets.ManaBase += _dice.Roll_1dn(trinket.Level) * 3;
                break;
            case 3:
                trinket.Assets.DefenseBase += _dice.Roll_1dn(trinket.Level) / 2;
                break;
            case 4:
                trinket.Assets.ActionsBase += _dice.Roll_1dn(trinket.Level) / 10;
                break;
            case 5:
                trinket.Assets.ResistBase += _dice.Roll_1dn(trinket.Level) / 2;
                break;
            case 6:
                trinket.Assets.ReflexBase += _dice.Roll_1dn(trinket.Level) / 10;
                break;
            default:
                throw new NotImplementedException();
        }

    }

    private void SetItemAssets(Item item)
    {
        int times;

        #region set bonus and times
        if (item.Level <= 39)
        {
            // do nothing
            return;
        }
        else if (item.Level >= 40 && item.Level <= 59) // 2x20
        {
            times = _dice.Roll_1dn(5);
        }
        else if (item.Level >= 60 && item.Level <= 79) // 3x20
        {
            times = _dice.Roll_1dn(10);
        }
        else if (item.Level >= 80 && item.Level <= 99) // 4x20
        {
            times = _dice.Roll_1dn(20);
        }
        else // 5x20 and up
        {
            times = _dice.Roll_1dn(40);
        }
        #endregion

        for (int i = 0; i < times; i++)
        {
            var stat = _dice.Roll_1dn(6); // accounts for the nr of assets
            int bonus;

            switch (stat)
            {
                case 1:
                    bonus = item.HasTaint ? _dice.Roll_1dn(5 * item.Level) + item.Level : _dice.Roll_1dn(10);
                    if (item.HasTaint)
                    {
                        item.Assets.HitpointsBase += bonus;
                    }
                    else
                    {
                        item.Assets.ResistBase += bonus;
                    }
                    break;
                case 2:
                    bonus = item.HasTaint ? _dice.Roll_1dn(10 * item.Level) : _dice.Roll_1dn(50);
                    if (item.HasTaint)
                    {
                        item.Assets.ManaBase += bonus;
                    }
                    else
                    {
                        item.Assets.ManaBase -= bonus;
                    }
                    break;
                case 3:
                    if (item.Type == Statics.Items.Types.Weapon) break;
                    
                    bonus = item.HasTaint ? _dice.Roll_1dn(item.Level / 10) : _dice.Roll_1dn(item.Level / 5);
                    if (item.HasTaint)
                    {
                        item.Assets.DefenseBase += bonus;
                    }
                    else
                    {
                        item.Assets.DefenseBase += bonus;
                    }
                    break;
                case 4:
                    bonus = item.HasTaint ? _dice.Roll_1dn(item.Level / 10) : _dice.Roll_1dn(20);
                    if (item.HasTaint)
                    {
                        item.Assets.ActionsBase += bonus;
                    }
                    else
                    {
                        item.Assets.ResistBase += bonus;
                    }
                    break;
                case 5:
                    bonus = item.HasTaint ? _dice.Roll_1dn(item.Level / 10) : _dice.Roll_1dn(20);
                    if (item.HasTaint)
                    {
                        item.Assets.ResistBase -= bonus;
                    }
                    else
                    {
                        item.Assets.ResistBase += bonus;
                    }
                    break;
                case 6:
                    bonus = _dice.Roll_1dn(item.Level / 10);
                    if (item.HasTaint)
                    {
                        item.Assets.ReflexBase += bonus;
                    }
                    else
                    {
                        item.Assets.ResistBase += bonus;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void SetStats(Item item)
    {
        int bonusRoll;
        int times;

        #region set bonus and times
        if (item.Level <= 19)
        {
            // do nothing
            return;
        }
        else if (item.Level >= 20 && item.Level <= 39) // 1x20
        {
            bonusRoll = _dice.Roll_1dn(3);
            times = _dice.Roll_1dn(3);
        }
        else if (item.Level >= 40 && item.Level <= 59) // 2x20
        {
            bonusRoll = _dice.Roll_1dn(5);
            times = _dice.Roll_1dn(5);
        }
        else if (item.Level >= 60 && item.Level <= 79) // 3x20
        {
            bonusRoll = _dice.Roll_1dn(10);
            times = _dice.Roll_1dn(10);
        }
        else if (item.Level >= 80 && item.Level <= 99) // 4x20
        {
            bonusRoll = _dice.Roll_1dn(20);
            times = _dice.Roll_1dn(20);
        }
        else // 5x20 and up
        {
            bonusRoll = _dice.Roll_1dn(40);
            times = _dice.Roll_1dn(40);
        }
        #endregion

        for (int i = 0; i < times; i++)
        {
            var stat = _dice.Roll_1dn(4); // accounts for the nr of stats
            var bonus = _dice.Roll_1dn(bonusRoll);

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
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void SetCrafts(Item item)
    {
        int bonusRoll;
        int times;

        #region set bonus and times
        if (item.Level <= 19)
        {
            bonusRoll = _dice.Roll_1dn(3);
            times = _dice.Roll_1dn(3);
        }
        else if (item.Level >= 20 && item.Level <= 39) // 1x20
        {
            bonusRoll = _dice.Roll_1dn(5);
            times = _dice.Roll_1dn(10);
        }
        else if (item.Level >= 40 && item.Level <= 59) // 2x20
        {
            bonusRoll = _dice.Roll_1dn(9);
            times = _dice.Roll_1dn(20);
        }
        else if (item.Level >= 60 && item.Level <= 79) // 3x20
        {
            bonusRoll = _dice.Roll_1dn(15);
            times = _dice.Roll_1dn(30);
        }
        else if (item.Level >= 80 && item.Level <= 99) // 4x20
        {
            bonusRoll = _dice.Roll_1dn(25);
            times = _dice.Roll_1dn(40);
        }
        else // 5x20 and up
        {
            bonusRoll = _dice.Roll_1dn(50);
            times = _dice.Roll_1dn(60);
        }
        #endregion

        for (int i = 0; i < times; i++)
        {
            var craft = _dice.Roll_1dn(10); // accounts for nr of crafts
            var bonus = _dice.Roll_1dn(bonusRoll);

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
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void SetType(Item item)
    {
        var roll = _dice.Roll_d20();

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

    private void SetName(Item? item, Trinket? trinket)
    {
        Validators.ValidateTrinketOrItemNotNull(item, trinket);

        if (item is null)
        {
            trinket!.Name = $"Trinket {trinket.Id.ToString()[..7]}";
        }
        else
        {
            if (item.Type == Statics.Items.Types.Weapon)
            {
                var index = _dice.Roll_1dn(Statics.Items.Weapons.All.Count) - 1;
                item.Name = Statics.Items.Weapons.All[index];
            }
            else if (item.Type == Statics.Items.Types.Shield)
            {
                var index = _dice.Roll_1dn(Statics.Items.Shields.All.Count) - 1;
                item.Name = Statics.Items.Shields.All[index];
            }
            else if (item.Type == Statics.Items.Types.Armour)
            {
                var index = _dice.Roll_1dn(Statics.Items.Armours.All.Count) - 1;
                item.Name = Statics.Items.Armours.All[index];
            }
        }
    }

    private void SetLevel(Item? item, Trinket? trinket)
    {
        Validators.ValidateTrinketOrItemNotNull(item, trinket);

        var roll = _dice.Roll_d20_rr();

        if (item is null)
        {
            trinket!.Level = roll;
            trinket.IsPermanent = roll >= 60;
        } 
        else
        {
            item.Level = roll;
        }
    }
    #endregion
}