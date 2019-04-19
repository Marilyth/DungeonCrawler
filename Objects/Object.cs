using System;
using System.Linq;

namespace DungeonCrawler.Objects{
    public class Object{
        public int XAxis;
        public int YAxis;
        public string Name;
        public ObjectVisibility Visibility = ObjectVisibility.Occupying;
        public bool isWalkThrough = false;

        public Object(int XAxis, int YAxis, string Name = ""){
            this.XAxis = XAxis;
            this.YAxis = YAxis;
            this.Name = Name;
        }

        public override string ToString(){
            return $"[{Name.FirstOrDefault()}]";
        }

        public virtual ConsoleColor GetColour(){
            return ConsoleColor.Black;
        }
    }

    public enum ObjectVisibility {Burried, Hidden, Occupying}
}