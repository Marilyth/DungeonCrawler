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

        public Situation playerAction;

        internal Player user;
        private Enemy curMob;
        private Treasure curTre;

        internal List<Enemy> allEnemies;
        internal List<Treasure> allTreasures;

        internal Field[,] mapset;

        public Dungeon(int lengthX, int lengthY)
        {
            mapset = new Field[lengthX, lengthY];
            playerAction = Situation.Walk;

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
                    else if (ran.Next(0, mapset.Length / 4) == 0 && pCount == 0) { style = 5; user = new Player(i, j); pCount++; }
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
            try
            {
                Field movedTo = mapset[user.axisX + x, user.axisY + y], currentAt = mapset[user.axisX, user.axisY];

                if (user.axisX + x >= 0 && user.axisX + x < mapset.GetLength(0) && user.axisY + y < mapset.GetLength(1) && user.axisY + y >= 0)
                {
                    if (movedTo.fieldType == Field.Type.Ground)
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

                    else if (movedTo.fieldType == Field.Type.Enemy || movedTo.fieldType == Field.Type.Boss)
                    {
                        playerAction = Situation.Fight;
                        curMob = allEnemies.Find(n => n.axisX == user.axisX + x && n.axisY == user.axisY + y);
                    }

                    else if (movedTo.fieldType == Field.Type.Treasure)
                    {
                        playerAction = Situation.Loot;
                        curTre = allTreasures.Find(n => n.axisX == user.axisX + x && n.axisY == user.axisY + y);
                    }
                }
            }
            catch (Exception) { }
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
                    playerAction = Situation.Fight;
                }
            }
        }

        internal void drawMap()
        {
            Console.Clear();

            switch (playerAction)
            {
                case Situation.Walk:
                    for (int i = 0; i < mapset.GetLength(1); i++)
                    {
                        for (int j = 0; j < mapset.GetLength(0); j++)
                        {
                            WriteColored(mapset[j, i].getColor(), $"{mapset[j, i].comfyView()}");
                        }
                        Console.Write("\n");
                    }
                    break;

                case Situation.Fight:
                    WriteColored(curMob.spectrum, curMob.enemyArt + $"\n\n{ curMob.enemyType}: { curMob.stats()}");
                    break;

                case Situation.Loot:
                    string output = $"{Enemy.art("treasure")}\n";
                    try
                    {
                        output += string.Join(", ", curTre.aDrop.ToList().ConvertAll(x => x.getStats())) + "\n";
                        output += string.Join(", ", curTre.wDrop.ToList().ConvertAll(x => x.getStats())) + "\n";
                    }
                    catch { }
                    output += $"{curTre.gold} gold\n";

                    WriteColored(ConsoleColor.Yellow, output);
                    break;

                case Situation.Dead:
                    break;

                case Situation.Done:
                    WriteColored(ConsoleColor.Green, "You are done! --- R to refresh\n");
                    break;
            }

            Console.WriteLine("------------------------------------------------------------" +
                    $"\nPosition: {user.axisX}X {user.axisY}Y\n");

            Console.WriteLine($"{playerSight()}\n\n" + user.getStats());
        }

        public void pickUp()
        {
            user.gold += curTre.gold;
            user.writeStats();
            mapset[curTre.axisX, curTre.axisY].fieldType = Field.Type.Ground;
            allTreasures.Remove(curTre);
            playerAction = Situation.Walk;
        }

        public void playerAttack()
        {
            if(playerAction == Situation.Fight)
            {
                curMob.curHP -= ran.Next(user.dmg/2, user.dmg);
                user.curHp -= ran.Next(curMob.dmg / 2, curMob.dmg);

                if (curMob.curHP <= 0)
                {
                    playerAction = Situation.Walk;

                    mapset[curMob.axisX, curMob.axisY].fieldType = Field.Type.Treasure;
                    allTreasures.Add(new Treasure(curMob.wDropList.ToArray(), curMob.aDropList.ToArray(), curMob.HP, curMob.axisX, curMob.axisY));

                    allEnemies.Remove(curMob);
                    user.exp += curMob.exp;

                    user.writeStats();

                    curMob = null;

                    if(allEnemies.Count == 0)
                    {
                        playerAction = Situation.Done;
                    }

                    drawMap();
                }
            }
        }

        public enum Situation
        {
            Fight,
            Loot,
            Walk,
            Dead,
            Done
        }

        Action<ConsoleColor, string> WriteColored = (x, y) => { Console.ForegroundColor = x; Console.Write(y); Console.ForegroundColor = ConsoleColor.Gray; };
    }
}
