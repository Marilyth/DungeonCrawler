using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual
{
    class Treasure
    {
        internal int axisX { get; set; }
        internal int axisY { get; set; }

        public Weapon wDrop; 
        public Armor aDrop;
        public int gold;

        public Treasure(Weapon pWeapon, Armor pArmor, int g, int x, int y)
        {
            wDrop = pWeapon;
            aDrop = pArmor;
            gold = g;
            axisX = x;
            axisY = y;
        }
    }
}
