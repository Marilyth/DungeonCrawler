using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual
{
    class Weapon
    {
        internal int dropChance, dmg;
        internal string name;

        public Weapon(string wName, int wDmg, int wChance)
        {
            name = wName;
            dmg = wDmg;
            dropChance = wChance;
        }

        public string getStats()
        {
            return $"{name}: {dmg}atk";
        }

        public static Weapon getWeapon(string name)
        {
            Weapon output = null;

            StreamReader sr = new StreamReader("data//itemBase.txt");
            string s = null;
            while ((s = sr.ReadLine()) != null)
            {
                string[] information = s.Split(':');

                if (information[0].Equals(name))
                    output = new Weapon(information[0], int.Parse(information[1]), 0);
            }
            sr.Close();

            return output;
        }

        public static Weapon getWeapon(string name, int drop)
        {
            Weapon output = null;

            StreamReader sr = new StreamReader("data//itemBase.txt");
            string s = null;
            while ((s = sr.ReadLine()) != null)
            {
                string[] information = s.Split(':');

                if (information[0].Equals(name))
                    output = new Weapon(information[0], int.Parse(information[1]), drop);
            }
            sr.Close();

            return output;
        }
    }
}
