﻿using System;
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

        internal List<Enemy> allEnemies;

        internal Field[,] mapset;

        public Dungeon(int lengthX, int lengthY)
        {
            mapset = new Field[lengthX, lengthY];

            allEnemies = new List<Enemy>();
            Array possibleStyle = Enum.GetValues(typeof(Field.Type));
            Array possibleMobs = Enum.GetValues(typeof(Enemy.Type));

            for (int i = 0; i < mapset.GetLength(0); i++)
            {
                for (int j = 0; j < mapset.GetLength(1); j++)
                {
                    int style = 0;

                    if (ran.Next(0, mapset.Length/4) == 0) style = 2;
                    else if (ran.Next(0, mapset.Length/8) == 0) { style = 3; allEnemies.Add(new Enemy(i, j, 1, (Enemy.Type)possibleMobs.GetValue(ran.Next(0, possibleMobs.Length)))); }
                    else if (ran.Next(0, mapset.Length / 4) == 0 && pCount==0) { style = 5; user = new Player(i, j, 1000); pCount++; }
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
            if(user.axisX + x >= 0 && user.axisX + x < mapset.GetLength(0) && user.axisY + y < mapset.GetLength(1) && user.axisY + y >= 0)
            {
                if(mapset[user.axisX +x, user.axisY +y].fieldType == Field.Type.Ground)
                {
                    mapset[user.axisX, user.axisY].fieldType = Field.Type.Ground;
                    user.axisX += x;
                    user.axisY += y;
                    mapset[user.axisX, user.axisY].fieldType = Field.Type.Player;

                    foreach (Enemy mob in allEnemies)
                    {
                        int xAx = ran.Next(-1, 2);
                        int yAx = ran.Next(-1, 2);
                        enemyMove(xAx, yAx, mob);
                    }
                }

                else if(mapset[user.axisX + x, user.axisY + y].fieldType == Field.Type.Enemy || mapset[user.axisX + x, user.axisY + y].fieldType == Field.Type.Boss)
                {
                    situation = 1;
                    curMob = allEnemies.Find(n => n.axisX == user.axisX + x && n.axisY == user.axisY + y);
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
                    Console.Write("\n");
                    for (int j = 0; j < mapset.GetLength(0); j++)
                    {
                        switch (mapset[j, i].fieldType)
                        {
                            case Field.Type.Wall:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            case Field.Type.Ground:
                                Console.ForegroundColor = ConsoleColor.Black;
                                break;
                            case Field.Type.Treasure:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            case Field.Type.Player:
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case Field.Type.Enemy:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case Field.Type.Boss:
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                break;
                        }
                        Console.Write($"{mapset[j, i].comfyView()}");
                    }
                }
            }

            else if(situation == 1)
            {
                Console.ForegroundColor = curMob.spectrum;
                Console.WriteLine(curMob.enemyArt);
                Console.WriteLine($"\n{curMob.enemyType}: {curMob.stats()}");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
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
                    allEnemies.Remove(curMob);

                    curMob = null;

                    drawMap();
                }
            }
        }
    }
}
