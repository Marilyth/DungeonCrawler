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
        public static Objects.Dungeon map = new Objects.Dungeon();

        static void Main(string[] args)
        {
            map = new Objects.Dungeon();

            refresh(); 

            while (true)
            {
                inputCommand();
            }
        }

        public static void refresh()
        {
            Console.Clear();

            string mapVisual = "";
            for (int i = 0; i < 20; i++)
            {
                mapVisual += "\n";
                for (int j = 0; j < 20; j++)
                {
                    switch (map.mapset[j, i].fieldType)
                    {
                        case "wall":
                            mapVisual += $"[X]";
                            break;
                        case "ground":
                            mapVisual += $"[ ]";
                            break;
                        case "treasure":
                            mapVisual += $"[O]";
                            break;
                        case "player":
                            mapVisual += $"[P]";
                            break;
                        case "enemy":
                            mapVisual += $"[E]";
                            break;
                    }
                }
            }

            Console.WriteLine($"{mapVisual} \n\n ");
            Console.WriteLine($"Position: {map.user.axisX}X {map.user.axisY}Y\n");
            Console.WriteLine($"{map.playerSight()}");
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
