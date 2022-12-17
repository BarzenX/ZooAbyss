﻿using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.Items;

namespace ZooAbyss.Armor
{
    // The AutoloadEquip attribute automatically attaches an equip texture to this item.
    // Providing the EquipType.Body value here will result in TML expecting X_Arms.png, X_Body.png and X_FemaleBody.png sprite-sheet files to be placed next to the item's main texture.
    [AutoloadEquip(EquipType.Body)]
    public class LeafletChestplate : ModItem
    {
        public override void SetStaticDefaults()
        {
           
            DisplayName.SetDefault("LeafOverdoseBreastplate");
            Tooltip.SetDefault("A living place for creatures and plants alike.");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Green; // The rarity of the item
            Item.defense = 3; // The amount of defense the item will give when equipped
        }
        
        public override void UpdateEquip(Player player)
        {
            player.statDefense = 3;
            

        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.Wood, 10)
                .AddIngredient(ModContent.ItemType<Leaf>(), 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}