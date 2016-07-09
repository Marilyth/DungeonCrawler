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
            Console.SetWindowSize(62,32);

            map = new Objects.Dungeon(20, 20);

            map.drawMap(); 

            while (true)
            {
                inputCommand();
            }
        }

        public static void inputCommand()
        {
            Console.WriteLine("\nMovement: Left, Right, Up, Down\nCombat: Attack\nSystem: Refresh");

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
                case "attack":
                    map.playerAttack();
                    break;
                case "refresh":
                    map = new Objects.Dungeon(20, 20);
                    break;
            }
            map.drawMap();
        }
    }
}
