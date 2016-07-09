using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual
{
    class Enemy
    {
        internal int axisX { get; set; }
        internal int axisY { get; set; }

        internal List<Weapon> wDropList;
        internal List<Armor> aDropList;

        internal enum Type {Rat, Snake, Bat, Ghost }

        internal int HP, curHP, dmg, exp;
        internal ConsoleColor spectrum = ConsoleColor.Red;

        internal Type enemyType { get; }
        internal string enemyArt { get; }

        public Enemy(int x, int y, int difficulty, Type enemy)
        {
            wDropList = new List<Weapon>();
            aDropList = new List<Armor>();

            axisX = x;
            axisY = y;

            switch (enemy)
            {
                case Type.Rat:
                    HP = 10 * difficulty;
                    dmg = 2 * difficulty;
                    exp = 10 * difficulty;
                    wDropList.Add(new Weapon("Teeth", 5, 10));
                    break;
                case Type.Snake:
                    HP = 20 * difficulty;
                    dmg = 3 * difficulty;
                    exp = 20 * difficulty;
                    wDropList.Add(new Weapon("Venom-Dagger", 5, 10));
                    break;
                case Type.Bat:
                    HP = 10 * difficulty;
                    dmg = 5 * difficulty;
                    exp = 20 * difficulty;
                    wDropList.Add(new Weapon("Echo", 5, 10));
                    aDropList.Add(new Armor("Batwing-Dress", 1, 3));
                    break;
                case Type.Ghost:
                    HP = 30 * difficulty;
                    dmg = 4 * difficulty;
                    exp = 40 * difficulty;
                    wDropList.Add(new Weapon("Arcane Dust", 5, 10));
                    break;
            }
            curHP = HP;
            if (difficulty > 1) spectrum = ConsoleColor.Magenta;
            enemyArt = Ascii.art(enemy.ToString());
            enemyType = enemy;
        }

        public string stats()
        {
            return $"HP: {curHP}/{HP}" +
                   $" Dmg: {dmg}";
        }
    }
}
