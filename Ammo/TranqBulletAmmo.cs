using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.Items;
using ZooAbyss.projectiles;

namespace ZooAbyss.Ammo
{
    public class TranqBulletAmmo : ModItem
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
            Item.height = 7; 
            //Combat Stats
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 1f;
            Item.shootSpeed = 10f; // The speed of the projectile.
            //Noncombat Stats
            Item.value = Item.sellPrice(0, 1, 0, 0); 
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<TranqBulletP>();
            Item.ammo = AmmoID.Bullet; // The ammo class this ammo belongs to.
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true; // This marks the item as consumable, making it automatically be consumed when it's used as ammunition, or something else, if possible.
        }
        
        

        public override void AddRecipes()
        {
            //Recipe
            CreateRecipe(1)
                .AddIngredient(ItemID.SoulofLight, 1)
                .AddIngredient(ItemID.SoulofNight, 1)
                .AddIngredient(ModContent.ItemType<BottleOfCobraVenom>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}