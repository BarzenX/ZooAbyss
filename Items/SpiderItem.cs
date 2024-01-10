using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.NPCs;

namespace ZooAbyss.Items
{
    public class SpiderItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.scale = 1;
            Item.value = Item.buyPrice(silver: 15);

            Item.useStyle = 1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 20;
            Item.height = 20;
            Item.makeNPC = ModContent.NPCType<Spider>(); ;
            Item.noUseGraphic = true;

            Item.bait = 20;

        }
    }
}
