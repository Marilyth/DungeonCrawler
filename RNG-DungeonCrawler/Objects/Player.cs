using System;
using System.IO;
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

        internal int hp, curHp, dmg, exp, level, gold;
        public Individual.Weapon weaponHold;
        public Individual.Armor armorHold;


        public Player(int x, int y)
        {
            axisX = x;
            axisY = y;

            StreamReader sr = new StreamReader($"data//playerStats.txt");
            string[] data = sr.ReadLine().Split(':');
            exp = int.Parse(data[0]);
            weaponHold = Individual.Weapon.getWeapon(data[1]);
            armorHold = Individual.Armor.getArmor(data[2]);
            gold = int.Parse(data[3]);

            sr.Close();

            calcStats();
        }

        internal delegate double del(int i);
        internal static del levelCalc = x => (50 * (x * x));

        private void calcStats()
        {
            int i = 0;
            while (exp > levelCalc(i))
            {
                i++;
            }
            level = i - 1;

            hp = (level + 1) * 3 + 10 + armorHold.def;
            dmg = level + 3 + weaponHold.dmg;
        }

        internal string calcNextLevel()
        {
            double expCurrentHold = exp - levelCalc(level);
            string output = "", TempOutput = "";
            double diffExperience = levelCalc(level + 1) - levelCalc(level);
            for (int i = 0; i < Math.Floor(expCurrentHold / (diffExperience / 10)); i++)
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
            return $"HP: {curHp}/{hp} ({armorHold.name} -> +{armorHold.def})\n" +
                   $"Dmg: {dmg} ({weaponHold.name} -> +{weaponHold.dmg})\n"+
                   $"Level: {level}  ({calcNextLevel()})\n"+
                   $"Gold: {gold}";
        }

        public void writeStats()
        {
            StreamWriter sw = new StreamWriter("data//playerStats.txt");
            sw.WriteLine($"{exp}:{weaponHold.name}:{armorHold.name}:{gold}");
            sw.Close();

            calcStats();
        }
    }
}
