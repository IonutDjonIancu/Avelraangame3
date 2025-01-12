using Models;
using static Models.Statics;

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
        var type = RandomizeType();
        
        return GenerateSpecificItem(type);
    }

    public Item GenerateSpecificItem(string type)
    {
        if (!Statics.Items.Types.All.Contains(type))
        {
            throw new Exception("Wrong item type provided.");
        }

        var item = type == Statics.Items.Types.Trinket ? new Trinket() : new Item();

        item.Id = Guid.NewGuid();
        item.Type = type;   

        SetLevel(item);
        SetName(item);
        SetIcon(item);
        SetStats(item);
        SetValue(item);

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
    private static void SetValue(Item item)
    {
        int total = 0;
        foreach (var property in item.Bonuses.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(int))
            {
                total += (int)property.GetValue(item.Bonuses)!;
            }
        }

        item.Value = item.HasTaint ? total * 10 : total * 5;
    }

    private static void SetIcon(Item item)
    {
        if (item.Type == Statics.Items.Types.Weapon)
        {
            if (item.Name == Statics.Items.Weapons.Sword) // swords
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_21.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_13.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.LongSword)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_07.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_06.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.ShortSword)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_05.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_04.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.CurvedBlade)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_34.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_36.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Falx)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_22.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_16.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Axe)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_axe_02.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_axe_01.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.DaneAxe)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_axe_05.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_axe_04.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Mace)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_mace_25.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_mace_03.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Warhammer)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_hammer_02.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_hammer_03.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Spear)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_spear_07.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_spear_05.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Polearm)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_axe_15.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_axe_13.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Staff)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_staff_18.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_staff_21.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Dagger)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_31.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_40.jpg";
            }
            else if (item.Name == Statics.Items.Weapons.Seax)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_29.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_41.jpg";
            }
            else
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_sword_21.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_sword_13.jpg";
            }
        } 
        else if (item.Type == Statics.Items.Types.Shield) // shields
        {
            if (item.Name == Statics.Items.Shields.Buckler)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_10.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_shield_09.jpg";
            }
            else if (item.Name == Statics.Items.Shields.RoundShield)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_02.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_shield_20.jpg";
            }
            else if (item.Name == Statics.Items.Shields.KiteShield)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_05.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_shield_04.jpg";
            }
            else if (item.Name == Statics.Items.Shields.NormanShield)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_06.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_shield_03.jpg";
            }
            else if (item.Name == Statics.Items.Shields.Pavise)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_11.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_shield_16.jpg";
            }
            else
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_10.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_shield_09.jpg";
            }
        }
        else if (item.Type == Statics.Items.Types.Armour) // armour
        {
            if (item.Name == Statics.Items.Armours.Linothorax)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_11.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_leather_07.jpg";
            }
            else if (item.Name == Statics.Items.Armours.Chainmail)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_chest_chain_04.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_chain_05.jpg";
            }
            else if (item.Name == Statics.Items.Armours.Scalemail)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_chest_chain_07.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_chain_06.jpg";
            }
            else if (item.Name == Statics.Items.Armours.Halfplate)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate09.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate04.jpg";
            }
            else if (item.Name == Statics.Items.Armours.Fullplate)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate03.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate05.jpg";
            }
            else if (item.Name == Statics.Items.Armours.Brigandine)
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate11.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_plate10.jpg";
            }
            else
            {
                item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_shield_11.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_chest_leather_07.jpg";
            }
        }
        else if (item.Type == Statics.Items.Types.Trinket) // trinkets
        {
            if (((Trinket)item).IsPermanent)
            {
                item.Icon = "https://wow.zamimg.com/images/wow/icons/large/inv_potion_115.jpg";
            }
            else
            {
                if (item.Name == Statics.Items.Trinkets.OrbOfStars)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_04.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_02.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.RingOfTheDjinn)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_misc_ring_firelands_1.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_ring_12.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.AmuletOfBlackraveny)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_06.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_03.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.SashOfDeirmaidun)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_belt_leather_raidmonkprogenitormythic_d_01.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_belt_42b.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.SealOfSojourn)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/spell_holy_sealofwrath.jpg" : "https://wow.zamimg.com/images/wow/icons/large/ability_paladin_empoweredsealsjustice.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.ArmbandOfAsriedor)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_bracers_plate_pvpdeathknight_f_01.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_bracers_pvpwarrior_f_01.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.ScarfOfFearuinar)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_belt_44c.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_belt_44a.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.OakleafOfCedricon)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_misc_herb_mana_thistle_leaf.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_misc_herb_torngreentealeaf.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.RuneOfTheSouthernStar)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_misc_rune_09.jpg" : "https://wow.zamimg.com/images/wow/icons/large/trade_archaeology_dwarf_runestone.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.ShardOfSepteracorium)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_misc_orb_purple.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_misc_orb_yellow.jpg";
                }
                else if (item.Name == Statics.Items.Trinkets.PommelOfTheMarquis)
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/achievement_challengemode_scarletmonastery_gold.jpg" : "https://wow.zamimg.com/images/wow/icons/large/achievement_challengemode_gold.jpg";
                }
                else
                {
                    item.Icon = item.HasTaint ? "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_04.jpg" : "https://wow.zamimg.com/images/wow/icons/large/inv_jewelry_amulet_02.jpg";
                }
            }
        }
        else
        {
            throw new NotImplementedException(); 
        }
    }

    private string RandomizeType()
    {
        var roll = _diceService.Rolld20NoReroll();

        return roll switch
        {
            20 or 19 => Statics.Items.Types.Armour,// 10% chance
            18 or 17 or 16 or 15 => Statics.Items.Types.Trinket,// 20% chance
            14 or 13 or 12 or 11 or 10 => Statics.Items.Types.Shield,// 25% chance
            _ => Statics.Items.Types.Weapon,// 55% chance
        };
    }

    private void SetName(Item item)
    {
        if (item.Type == Statics.Items.Types.Weapon)
        {
            var index = _diceService.Roll1dN(Statics.Items.Weapons.All.Count) - 1;
            item.Name = Statics.Items.Weapons.All[index];
        }
        else if (item.Type == Statics.Items.Types.Shield)
        {
            var index = _diceService.Roll1dN(Statics.Items.Shields.All.Count) - 1;
            item.Name = Statics.Items.Shields.All[index];
        }
        else if (item.Type == Statics.Items.Types.Armour)
        {
            var index = _diceService.Roll1dN(Statics.Items.Armours.All.Count) - 1;
            item.Name = Statics.Items.Armours.All[index];
        }
        else if (item.Type == Statics.Items.Types.Trinket)
        {
            var index = _diceService.Roll1dN(Statics.Items.Trinkets.All.Count) - 1;
            item.Name = Statics.Items.Trinkets.All[index];
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void SetLevel(Item item)
    {
        var roll = _diceService.Rolld20();
        item.Level = 1 + roll / 20;

        if (item.Type == Statics.Items.Types.Trinket)
        {
            ((Trinket)item).IsPermanent = roll >= 60;
        }

        item.HasTaint = item.Level >= 2 && _diceService.Roll1dN(2) % 2 == 0;
    }

    private void SetStats(Item item)
    {
        CraftItem(item);
        
        if (item.HasTaint)
        {
            TaintItem(item);
        }
    }

    private void TaintItem(Item item)
    {
        var rollTimes = _diceService.Roll1dN(5 + item.Level);

        for (int i = 0; i < rollTimes; i++)
        {
            var chosenStat = _diceService.Roll1dN(19);
            var bonus = _diceService.Roll1dN(5) * item.Level;

            switch (chosenStat)
            {
                case 1:
                    item.Bonuses.Strength += bonus;
                    break;
                case 2:
                    item.Bonuses.Constitution += bonus;
                    break;
                case 3:
                    item.Bonuses.Agility += bonus;
                    break;
                case 4:
                    item.Bonuses.Willpower += bonus;
                    break;
                case 5:
                    item.Bonuses.Abstract += bonus;
                    break;
                case 6:
                    item.Bonuses.Melee += bonus;
                    break;
                case 7:
                    item.Bonuses.Arcane += bonus;
                    break;
                case 8:
                    item.Bonuses.Psionics += bonus;
                    break;
                case 9:
                    item.Bonuses.Social += bonus;
                    break;
                case 10:
                    item.Bonuses.Hide += bonus;
                    break;
                case 11:
                    item.Bonuses.Survival += bonus;
                    break;
                case 12:
                    item.Bonuses.Tactics += bonus;
                    break;
                case 13:
                    item.Bonuses.Aid += bonus;
                    break;
                case 14:
                    item.Bonuses.Crafting += bonus;
                    break;
                case 15:
                    item.Bonuses.Perception += bonus;
                    break;
                case 16:
                    item.Bonuses.Defense += bonus;
                    break;
                case 17:
                    item.Bonuses.Actions += bonus;
                    break;
                case 18:
                    item.Bonuses.Endurance += bonus;
                    break;
                case 19:
                    item.Bonuses.Accretion += bonus;
                    break;
                default:
                    break;
            }
        }
    }

    private void CraftItem(Item item)
    {
        if (item.Type == Statics.Items.Types.Weapon)
        {
            item.Bonuses.Melee += _diceService.Roll1dN(5) * item.Level;
            if (item.Name == Statics.Items.Weapons.Sword || item.Name == Statics.Items.Weapons.Falx || item.Name == Statics.Items.Weapons.Spear || item.Name == Statics.Items.Weapons.Warhammer) item.Bonuses.Melee += 1 * item.Level;
            else if (item.Name == Statics.Items.Weapons.CurvedBlade || item.Name == Statics.Items.Weapons.DaneAxe) item.Bonuses.Melee += 2 * item.Level;
            else if (item.Name == Statics.Items.Weapons.LongSword) item.Bonuses.Melee += 3 * item.Level;
        }
        else if (item.Type == Statics.Items.Types.Shield)
        {
            item.Bonuses.Melee += _diceService.Roll1dN(2) * item.Level;

            item.Bonuses.Defense += _diceService.Roll1dN(3) * item.Level;
            if (item.Name == Statics.Items.Shields.RoundShield || item.Name == Statics.Items.Shields.KiteShield) item.Bonuses.Defense += 1 * item.Level;
            else if (item.Name == Statics.Items.Shields.NormanShield) item.Bonuses.Defense += 2 * item.Level;
            else if (item.Name == Statics.Items.Shields.Pavise) item.Bonuses.Defense += 3 * item.Level;

            item.Bonuses.Hide += _diceService.Roll1dN(3) * item.Level;
            if (item.Name == Statics.Items.Shields.NormanShield) item.Bonuses.Hide += 1 * item.Level;
            else if (item.Name == Statics.Items.Shields.Pavise) item.Bonuses.Hide += 2 * item.Level;
        }
        else if (item.Type == Statics.Items.Types.Armour)
        {
            item.Bonuses.Defense += _diceService.Roll1dN(5) * item.Level;
            if (item.Name == Statics.Items.Armours.Scalemail) item.Bonuses.Defense += 1 * item.Level;
            else if (item.Name == Statics.Items.Armours.Chainmail) item.Bonuses.Defense += 2 * item.Level;
            else if (item.Name == Statics.Items.Armours.Halfplate) item.Bonuses.Defense += 3 * item.Level;
            else if (item.Name == Statics.Items.Armours.Brigandine) item.Bonuses.Defense += 4 * item.Level;
            else if (item.Name == Statics.Items.Armours.Fullplate) item.Bonuses.Defense += 5 * item.Level;

            item.Bonuses.Survival += _diceService.Roll1dN(5) * item.Level;
            if (item.Name == Statics.Items.Armours.Chainmail) item.Bonuses.Survival += 1 * item.Level;
            else if (item.Name == Statics.Items.Armours.Brigandine) item.Bonuses.Survival += 2 * item.Level;
            else if (item.Name == Statics.Items.Armours.Fullplate) item.Bonuses.Survival += 3 * item.Level;
        }
        else // trinkets
        {
            item.Bonuses.Social += _diceService.Roll1dN(5) * item.Level;
            if (item.Name == Statics.Items.Trinkets.OrbOfStars) item.Bonuses.Accretion += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.AmuletOfBlackraveny) item.Bonuses.Survival += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.RingOfTheDjinn) item.Bonuses.Psionics += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.SashOfDeirmaidun) item.Bonuses.Hide += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.SealOfLotharius) item.Bonuses.Tactics += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.SealOfSojourn) item.Bonuses.Endurance += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.ArmbandOfAsriedor) item.Bonuses.Melee += _diceService.Roll1dN(2) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.ScarfOfFearuinar) item.Bonuses.Arcane += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.OakleafOfCedricon) item.Bonuses.Aid += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.RuneOfTheSouthernStar) item.Bonuses.Crafting += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.ShardOfSepteracorium) item.Bonuses.Perception += _diceService.Roll1dN(5) * item.Level;
            else if (item.Name == Statics.Items.Trinkets.PommelOfTheMarquis) item.Bonuses.Social += _diceService.Roll1dN(5) * item.Level;
        }
    }
    #endregion
}