using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual
{
    class Armor
    {
        internal int dropChance, def;
        internal string name;

        public Armor(string aName, int aDef, int aChance)
        {
            name = aName;
            def = aDef;
            dropChance = aChance;
        }

        public string getStats()
        {
            return $"{name}: {def}def";
        }

        public static Armor getArmor(string name)
        {
            Armor output = null;

            StreamReader sr = new StreamReader("data//itemBase.txt");
            string s = null;
            while ((s = sr.ReadLine()) != null)
            {
                string[] information = s.Split(':');

                if (information[0].Equals(name))
                    output = new Armor(information[0], int.Parse(information[1]), 0);
            }
            sr.Close();

            return output;
        }

        public static Armor getArmor(string name, int drop)
        {
            Armor output = null;

            StreamReader sr = new StreamReader("data//itemBase.txt");
            string s = null;
            while ((s = sr.ReadLine()) != null)
            {
                string[] information = s.Split(':');

                if (information[0].Equals(name))
                    output = new Armor(information[0], int.Parse(information[1]), drop);
            }
            sr.Close();

            return output;
        }
    }
}
