using System;
using DungeonCrawler.Objects;

namespace DungeonCrawler
{
    class Program
    {
        public static Objects.WorldMap map;
        public static AllEnemies information;
        private static ConsoleKeyInfo cki;
        public static Random ran = new Random();

        static void Main(string[] args)
        {
            Program p = new Program();
            p.program();
        }

        public void program()
        {
            Console.SetWindowSize(181, 30);
            information = new AllEnemies();

            menuCommand();

            inputCommand();
        }

        private static void help()
        {
            Console.Clear();
            Console.WriteLine("[H] to heal while in Dungeon\n"+ 
                              "[Arrow Keys] to move while in Dungeon\n"+
                              "[Enter] to pick up loot\n"+
                              "[Run out of map] to return from Dungeon\n"+
                              "[Any other Key] to attack while in Battle");
            Console.ReadKey();

            menuCommand();
        }

        public static void menuCommand()
        {
            /*cki = Console.ReadKey();

            int difficulty = 0;

            switch (cki.Key)
            {
                case ConsoleKey.D1:
                    difficulty = 10;
                    break;
                case ConsoleKey.D2:
                    difficulty = 20;
                    break;
                case ConsoleKey.D3:
                    difficulty = 30;
                    break;
                case ConsoleKey.D4:
                    difficulty = 40;
                    break;
                case ConsoleKey.H:
                    help();
                    return;
                default:
                    menuCommand();
                    return;
            }*/
            //map = WorldMap.LoadMap();
            map = new Objects.WorldMap(60, 60);

            map.FillMapRandom(60, 60, BiomeType.Desert, 0, 0);
            //map.FillMapRandom(35, 52, BiomeType.Swamp, 0, 25);
            //map.FillMapRandom(60, 10, BiomeType.Cave, 50, 0);

            //map.DrawMap();
        }

        public void inputCommand()
        {
            Console.SetWindowSize(60, 30);
            map.SetPlayer(30, 30, "God");
            map.DrawVisibleMap();
            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        map.PlayerMove(0, -1);
                        break;
                    case ConsoleKey.DownArrow:
                        map.PlayerMove(0, 1);
                        break;
                    case ConsoleKey.RightArrow:
                        map.PlayerMove(1, 0);
                        break;
                    case ConsoleKey.LeftArrow:
                        map.PlayerMove(-1, 0);
                        break;
                    case ConsoleKey.S:
                        map.SaveMap();
                        Console.WriteLine("Saved map successfully!");
                        break;
                    case ConsoleKey.L:
                        map = WorldMap.LoadMap();
                        Console.WriteLine("Loaded map successfully!");
                        break;
                    default:
                        var worked = int.TryParse(""+cki.KeyChar, out int number);
                        if(worked) map.SetField(number);
                        break;

                }
                Console.Clear();
                map.DrawVisibleMap();

            } while (cki.Key != ConsoleKey.Escape);
        }
    }
}
