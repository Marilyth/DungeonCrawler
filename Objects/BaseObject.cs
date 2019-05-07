using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace DungeonCrawler.Objects
{
    public class BaseObject
    {
        public int XAxis;
        public int YAxis;
        public int HP, CurHP;
        public string Name;
        public BaseObject Below = null;
        public BaseObject Above = null;
        public bool IsWalkThrough = false;
        public bool IsSeeThrough = false;
        public bool IsPushable = false;
        public bool IsDestroyable = false;
        public long Durability = -1;
        public ConsoleColor BackgroundColour = ConsoleColor.White;
        public ConsoleColor ForegroundColour = ConsoleColor.Black;

        public BaseObject(int XAxis, int YAxis, string Name = "", long durability = -1)
        {
            this.XAxis = XAxis;
            this.YAxis = YAxis;
            this.Name = Name;
            Durability = durability;
        }

        public override string ToString()
        {
            return $"[{Name.FirstOrDefault()}]";
        }

        public virtual ConsoleColor GetColour()
        {
            return ConsoleColor.Black;
        }

        public virtual string OnLook(Player player)
        {
            return $"You see a {Name}";
        }

        public static BaseObject GetObject(string Id)
        {
            var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).Split("DungeonCrawler").First();
            var targetDir = Path.Combine(curDir, "DungeonCrawler", "worldobjects");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            using (var sr = new System.IO.StreamReader($"{targetDir}\\{Id}.json"))
            {
                var obj = JsonConvert.DeserializeObject<BaseObject>(sr.ReadToEnd());
                return obj;
            }
        }

        public void SaveObject(){
            var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).Split("DungeonCrawler").First();
            var targetDir = Path.Combine(curDir, "DungeonCrawler", "worldobjects");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            using (var sr = new System.IO.StreamWriter($"{targetDir}\\{Name}.json"))
            {
                sr.WriteLine(JsonConvert.SerializeObject(this));
            }
        }
    }
}