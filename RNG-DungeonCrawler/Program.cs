using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RNG_DungeonCrawler
{
    class Program
    {
        public static Objects.Dungeon map;

        static void Main(string[] args)
        {
            map = new Objects.Dungeon(20, 20);

            refresh(); 

            while (true)
            {
                inputCommand();
            }
        }

        public static void refresh()
        {
            Console.Clear();

            for (int i = 0; i < map.mapset.GetLength(1); i++)
            {
                Console.Write("\n");
                for (int j = 0; j < map.mapset.GetLength(0); j++)
                {
                    switch (map.mapset[j, i].fieldType)
                    {
                        case "wall":
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"[X]");
                            break;
                        case "ground":
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write($"[ ]");
                            break;
                        case "treasure":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"[O]");
                            break;
                        case "player":
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write($"[P]");
                            break;
                        case "enemy":
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write($"[E]");
                            break;
                        case "boss":
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write($"[B]");
                            break;
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n------------------------------------------------------------" +
                $"\nPosition: {map.user.axisX}X {map.user.axisY}Y\n");
            Console.WriteLine($"{map.playerSight()}\n\n" + map.user.getStats());
        }

        public static void inputCommand()
        {
            Console.WriteLine("\n\nCommands: Left, Right, Up, Down");

            switch (Console.ReadLine().ToLower())
            {
                case "left":
                    map.playerMove(-1,0);
                    break;
                case "right":
                    map.playerMove(1,0);
                    break;
                case "up":
                    map.playerMove(0,-1);
                    break;
                case "down":
                    map.playerMove(0,1);
                    break;
            }
            refresh();
        }
    }
}
