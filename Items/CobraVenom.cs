using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ZooAbyss.Items
{
    public class CobraVenom : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 8;
            Item.height = 11;
            Item.rare = ItemRarityID.Green;

            Item.maxStack = 10;
            Item.value = Item.buyPrice(silver: 50);

        }
    }
}
