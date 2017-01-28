using System;
using System.IO;
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

        internal int HP, curHP, dmg, exp, movement;
        internal ConsoleColor spectrum = ConsoleColor.Red;

        internal string enemyArt { get; }

        public Enemy(int x, int y, Boolean boss, string enemy)
        {
            wDropList = new List<Weapon>();
            aDropList = new List<Armor>();

            axisX = x;
            axisY = y;

            switch (enemy)
            {
                case "Rat":
                    HP = 10;
                    dmg = 2;
                    exp = 25;
                    wDropList.Add(Weapon.getWeapon("Teeth", 17));
                    break;
                case "Snake":
                    HP = 20;
                    dmg = 3;
                    exp = 50;
                    wDropList.Add(Weapon.getWeapon("Venom-Dagger", 20));
                    break;
                case "Bat":
                    HP = 10;
                    dmg = 5;
                    exp = 60;
                    wDropList.Add(Weapon.getWeapon("Echo", 5));
                    aDropList.Add(Armor.getArmor("Batwing-Dress", 10));
                    break;
                case "Spider":
                    HP = 100;
                    dmg = 10;
                    exp = 370;
                    wDropList.Add(Weapon.getWeapon("Fangs", 35));
                    aDropList.Add(Armor.getArmor("Spider Shell", 30));
                    break;
                case "Ghost":
                    HP = 30;
                    dmg = 4;
                    exp = 100;
                    wDropList.Add(Weapon.getWeapon("Arcane Dust", 19));
                    break;
                case "Skeleton":
                    HP = 500;
                    dmg = 20;
                    exp = 1800;
                    wDropList.Add(Weapon.getWeapon("Rusty Sword", 40));
                    aDropList.Add(Armor.getArmor("Moldy Shield", 20));
                    break;
                case "Phoenix":
                    HP = 200;
                    dmg = 60;
                    exp = 3500;
                    wDropList.Add(Weapon.getWeapon("Fireball", 19));
                    break;
                case "Vampire":
                    HP = 2000;
                    dmg = 100;
                    exp = 13500;
                    wDropList.Add(Weapon.getWeapon("Bats", 19));
                    aDropList.Add(Armor.getArmor("Vampire Curse", 20));
                    break;
                case "Dragon":
                    HP = 50000;
                    dmg = 200;
                    exp = 180000;
                    wDropList.Add(Weapon.getWeapon("Rusty Sword", 19));
                    aDropList.Add(Armor.getArmor("Moldy Shield", 20));
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
            StreamReader sr = new StreamReader($"data//ascii//{enemyType}.txt");
            string art = sr.ReadToEnd();
            return art;
        }

        public Treasure uponDeath()
        {
            Random ran = new Random();
            Weapon wDrop = null;
            Armor aDrop = null;

            int decider = ran.Next(0, 101);

            foreach (Weapon possibDrop in wDropList)
                if (decider <= possibDrop.dropChance)
                    wDrop = possibDrop;

            foreach (Armor possibDrop in aDropList)
                if (decider <= possibDrop.dropChance)
                    aDrop = possibDrop;

            return new Treasure(wDrop, aDrop, ran.Next(1, dmg), axisX, axisY);
        }
    }
}
