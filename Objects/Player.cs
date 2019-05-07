using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace DungeonCrawler.Objects
{
    public class Player : BaseObject
    {
        public int HP, HPMax, MP, MPMax, Exp, Gold;


        public Player(int x, int y, string name = "") : base(x, y, name)
        {
        }

        public static double LevelToExp(int x) => (50 * (x * x));
        public static double ExpToLevel(int y) => (Math.Sqrt(y / 50));

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
                   $"Level: {(int)ExpToLevel(Exp)}  ({CalcNextLevel()})\n" +
                   $"Gold: {Gold}\n" +
                   $"Position: ({XAxis}x, {YAxis}y)";
        }

        public void SaveCharacter()
        {
            //ToDo: Write to Server
        }

        public static Player GetPlayerAsync(string name)
        {
            //ToDo: Request player from Server
            return new Player(0, 0);
        }

        //public override string ToString(){
        //    return "[P]";
        //}

        public override ConsoleColor GetColour()
        {
            return ConsoleColor.Blue;
        }

        public static Player GetPlayer(string Id)
        {
            var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).Split("DungeonCrawler").First();
            var targetDir = Path.Combine(curDir, "DungeonCrawler", "players");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            try
            {
                using (var sr = new System.IO.StreamReader($"{targetDir}\\{Id}.json"))
                {
                    var obj = JsonConvert.DeserializeObject<Player>(sr.ReadToEnd());
                    return obj;
                }
            }
            catch
            {
                return new Player((int)Program.map.Fields.GetLongLength(1) / 2, (int)Program.map.Fields.GetLongLength(0) / 2, Id);
            }
        }

        public void SavePlayer()
        {
            var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).Split("DungeonCrawler").First();
            var targetDir = Path.Combine(curDir, "DungeonCrawler", "players");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            using (var sr = new System.IO.StreamWriter($"{targetDir}\\{Name}.json"))
            {
                sr.WriteLine(JsonConvert.SerializeObject(this));
            }
        }
    }
}
