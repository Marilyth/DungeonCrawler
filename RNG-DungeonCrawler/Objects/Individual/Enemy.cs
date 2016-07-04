using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual
{
    class Enemy
    {
        internal int axisX { get; }
        internal int axisY { get; }

        internal List<Specific.Weapon> wDropList;
        internal List<Specific.Armor> aDropList;

        internal int HP, dmg, exp;

        internal string enemyType { get; }

        public Enemy(int x, int y, int difficulty, string enemy)
        {
            wDropList = new List<Specific.Weapon>();
            aDropList = new List<Specific.Armor>();

            axisX = x;
            axisY = y;
            enemyType = enemy;

            switch (enemy)
            {
                case "Rat":
                    HP = 10 * difficulty;
                    dmg = 2 * difficulty;
                    exp = 10 * difficulty;
                    break;
                case "Snake":
                    HP = 20 * difficulty;
                    dmg = 3 * difficulty;
                    exp = 20 * difficulty;
                    break;
                case "Bat":
                    HP = 10 * difficulty;
                    dmg = 5 * difficulty;
                    exp = 20 * difficulty;
                    wDropList.Add(new Specific.Weapon("Echo", 5, 10));
                    break;
                case "":
                    HP = 10 * difficulty;
                    dmg = 2 * difficulty;
                    exp = 10 * difficulty;
                    break;
            }
            enemyType = enemy;
        }
    }
}
