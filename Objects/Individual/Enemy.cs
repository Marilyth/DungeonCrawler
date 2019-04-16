using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Objects.Individual
{
    class Enemy
    {
        internal int axisX { get; set; }
        internal int axisY { get; set; }

        internal int HP, curHP, dmg, exp, movement;
        internal ConsoleColor spectrum = ConsoleColor.Red;

        internal string enemyArt { get; }

        public Enemy(int x, int y, Boolean boss, string enemy)
        {
            axisX = x;
            axisY = y;

            switch (enemy)
            {
                case "Rat":
                    HP = 10;
                    dmg = 2;
                    exp = 25;
                    break;
                case "Snake":
                    HP = 20;
                    dmg = 3;
                    exp = 50;
                    break;
                case "Bat":
                    HP = 10;
                    dmg = 5;
                    exp = 60;
                    break;
                case "Spider":
                    HP = 100;
                    dmg = 10;
                    exp = 370;
                    break;
                case "Ghost":
                    HP = 30;
                    dmg = 4;
                    exp = 100;
                    break;
                case "Skeleton":
                    HP = 500;
                    dmg = 20;
                    exp = 1800;
                    break;
                case "Phoenix":
                    HP = 200;
                    dmg = 60;
                    exp = 3500;
                    break;
                case "Vampire":
                    HP = 2000;
                    dmg = 100;
                    exp = 13500;
                    break;
                case "Dragon":
                    HP = 50000;
                    dmg = 200;
                    exp = 180000;
                    break;
            }
            curHP = HP;
            if (boss) spectrum = ConsoleColor.DarkMagenta;
            enemyArt = art(enemy);
        }

        public string stats()
        {
            return $"HP: {curHP}/{HP}" +
                   $" Dmg: {dmg}";
        }

        public static string art(string enemyType)
        {
            try{
                StreamReader sr = new StreamReader($"data//ascii//{enemyType}.txt");
                string art = sr.ReadToEnd();
                return art;
            } catch{
                return "";
            }
        }

        public Treasure uponDeath()
        {
            Random ran = new Random();

            int decider = ran.Next(0, 101);

            return new Treasure(ran.Next(1, dmg), axisX, axisY);
        }
    }
}
