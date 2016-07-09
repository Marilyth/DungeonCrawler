using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual
{
    class Ascii
    {
        public static string art(string enemyType)
        {
            StreamReader sr = new StreamReader($"ascii//{enemyType}.txt");
            string art = sr.ReadToEnd();
            return art;
        }
    }
}
