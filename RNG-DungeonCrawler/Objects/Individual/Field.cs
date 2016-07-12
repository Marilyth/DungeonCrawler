﻿using System;
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
        internal Type fieldType { get; set; }

        internal enum Type {Ground, Wall, Treasure, Enemy, Boss, Player};

        public Field(int x, int y, Type type)
        {
            axisX = x;
            axisY = y;
            fieldType = type;
        }

        internal string comfyView()
        {
            switch (fieldType)
            {
                case Type.Wall:
                    return "[X]";
                case Type.Ground:
                    return "[ ]";
                case Type.Treasure:
                    return "[T]";
                case Type.Enemy:
                    return "[E]";
                case Type.Boss:
                    return "[B]";
                case Type.Player:
                    return "[P]";
                default:
                    return "";
            }
        }

        internal ConsoleColor getColor()
        {
            switch (fieldType)
            {
                case Type.Ground:
                    return ConsoleColor.Black;
                case Type.Wall:
                    return ConsoleColor.Gray;
                case Type.Treasure:
                    return ConsoleColor.Yellow;
                case Type.Enemy:
                    return ConsoleColor.Red;
                case Type.Boss:
                    return ConsoleColor.Magenta;
                case Type.Player:
                    return ConsoleColor.Blue;
            }
            return ConsoleColor.Gray;
        }
    }
}
