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
        public static AllEnemies information;
        private ConsoleKeyInfo cki;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.program();
        }

        public void program()
        {
            Console.SetWindowSize(62, 32);

            information = new AllEnemies();

            menuCommand();

            inputCommand();
        }

        public void help()
        {
            Console.Clear();
            Console.WriteLine("[H] to heal while in Dungeon\n"+ 
                              "[Arrow Keys] to move while in Dungeon\n"+
                              "[Enter] to pick up loot\n"+
                              "[R] to return from Dungeon after boss was defeated\n"+
                              "[Any other Key] to attack while in Battle");
            Console.ReadKey();

            menuCommand();
        }

        public void menuCommand()
        {
            Console.Clear();
            Console.WriteLine("[1]: Dungeon level 1 - 10");
                            WriteColored(ConsoleColor.Red, "    Enemies: " + (string.Join(", ", information.getEligable(10))) + "\n");
                            WriteColored(ConsoleColor.Magenta, "    Bosses: " + (string.Join(", ", information.getEligableBoss(10))) + "\n\n");
            Console.WriteLine("[2]: Dungeon Level 10 - 15");
                            WriteColored(ConsoleColor.Red, "    Enemies: " + (string.Join(", ", information.getEligable(15))) + "\n");
                            WriteColored(ConsoleColor.Magenta, "    Bosses: " + (string.Join(", ", information.getEligableBoss(15))) + "\n\n");
            Console.WriteLine("[H]: Controls");
            Console.Write("\nInput: ");

            cki = Console.ReadKey();

            int difficulty = 0;

            switch (cki.Key)
            {
                case ConsoleKey.D1:
                    difficulty = 10;
                    break;
                case ConsoleKey.D2:
                    difficulty = 15;
                    break;
                case ConsoleKey.H:
                    help();
                    return;
            }

            map = new Objects.Dungeon(20, 20, difficulty);

            map.drawMap(true);
        }

        public void inputCommand()
        {
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
                        if((map.allEnemies.Where(x => x.spectrum == ConsoleColor.Magenta).ToList()).Count == 0 
                            || map.playerAction == Objects.Dungeon.Situation.Dead)
                            menuCommand();
                        break;
                    case ConsoleKey.H:
                        map.healUp();
                        break;
                    case ConsoleKey.Enter:
                        if (map.playerAction == Objects.Dungeon.Situation.Loot)
                            map.pickUp();
                        break;
                    default:
                        map.playerAttack();
                        break;
                }
                map.drawMap(false);

            } while (cki.Key != ConsoleKey.Escape);
        }

        Action<ConsoleColor, string> WriteColored = (x, y) => { Console.ForegroundColor = x; Console.Write(y); Console.ForegroundColor = ConsoleColor.Gray; };
    }
}
