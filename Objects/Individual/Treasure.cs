using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Objects.Individual
{
    class Treasure
    {
        internal int axisX { get; set; }
        internal int axisY { get; set; }

        public int gold;

        public Treasure(int g, int x, int y)
        {
            gold = g;
            axisX = x;
            axisY = y;
        }
    }
}
