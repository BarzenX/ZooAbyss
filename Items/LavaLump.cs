using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.NPCs;

namespace ZooAbyss.Items
{
    public class LavaLump: ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
            Item.scale = 1;
            Item.value = Item.buyPrice(silver: 50);

            //Item.useStyle = 1; // not usable
            //Item.autoReuse = true;
            //Item.useTurn = true;
            //Item.useAnimation = 15;
            //Item.useTime = 10;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 8;
            Item.height = 11;
            //Item.makeNPC = ModContent.NPCType<LavaLump>(); ;
            //Item.noUseGraphic = true;

            Item.bait = 20;

        }
    }
}
