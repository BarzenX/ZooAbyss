using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.Bosses.BossMinionsAndAtk;

namespace ZooAbyss.Items
{
    public class Leaf : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 10;
        }
        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.White;
        }
        public override void AddRecipes()
        {
            CreateRecipe(3)
                .AddIngredient(ItemID.Wood, 5)
                .AddTile(TileID.LivingLoom)
                .Register();
        }
    }
}

