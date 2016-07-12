using System;
using RNG_DungeonCrawler.Objects.Individual;
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

        private int situation = 0;

        internal Player user;
        private Enemy curMob;
        private Treasure curTre;

        internal List<Enemy> allEnemies;
        internal List<Treasure> allTreasures;

        internal Field[,] mapset;

        public Dungeon(int lengthX, int lengthY)
        {
            mapset = new Field[lengthX, lengthY];

            allEnemies = new List<Enemy>();
            allTreasures = new List<Treasure>();
            Array possibleStyle = Enum.GetValues(typeof(Field.Type));
            Array possibleMobs = Enum.GetValues(typeof(Enemy.Type));

            for (int i = 0; i < mapset.GetLength(0); i++)
            {
                for (int j = 0; j < mapset.GetLength(1); j++)
                {
                    int style = 0;

                    if (ran.Next(0, mapset.Length / 4) == 0) { style = 2; allTreasures.Add(new Treasure(null, null, 10, i, j)); }
                    else if (ran.Next(0, mapset.Length / 8) == 0) { style = 3; allEnemies.Add(new Enemy(i, j, 1, (Enemy.Type)possibleMobs.GetValue(ran.Next(0, possibleMobs.Length)))); }
                    else if (ran.Next(0, mapset.Length / 4) == 0 && pCount == 0) { style = 5; user = new Player(i, j, 1000); pCount++; }
                    else if (ran.Next(0, mapset.Length / 100) == 0) style = 1;
                    else if (ran.Next(0, mapset.Length / 2) == 0) { style = 4; allEnemies.Add(new Enemy(i, j, 2, (Enemy.Type)possibleMobs.GetValue(ran.Next(0, possibleMobs.Length)))); }

                    mapset[i, j] = new Field(i,j, (Field.Type)possibleStyle.GetValue(style));
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
            Field movedTo = mapset[user.axisX + x, user.axisY + y], currentAt = mapset[user.axisX, user.axisY];

            if (user.axisX + x >= 0 && user.axisX + x < mapset.GetLength(0) && user.axisY + y < mapset.GetLength(1) && user.axisY + y >= 0)
            {
                if(movedTo.fieldType == Field.Type.Ground)
                {
                    currentAt.fieldType = Field.Type.Ground;
                    user.axisX += x;
                    user.axisY += y;
                    movedTo.fieldType = Field.Type.Player;

                    foreach (Enemy mob in allEnemies)
                    {
                        int xAx = ran.Next(-1, 2);
                        int yAx = ran.Next(-1, 2);
                        enemyMove(xAx, yAx, mob);
                    }
                }

                else if(movedTo.fieldType == Field.Type.Enemy || movedTo.fieldType == Field.Type.Boss)
                {
                    situation = 1;
                    curMob = allEnemies.Find(n => n.axisX == user.axisX + x && n.axisY == user.axisY + y);
                }

                else if (movedTo.fieldType == Field.Type.Treasure)
                {
                    situation = 2;
                    curTre = allTreasures.Find(n => n.axisX == user.axisX + x && n.axisY == user.axisY + y);
                }
            }
        }

        private void enemyMove(int x, int y, Enemy mob)
        {
            Field.Type mobType = mapset[mob.axisX, mob.axisY].fieldType;

            if (mob.axisX + x >= 0 && mob.axisX + x < mapset.GetLength(0) && mob.axisY + y < mapset.GetLength(1) && mob.axisY + y >= 0)
            {
                if (mapset[mob.axisX + x, mob.axisY + y].fieldType == Field.Type.Ground)
                {
                    mapset[mob.axisX, mob.axisY].fieldType = Field.Type.Ground;
                    mob.axisX += x;
                    mob.axisY += y;
                    mapset[mob.axisX, mob.axisY].fieldType = mobType;
                }

                else if (mapset[mob.axisX + x, mob.axisY + y].fieldType == Field.Type.Player)
                {
                    curMob = mob;
                    situation = 1;
                }
            }
        }

        internal void drawMap()
        {
            Console.Clear();

            if (situation == 0)
            {
                for (int i = 0; i < mapset.GetLength(1); i++)
                {
                    for (int j = 0; j < mapset.GetLength(0); j++)
                    {
                        WriteColored(mapset[j, i].getColor(), $"{mapset[j, i].comfyView()}");
                    }
                    Console.Write("\n");
                }
            }

            else if(situation == 1)
            {
                WriteColored(curMob.spectrum, curMob.enemyArt + $"\n\n{ curMob.enemyType}: { curMob.stats()}");
            }
            else if (situation == 2)
            {
                string output = $"{Enemy.art("treasure")}\n";
                try
                {
                    output += string.Join(", ", curTre.aDrop.ToList().ConvertAll(x => x.getStats())) + "\n";
                    output += string.Join(", ", curTre.wDrop.ToList().ConvertAll(x => x.getStats())) + "\n";
                }
                catch { }
                output += $"{curTre.gold} gold";

                WriteColored(ConsoleColor.Yellow, output);

                situation = 0;
            }

            Console.WriteLine("\n------------------------------------------------------------" +
                    $"\nPosition: {user.axisX}X {user.axisY}Y\n");

            Console.WriteLine($"{playerSight()}\n\n" + user.getStats());
        }

        public void playerAttack()
        {
            if(situation == 1)
            {
                curMob.curHP -= ran.Next(user.dmg/2, user.dmg);

                if (curMob.curHP <= 0)
                {
                    situation = 0;

                    mapset[curMob.axisX, curMob.axisY].fieldType = Field.Type.Treasure;
                    allTreasures.Add(new Treasure(curMob.wDropList.ToArray(), curMob.aDropList.ToArray(), curMob.HP, curMob.axisX, curMob.axisY));

                    allEnemies.Remove(curMob);

                    curMob = null;

                    drawMap();
                }
            }
        }

        Action<ConsoleColor, string> WriteColored = (x, y) => { Console.ForegroundColor = x; Console.Write(y); Console.ForegroundColor = ConsoleColor.Gray; };
    }
}
