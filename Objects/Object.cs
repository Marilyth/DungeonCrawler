using System;

namespace DungeonCrawler.Objects{
    public class Object{
        public int XAxis;
        public int YAxis;

        public override string ToString(){
            return "[O]";
        }

        public ConsoleColor GetColour(){
            return ConsoleColor.Gray;
        }
    }
}