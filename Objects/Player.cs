using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Objects
{
    class Player : Object
    {
        public int HP, HPMax, MP, MPMax, Exp, Gold;


        public Player(int x, int y)
        {
            XAxis = x;
            YAxis = y;
        }

        public static double LevelToExp(int x) => (50 * (x * x));
        public static double ExpToLevel(int y) => (Math.Sqrt(y/50));

        public string CalcNextLevel()
        {
            var curLevel = (int)ExpToLevel(Exp);
            string output = "", TempOutput = "";
            double diffExperience = LevelToExp(curLevel + 1) - Exp;
            for (int i = 0; i < Math.Floor(Exp / (diffExperience / 10)); i++)
            {
                output += "■";
            }
            for (int i = 0; i < 10 - output.Length; i++)
            {
                TempOutput += "□";
            }
            return output + TempOutput;
        }

        public string GetStats()
        {
            return $"HP: {HP}/{HPMax}\n" +
                   $"Level: {(int)ExpToLevel(Exp)}  ({CalcNextLevel()})\n"+
                   $"Gold: {Gold}";
        }

        public void SaveCharacter()
        {
            //ToDo: Write to Server
        }

        public static Player GetPlayerAsync(string name){
            //ToDo: Request player from Server
            return new Player(0, 0);
        }

        //public override string ToString(){
        //    return "[P]";
        //}

        public override ConsoleColor GetColour(){
            return ConsoleColor.Blue;
        }
    }
}
