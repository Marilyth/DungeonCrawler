using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNG_DungeonCrawler.Objects
{
    class Dungeon
    {
        private Random ran = new Random();
        int pCount;

        internal Player user;
        internal List<Individual.Enemy> allEnemies;

        internal Individual.Field[,] mapset;

        public Dungeon(int lengthX, int lengthY)
        {
            mapset = new Individual.Field[lengthX, lengthY];

            allEnemies = new List<Individual.Enemy>();
            string[] possibleStyle = new string[] { "ground", "wall", "treasure", "player", "enemy", "boss" };

            for (int i = 0; i < mapset.GetLength(0); i++)
            {
                for (int j = 0; j < mapset.GetLength(1); j++)
                {
                    int style = 0;

                    if (ran.Next(0, mapset.Length/4) == 0) style = 2;
                    else if (ran.Next(0, mapset.Length/8) == 0) { style = 4; allEnemies.Add(new Individual.Enemy(i, j, 1, "Rat")); }
                    else if (ran.Next(0, mapset.Length / 4) == 0 && pCount==0) { style = 3; user = new Player(i, j, 1000); pCount++; }
                    else if (ran.Next(0, mapset.Length / 100) == 0) style = 1;
                    else if (ran.Next(0, mapset.Length / 2) == 0) style = 5;

                    mapset[i, j] = new Individual.Field(i,j, possibleStyle[style]);
                }
            }
            return;
        }

        internal string playerSight()
        {

            string output = "";

            try { output += $"   {(mapset[user.axisX, user.axisY - 1].comfyView())}\n"; } catch (Exception) { output += "   [X]\n"; }
            try { output += $"{(mapset[user.axisX - 1, user.axisY].comfyView())}"; } catch (Exception) { output += "[X]"; }
            try { output += $"   {(mapset[user.axisX + 1, user.axisY].comfyView())}\n"; } catch (Exception) { output += "   [X]\n"; }
            try { output += $"   {(mapset[user.axisX, user.axisY + 1].comfyView())}"; } catch (Exception) { output += "   [X]\n"; }

            return output;
        }

        internal void playerMove(int x, int y)
        {
            if(user.axisX + x >= 0 && user.axisX + x < mapset.GetLength(0) && user.axisY + y < mapset.GetLength(1) && user.axisY + y >= 0 && mapset[user.axisX+x,user.axisY+y].trespass())
            {
                mapset[user.axisX, user.axisY].fieldType = "ground";
                user.axisX += x;
                user.axisY += y;
                mapset[user.axisX, user.axisY].fieldType = "player";

                foreach(Individual.Enemy mob in allEnemies)
                {
                    int xAx = ran.Next(-1,2);
                    int yAx = ran.Next(-1,2);
                    enemyMove(xAx, yAx, mob);
                }
            }
        }

        private void enemyMove(int x, int y, Individual.Enemy mob)
        {
            if (mob.axisX + x >= 0 && mob.axisX + x < mapset.GetLength(0) && mob.axisY + y < mapset.GetLength(1) && mob.axisY + y >= 0 && mapset[mob.axisX + x, mob.axisY + y].trespass())
            {
                mapset[mob.axisX, mob.axisY].fieldType = "ground";
                mob.axisX += x;
                mob.axisY += y;
                mapset[mob.axisX, mob.axisY].fieldType = "enemy";
            }
        }
    }
}
