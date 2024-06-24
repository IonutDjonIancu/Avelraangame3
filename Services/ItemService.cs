using Models;

namespace Services;

public interface IItemService
{
    Item GenerateRandomItem();
    Item GenerateSpecificItem(string type);
    List<Item> GenerateRandomItems(int amount);
}

public class ItemService : IItemService
{
    private readonly IDiceService _diceService;

    public ItemService(IDiceService diceService)
    {
        _diceService = diceService;
    }

    public Item GenerateRandomItem()
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            HasTaint = _diceService.Roll_1dn(2) % 2 == 0,
        };

        SetType(item);
        
        return GenerateSpecificItem(item.Type);
    }

    public Item GenerateSpecificItem(string type)
    {
        if (!Statics.Items.Types.All.Contains(type))
        {
            throw new Exception("Wrong item type provided.");
        }

        var item = type == Statics.Items.Types.Trinket ? new Trinket() : new Item();

        item.Id = Guid.NewGuid();
        item.HasTaint = _diceService.Roll_1dn(2) % 2 == 0;
        item.Type = type;   

        SetLevel(item);
        SetIcon(item);
        SetStats(item);
        SetFeats(item);
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
    private void SetValue(Item item)
    {
        if (item.HasTaint)
        {
            item.Value += 1 + _diceService.Roll_1dn(10);
        }
        else
        {
            item.Value += 1 + _diceService.Roll_1dn(20) * 3;
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
        else if (item.Type == Statics.Items.Types.Trinket)
        {
            if (((Trinket)item).IsPermanent)
            {
                item.Icon = "https://wow.zamimg.com/images/wow/icons/large/inv_misc_food_vendor_witchberries.jpg";
            }
            else
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_04.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_02.jpg";
            }
        }
        else
        {
            throw new NotImplementedException(); 
        }
    }

    private void SetType(Item item)
    {
        var roll = _diceService.Roll_d20_no_rr();

        if (roll <= 8)
        {
            item.Type = Statics.Items.Types.Weapon; // 40%
        }
        else if (roll > 8 && roll <= 14)
        {
            item.Type = Statics.Items.Types.Shield; // 30%
        }
        else if (roll > 14 && roll <= 18)
        {
            item.Type = Statics.Items.Types.Trinket; // 20%
        }
        else if (roll > 18)
        {
            item.Type = Statics.Items.Types.Armour; // 10%
        }
        else // lol
        {
            throw new NotImplementedException();
        }
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
        else if (item.Type == Statics.Items.Types.Trinket)
        {
            var index = _diceService.Roll_1dn(Statics.Items.Trinkets.All.Count) - 1;
            item.Name = Statics.Items.Trinkets.All[index];
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void SetLevel(Item item)
    {
        var roll = _diceService.Roll_d20();
        item.Level = 1 + roll / 20;

        if (item.Type == Statics.Items.Types.Trinket)
        {
            ((Trinket)item).IsPermanent = roll >= 60;
        }
    }

    private void SetStats(Item item)
    {
        var rollTimes = _diceService.Roll_1dn(5 + item.Level);

        for (int i = 0; i < rollTimes; i++)
        {
            if (item.Type == Statics.Items.Types.Weapon)
            {
                // flat bonus
                item.Feats.Combat += _diceService.Roll_1dn(2 + item.Level);
                item.Feats.CombatEff += _diceService.Roll_1dn(2 + item.Level);
                // taint notaint bonus
                if (item.HasTaint)
                {
                    item.Feats.Abstract += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.AbstractEff += _diceService.Roll_1dn(3 + item.Level);
                    item.Stats.Accretion += _diceService.Roll_1dn(3 + item.Level);
                    item.Stats.Resist -= _diceService.Roll_1dn(2 + item.Level);
                }
                else
                {
                    item.Feats.Psionic += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.PsionicEff += _diceService.Roll_1dn(3 + item.Level);
                    item.Stats.Resist += _diceService.Roll_1dn(2 + item.Level);
                }
            }
            else if (item.Type == Statics.Items.Types.Trinket)
            {
                // flat bonus
                item.Value += _diceService.Roll_d20();
                // taint notaint bonus
                if (item.HasTaint)
                {
                    item.Feats.Abstract += _diceService.Roll_1dn(2 + item.Level);
                    item.Feats.AbstractEff += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Accretion += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Resist -= _diceService.Roll_1dn(2 + item.Level);
                }
                else
                {
                    item.Feats.Crafting += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.CraftingEff += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.Medicine += _diceService.Roll_1dn(2 + item.Level);
                    item.Feats.MedicineEff += _diceService.Roll_1dn(2 + item.Level);
                }
            }
            else if (item.Type == Statics.Items.Types.Shield)
            {
                // flat bonus
                item.Stats.Defense += _diceService.Roll_1dn(3 + item.Level);
                item.Stats.Resist += _diceService.Roll_1dn(3 + item.Level);
                // taint notaint bonus
                if (item.HasTaint)
                {
                    item.Feats.Abstract += _diceService.Roll_1dn(2 + item.Level);
                    item.Feats.AbstractEff += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Accretion += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Resist -= _diceService.Roll_1dn(2 + item.Level);
                }
                else
                {
                    item.Feats.Psionic += _diceService.Roll_1dn(2 + item.Level);
                    item.Feats.PsionicEff += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Resist += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.CombatEff += _diceService.Roll_1dn(2 + item.Level);
                }
            }
            else if (item.Type == Statics.Items.Types.Armour)
            {
                // flat bonus
                item.Stats.Defense += _diceService.Roll_1dn(3 + item.Level);
                item.Stats.Endurance += _diceService.Roll_1dn(5 + item.Level);
                // taint notaint bonus
                if (item.HasTaint)
                {
                    item.Feats.Abstract += _diceService.Roll_1dn(2 + item.Level);
                    item.Feats.AbstractEff += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Accretion += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Resist -= _diceService.Roll_1dn(2 + item.Level);
                }
                else
                {
                    item.Feats.Psionic += _diceService.Roll_1dn(2 + item.Level);
                    item.Feats.PsionicEff += _diceService.Roll_1dn(2 + item.Level);
                    item.Stats.Resist += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.CombatEff += _diceService.Roll_1dn(2 + item.Level);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            var statNr = _diceService.Roll_1dn(Statics.Stats.All.Count); // accounts for the nr of stats
            switch (statNr)
            {
                case 1:
                    item.Stats.Defense += _diceService.Roll_1dn(2 + item.Level);
                    break;
                case 2:
                    item.Stats.Resist += _diceService.Roll_1dn(2 + item.Level);
                    break;
                case 3:
                    item.Stats.Actions += _diceService.Roll_1dn(2 + item.Level);
                    break;
                case 4:
                    item.Stats.Endurance += _diceService.Roll_1dn(5 + item.Level);
                    break;
                case 5:
                    item.Stats.Accretion += _diceService.Roll_1dn(10 + item.Level);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void SetFeats(Item item)
    {
        var rollTimes = _diceService.Roll_1dn(5 + item.Level);

        for (int i = 0; i < rollTimes; i++)
        {
            var featNr = _diceService.Roll_1dn(Statics.Feats.All.Count); // accounts for the nr of stats
            switch (featNr)
            {
                case 1:
                    item.Feats.Combat += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.CombatEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 2:
                    item.Feats.Strength += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.StrengthEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 3:
                    item.Feats.Tactics += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.TacticsEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 4:
                    item.Feats.Athletics += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.AthleticsEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 5:
                    item.Feats.Survival += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.SurvivalEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 6:
                    item.Feats.Social += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.SocialEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 7:
                    item.Feats.Abstract += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.AbstractEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 8:
                    item.Feats.Psionic += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.PsionicEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 9:
                    item.Feats.Crafting += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.CraftingEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                case 10:
                    item.Feats.Medicine += _diceService.Roll_1dn(3 + item.Level);
                    item.Feats.MedicineEff += _diceService.Roll_1dn(3 + item.Level);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
    #endregion
}