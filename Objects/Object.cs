using System;
using System.Linq;

namespace DungeonCrawler.Objects{
    public class Object{
        public int XAxis;
        public int YAxis;
        public string Name;

        public override string ToString(){
            return $"[{Name.FirstOrDefault()}]";
        }

        public virtual ConsoleColor GetColour(){
            return ConsoleColor.Black;
        }
    }
}