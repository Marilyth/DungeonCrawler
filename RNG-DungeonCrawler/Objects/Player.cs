using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects
{
    class Player
    {
        internal int axisX { get; set; }
        internal int axisY { get; set; }

        internal int hp, dmg, exp, level;

        public Player(int x, int y, int experience)
        {
            axisX = x;
            axisY = y;

            exp = experience;
            calcStats();
        }

        internal delegate double del(int i);
        internal static del levelCalc = x => ((x * x * x) - 6 * (x * x) + 17 * (x) - 12) * (50 / 3.0);

        private void calcStats()
        {
            int i = 0;
            while (exp > levelCalc(i))
            {
                i++;
            }
            level = i - 1;

            hp = level * level + 3;
            dmg = level * 2;
        }
    }
}
