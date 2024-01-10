using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.Items;
using ZooAbyss.projectiles;

namespace ZooAbyss.Ammo
{
    public class TranqDartAmmo : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Journey Mode sacrifice/research amount.
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 999;
        }

        public override void SetDefaults()
        {
            //Stats:
            //Display Stats
            Item.width = 7; 
            Item.height = 12;
            //Combat Stats
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 1f;
            Item.shootSpeed = 10f; // The speed of the projectile.
            //Noncombat Stats
            Item.value = Item.sellPrice(0, 0, 50, 0); 
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<TranqDartP>();
            Item.ammo = AmmoID.Dart; // The ammo class this ammo belongs to.
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true; // This marks the item as consumable, making it automatically be consumed when it's used as ammunition, or something else, if possible.
        }



        public override void AddRecipes()
        {
            //Recipe
            CreateRecipe(1)
                .AddIngredient(ItemID.Stinger, 1)
                .AddIngredient(ModContent.ItemType<BottleOfSpiderVenom>(), 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }


    }
}