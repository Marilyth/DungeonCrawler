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

        public Weapon[] wDrop;
        public Armor[] aDrop;
        public int gold;

        public Treasure(Weapon[] w, Armor[] a, int g, int x, int y)
        {
            wDrop = w;
            aDrop = a;
            gold = g;
            axisX = x;
            axisY = y;
        }
    }
}
