using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects.Individual
{
    class Field
    {
        internal int axisX { get; }
        internal int axisY { get; }
        internal string fieldType { get; set; }

        public Field(int x, int y, string type)
        {
            axisX = x;
            axisY = y;
            fieldType = type;
        }

        internal bool trespass()
        {
            return (fieldType=="ground"? true: false);
        }

        internal string comfyView()
        {
            switch (fieldType)
            {
                case "wall":
                    return "[X]";
                case "ground":
                    return "[ ]";
                case "treasure":
                    return "[O]";
                case "enemy":
                    return "[E]";
                case "boss":
                    return "[B]";
                default:
                    return "";
            }
        }
    }
}
