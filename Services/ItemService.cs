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
        if (item.HasTaint)
        {
            item.Value += 1 + _diceService.Roll_1dn(10);
        }
        else
        {
            item.Value += 1 + _diceService.Roll_1dn(20) * 3;
        }
    }

    private void SetStats(Item item)
    {
        if (item.HasTaint)
        {
            IncreaseRandomStatForTaintedItem(item);
        }
        else
        {
            IncreaseRandomStatForNonTaintedItem(item);
        }
    }

    private void SetFeats(Item item)
    {
        if (item.HasTaint)
        {
            IncreaseRandomFeatForTaintedItem(item);
        }
        else
        {
            IncreaseRandomFeatForNonTaintedItem(item);
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

    private void IncreaseRandomStatForNonTaintedItem(Item item)
    {
        var rollTimes = _diceService.Roll_1dn(5) * item.Level;

        for (int i = 0; i < rollTimes; i++)
        {
            var statNr = _diceService.Roll_1dn(Statics.Stats.All.Count); // accounts for the nr of stats
            switch (statNr)
            {
                // defense
                case 1:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Combat += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                        item.Value += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Value += _diceService.Roll_1dn(10) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(7) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                        item.Value += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(20) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.PsionicEff += _diceService.Roll_1dn(5) * item.Level;
                        item.Value += _diceService.Roll_1dn(25) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // resist
                case 2:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Resist += _diceService.Roll_1dn(5) * item.Level;
                        item.Value += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Resist += _diceService.Roll_1dn(5) * item.Level;
                        item.Value += _diceService.Roll_1dn(10) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(3) * item.Level;
                        item.Stats.Resist += _diceService.Roll_1dn(10) * item.Level;
                        item.Value += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Resist += _diceService.Roll_1dn(10) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // actions
                case 3:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Actions += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Actions += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(3) * item.Level;
                        item.Stats.Actions += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Actions += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // endurance
                case 4:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Combat += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Value += _diceService.Roll_1dn(7) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Endurance += _diceService.Roll_1dn(10) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // accretion
                case 5:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Resist += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Resist += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(3) * item.Level;
                        item.Stats.Resist += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Resist += _diceService.Roll_1dn(10) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void IncreaseRandomStatForTaintedItem(Item item)
    {
        var rollTimes = _diceService.Roll_1dn(5) * item.Level;

        for (int i = 0; i < rollTimes; i++)
        {
            var statNr = _diceService.Roll_1dn(Statics.Stats.All.Count); // accounts for the nr of stats
            switch (statNr)
            {
                // defense
                case 1:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(2) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(5);
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(2) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(10);
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(5);
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(10) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(15);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // resist
                case 2:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Resist += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Resist += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(3) * item.Level;
                        item.Stats.Resist += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Resist += _diceService.Roll_1dn(10) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // actions
                case 3:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Actions += _diceService.Roll_1dn(3) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(3);
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Actions += _diceService.Roll_1dn(2) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(2);
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(2) * item.Level;
                        item.Stats.Actions += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(3);
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(3) * item.Level;
                        item.Stats.Actions += _diceService.Roll_1dn(10) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(5);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // endurance
                case 4:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Endurance += _diceService.Roll_1dn(10) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(3);
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Endurance += _diceService.Roll_1dn(20) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(2);
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(2) * item.Level;
                        item.Stats.Endurance += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(3);
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Endurance += _diceService.Roll_1dn(25) * item.Level;
                        item.Stats.Resist -= _diceService.Roll_1dn(5);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // accretion
                case 5:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Stats.Accretion += _diceService.Roll_1dn(15) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Stats.Accretion += _diceService.Roll_1dn(40) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(3) * item.Level;
                        item.Stats.Accretion += _diceService.Roll_1dn(20) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Stats.Defense += _diceService.Roll_1dn(5) * item.Level;
                        item.Stats.Accretion += _diceService.Roll_1dn(50) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void IncreaseRandomFeatForTaintedItem(Item item) // keep 8 roll / 8 effect as max
    {
        var rollTimes = _diceService.Roll_1dn(5) * item.Level;

        for (int i = 0; i < rollTimes; i++)
        {
            var featNr = _diceService.Roll_1dn(Statics.Feats.All.Count); // accounts for the nr of feats
            switch (featNr)
            {
                // combat
                case 1:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Combat += _diceService.Roll_1dn(8) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(8) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Combat += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Combat += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Combat += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.CombatEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // strength
                case 2:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Strength += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.StrengthEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Strength += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.StrengthEff += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Strength += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.StrengthEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Strength += _diceService.Roll_1dn(7) * item.Level;
                        item.Feats.StrengthEff += _diceService.Roll_1dn(6) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // tactics
                case 3:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Tactics += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.TacticsEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Tactics += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.TacticsEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Tactics += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.TacticsEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Tactics += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.TacticsEff += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // athletics
                case 4:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Athletics += _diceService.Roll_1dn(7) * item.Level;
                        item.Feats.AthleticsEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Athletics += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.AthleticsEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Athletics += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.AthleticsEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Athletics += _diceService.Roll_1dn(7) * item.Level;
                        item.Feats.AthleticsEff += _diceService.Roll_1dn(6) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // survival
                case 5:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Survival += _diceService.Roll_1dn(7) * item.Level;
                        item.Feats.SurvivalEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Survival += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.SurvivalEff += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Survival += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.SurvivalEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Survival += _diceService.Roll_1dn(7) * item.Level;
                        item.Feats.SurvivalEff += _diceService.Roll_1dn(7) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // social
                case 6:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(8) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(7) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // abstract
                case 7:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Abstract += _diceService.Roll_1dn(8) * item.Level;
                        item.Feats.AbstractEff += _diceService.Roll_1dn(8) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Abstract += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.AbstractEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Abstract += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.AbstractEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Abstract += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.AbstractEff += _diceService.Roll_1dn(8) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // psionic
                case 8:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Social += _diceService.Roll_1dn(2) * item.Level;
                        item.Feats.SocialEff += _diceService.Roll_1dn(2) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // crafting
                case 9:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Crafting += _diceService.Roll_1dn(7) * item.Level;
                        item.Feats.CraftingEff += _diceService.Roll_1dn(6) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Crafting += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.CraftingEff += _diceService.Roll_1dn(3) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Crafting += _diceService.Roll_1dn(3) * item.Level;
                        item.Feats.CraftingEff += _diceService.Roll_1dn(4) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Crafting += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.CraftingEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                // medicine
                case 10:
                    if (item.Type == Statics.Items.Types.Weapon)
                    {
                        item.Feats.Medicine += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.MedicineEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Trinket)
                    {
                        item.Feats.Medicine += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.MedicineEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Shield)
                    {
                        item.Feats.Medicine += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.MedicineEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else if (item.Type == Statics.Items.Types.Armour)
                    {
                        item.Feats.Medicine += _diceService.Roll_1dn(5) * item.Level;
                        item.Feats.MedicineEff += _diceService.Roll_1dn(5) * item.Level;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void IncreaseRandomFeatForNonTaintedItem(Item item) // keep 8 roll / 8 effect as max
    {
        var rollTimes = _diceService.Roll_1dn(5) * item.Level;

        for (int i = 0; i < rollTimes; i++)
        {
            if (item.Type == Statics.Items.Types.Weapon)
            {
                item.Feats.Combat += _diceService.Roll_1dn(2) * item.Level;
                item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
            }
            else if (item.Type == Statics.Items.Types.Trinket)
            {
                item.Value += _diceService.Roll_1dn(10) * item.Level;
            }
            else if (item.Type == Statics.Items.Types.Shield)
            {
                item.Feats.Combat += _diceService.Roll_1dn(2) * item.Level;
                item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
            }
            else if (item.Type == Statics.Items.Types.Armour)
            {
                item.Feats.Combat += _diceService.Roll_1dn(2) * item.Level;
                item.Feats.CombatEff += _diceService.Roll_1dn(2) * item.Level;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }


    #endregion
}