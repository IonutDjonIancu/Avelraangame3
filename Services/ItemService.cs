using Models;

namespace Services;

public interface IItemService
{
    Item GenerateRandomItem();
    Item GenerateSpecificItem(Statics.ItemType type);
    HashSet<Item> GenerateRandomItems(int amount);
}

public class ItemService(IDiceService diceService) : IItemService
{
    private readonly IDiceService _dice = diceService;

    public Item GenerateRandomItem()
    {
        var item = new Item()
        {
            Id = Guid.NewGuid(),
            HasTaint = _dice.Roll_1dn(2) % 2 == 0,
        };

        SetItemLevel(item);
        SetType(item);
        SetItemName(item);
        SetCrafts(item);
        SetStats(item);
        SetAssets(item);
        SetValue(item);

        return item;
    }

    public Item GenerateSpecificItem(Statics.ItemType type)
    {
        if (type != Statics.ItemType.Weapon
            || type != Statics.ItemType.Armour
            || type != Statics.ItemType.Shield)
        {
            throw new Exception("Wrong item type provided.");
        }

        var item = new Item()
        {
            Id = Guid.NewGuid(),
            HasTaint = _dice.Roll_1dn(2) % 2 == 0,
            Type = type
        };

        SetItemLevel(item);
        SetItemName(item);
        SetCrafts(item);
        SetStats(item);
        SetAssets(item);
        SetValue(item);

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
    private void SetValue(Item item)
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

    private void SetAssets(Item item)
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
            int bonusRoll;

            switch (stat)
            {
                case 1:
                    bonusRoll = item.HasTaint ? _dice.Roll_1dn(5 * item.Level) + item.Level : _dice.Roll_1dn(10);
                    if (item.HasTaint)
                    {
                        item.Assets.Hitpoints += bonusRoll;
                    }
                    else
                    {
                        item.Assets.Resist += bonusRoll;
                    }
                    break;
                case 2:
                    bonusRoll = item.HasTaint ? _dice.Roll_1dn(10 * item.Level) : _dice.Roll_1dn(50);
                    if (item.HasTaint)
                    {
                        item.Assets.Mana += bonusRoll;
                    }
                    else
                    {
                        item.Assets.Mana -= bonusRoll;
                    }
                    break;
                case 3:
                    if (item.Type == Statics.ItemType.Weapon) break;
                    
                    bonusRoll = item.HasTaint ? _dice.Roll_1dn(item.Level / 10) : _dice.Roll_1dn(item.Level / 5);
                    if (item.HasTaint)
                    {
                        item.Assets.Defense += bonusRoll;
                    }
                    else
                    {
                        item.Assets.Defense += bonusRoll;
                    }
                    break;
                case 4:
                    bonusRoll = item.HasTaint ? _dice.Roll_1dn(item.Level / 10) : _dice.Roll_1dn(20);
                    if (item.HasTaint)
                    {
                        item.Assets.Actions += bonusRoll;
                    }
                    else
                    {
                        item.Assets.Resist += bonusRoll;
                    }
                    break;
                case 5:
                    bonusRoll = item.HasTaint ? _dice.Roll_1dn(item.Level / 10) : _dice.Roll_1dn(20);
                    if (item.HasTaint)
                    {
                        item.Assets.Resist -= bonusRoll;
                    }
                    else
                    {
                        item.Assets.Resist += bonusRoll;
                    }
                    break;
                case 6:
                    bonusRoll = _dice.Roll_1dn(item.Level / 10);
                    if (item.HasTaint)
                    {
                        item.Assets.Reflex += bonusRoll;
                    }
                    else
                    {
                        item.Assets.Resist += bonusRoll;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void SetStats(Item item)
    {
        int bonus;
        int times;

        #region set bonus and times
        if (item.Level <= 19)
        {
            // do nothing
            return;
        }
        else if (item.Level >= 20 && item.Level <= 39) // 1x20
        {
            bonus = _dice.Roll_1dn(3);
            times = _dice.Roll_1dn(3);
        }
        else if (item.Level >= 40 && item.Level <= 59) // 2x20
        {
            bonus = _dice.Roll_1dn(5);
            times = _dice.Roll_1dn(5);
        }
        else if (item.Level >= 60 && item.Level <= 79) // 3x20
        {
            bonus = _dice.Roll_1dn(10);
            times = _dice.Roll_1dn(10);
        }
        else if (item.Level >= 80 && item.Level <= 99) // 4x20
        {
            bonus = _dice.Roll_1dn(20);
            times = _dice.Roll_1dn(20);
        }
        else // 5x20 and up
        {
            bonus = _dice.Roll_1dn(40);
            times = _dice.Roll_1dn(40);
        }
        #endregion

        for (int i = 0; i < times; i++)
        {
            var stat = _dice.Roll_1dn(4); // accounts for the nr of stats
            var bonusRoll = _dice.Roll_1dn(bonus);

            switch (stat)
            {
                case 1:
                    item.Stats.Strength += bonusRoll;
                    break;
                case 2:
                    item.Stats.Athletics += bonusRoll;
                    break;
                case 3:
                    item.Stats.Willpower += bonusRoll;
                    break;
                case 4:
                    item.Stats.Abstract += bonusRoll;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void SetCrafts(Item item)
    {
        int bonus;
        int times;

        #region set bonus and times
        if (item.Level <= 19)
        {
            bonus = _dice.Roll_1dn(3);
            times = _dice.Roll_1dn(3);
        }
        else if (item.Level >= 20 && item.Level <= 39) // 1x20
        {
            bonus = _dice.Roll_1dn(5);
            times = _dice.Roll_1dn(10);
        }
        else if (item.Level >= 40 && item.Level <= 59) // 2x20
        {
            bonus = _dice.Roll_1dn(9);
            times = _dice.Roll_1dn(20);
        }
        else if (item.Level >= 60 && item.Level <= 79) // 3x20
        {
            bonus = _dice.Roll_1dn(15);
            times = _dice.Roll_1dn(30);
        }
        else if (item.Level >= 80 && item.Level <= 99) // 4x20
        {
            bonus = _dice.Roll_1dn(25);
            times = _dice.Roll_1dn(40);
        }
        else // 5x20 and up
        {
            bonus = _dice.Roll_1dn(50);
            times = _dice.Roll_1dn(60);
        }
        #endregion

        for (int i = 0; i < times; i++)
        {
            var craft = _dice.Roll_1dn(10); // accounts for nr of crafts
            var bonusRoll = _dice.Roll_1dn(bonus);

            switch (craft)
            {
                case 1:
                    item.Crafts.Combat += bonusRoll;
                    break;
                case 2:
                    item.Crafts.Arcane += bonusRoll;
                    break;
                case 3:
                    item.Crafts.Alchemy += bonusRoll;
                    break;
                case 4:
                    item.Crafts.Psionics += bonusRoll;
                    break;
                case 5:
                    item.Crafts.Hunting += bonusRoll;
                    break;
                case 6:
                    item.Crafts.Advocacy += bonusRoll;
                    break;
                case 7:
                    item.Crafts.Mercantile += bonusRoll;
                    break;
                case 8:
                    item.Crafts.Tactics += bonusRoll;
                    break;
                case 9:
                    item.Crafts.Sailing += bonusRoll;
                    break;
                case 10:
                    item.Crafts.Medicine += bonusRoll;
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
            item.Type = Statics.ItemType.Weapon;
        }
        else if (roll > 10 && roll <= 16)
        {
            item.Type = Statics.ItemType.Shield;
        }
        else
        {
            item.Type = Statics.ItemType.Armour;
        }
    }

    private void SetItemName(Item item)
    {
        if (item.Type == Statics.ItemType.Weapon)
        {
            var index = _dice.Roll_1dn(Statics.Items.Weapons.All.Count) - 1;
            item.Name = Statics.Items.Weapons.All[index];
        }
        else if (item.Type == Statics.ItemType.Shield)
        {
            var index = _dice.Roll_1dn(Statics.Items.Shields.All.Count) - 1;
            item.Name = Statics.Items.Shields.All[index];
        }
        else
        {
            var index = _dice.Roll_1dn(Statics.Items.Armours.All.Count) - 1;
            item.Name = Statics.Items.Armours.All[index];
        }
    }

    private void SetItemLevel(Item item)
    {
        item.Level = _dice.Roll_d20rr();
    }
    #endregion
}