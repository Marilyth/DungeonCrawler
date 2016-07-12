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

        internal int hp, curHp, dmg, exp, level;

        public Player(int x, int y, int experience)
        {
            axisX = x;
            axisY = y;

            exp = experience;
            calcStats();
        }

        internal static Func<int, double> levelCalc = x => ((x * x * x) - 6 * (x * x) + 17 * (x) - 12) * (50 / 3.0);

        private void calcStats()
        {
            int i = 0;
            while (exp > levelCalc(i))
            {
                i++;
            }
            level = i - 1;

            hp = level * level + 3;
            curHp = hp;
            dmg = level * 2;
        }

        internal string calcNextLevel()
        {
            double expCurrentHold = exp - levelCalc(level);
            string output = "", TempOutput = "";
            double diffExperience = levelCalc(level + 1) - levelCalc(level);
            for (int i = 0; i < (expCurrentHold / (diffExperience / 10)); i++)
            {
                output += "■";
            }
            for (int i = 0; i < 10 - output.Length; i++)
            {
                TempOutput += "□";
            }
            return output + TempOutput;
        }

        public string getStats()
        {
            return $"HP: {curHp}/{hp}"+
                   $" Dmg: {dmg}"+
                   $" Level: {level}  ({calcNextLevel()})";
        }
    }
}
