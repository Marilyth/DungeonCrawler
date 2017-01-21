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
            Program p = new Program();
            p.program();
        }

        public void program()
        {
            Console.SetWindowSize(62, 32);

            map = new Objects.Dungeon(20, 20);

            map.drawMap();

            inputCommand();
        }

        public static void inputCommand()
        {
            Console.WriteLine("\nMovement: Left, Right, Up, Down\nCombat: Any Key\nRefresh: R");

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        map.playerMove(0, -1);
                        break;
                    case ConsoleKey.DownArrow:
                        map.playerMove(0, 1);
                        break;
                    case ConsoleKey.RightArrow:
                        map.playerMove(1, 0);
                        break;
                    case ConsoleKey.LeftArrow:
                        map.playerMove(-1, 0);
                        break;
                    case ConsoleKey.R:
                        map = new Objects.Dungeon(20, 20);
                        break;
                    case ConsoleKey.Enter:
                        if (map.playerAction == Objects.Dungeon.Situation.Loot)
                            map.pickUp();
                        break;
                    default:
                        map.playerAttack();
                        break;
                }
                map.drawMap();

            } while (cki.Key != ConsoleKey.Escape);
        }
    }
}
