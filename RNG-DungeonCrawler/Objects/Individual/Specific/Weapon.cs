using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual.Specific
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
    }
}
