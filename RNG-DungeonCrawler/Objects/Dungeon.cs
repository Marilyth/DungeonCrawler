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

        public Situation playerAction;

        internal static Player user;
        private Enemy curMob;
        private Treasure curTre;

        internal List<Enemy> allEnemies;
        internal List<Treasure> allTreasures;
        private AllEnemies information;

        internal Field[,] mapset;

        public Dungeon(int lengthX, int lengthY, int difficulty)
        {
            Boolean playerExist = false;
            Boolean bossExist = false;

            information = new AllEnemies();

            user = new Player(0, 0);

            mapset = new Field[lengthX, lengthY];
            playerAction = Situation.Walk;

            allEnemies = new List<Enemy>();
            allTreasures = new List<Treasure>();

            for (int x = 0; x < mapset.GetLength(0); x++)
            {
                for (int y = 0; y < mapset.GetLength(1); y++)
                {
                    Field.Type type = Field.Type.Ground;
                    int decision = ran.Next(0, 400);

                    if (decision < 100)
                    {
                        type = Field.Type.Wall;
                    }
                    else if (decision < 380)
                    {
                        type = Field.Type.Ground;
                    }
                    else if (decision < 390)
                    {
                        type = Field.Type.Enemy;
                        allEnemies.Add(new Enemy(x, y, false, information.getEnemy(difficulty)));
                    }
                    else if (decision < 395)
                    {
                        if (!bossExist)
                        {
                            type = Field.Type.Boss;
                            allEnemies.Add(new Enemy(x, y, true, information.getBoss(difficulty)));
                            bossExist = true;
                        }
                    }
                    else if (decision < 397)
                    {
                        type = Field.Type.Treasure;
                        allTreasures.Add(new Treasure(null, null, ran.Next(1, difficulty*2), x, y));
                    }
                    else
                    {
                        if (!playerExist)
                        {
                            type = Field.Type.Player;
                            user = new Player(x, y);
                            playerExist = true;
                        }
                    }

                    mapset[x, y] = new Field(x, y, type);
                }
            }
            user.curHp = user.hp;
            return;
        }

        internal void playerMove(int x, int y)
        {
            if(playerAction == Situation.Walk)
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

        internal void drawMap(Boolean slowLoad)
        {
            Console.Clear();

            switch (playerAction)
            {
                case Situation.Walk:
                    for (int i = 0; i < mapset.GetLength(1); i++)
                    {
                        for (int j = 0; j < mapset.GetLength(0); j++)
                        {
                            if (slowLoad) System.Threading.Thread.Sleep(3);
                            WriteColored(mapset[j, i].getColor(), $"{mapset[j, i].comfyView()}");
                        }
                        Console.Write("\n");
                    }
                    break;

                case Situation.Fight:
                    WriteColored(curMob.spectrum, curMob.enemyArt + $"\n\n{ curMob.stats()}");
                    break;

                case Situation.Loot:
                    string output = $"{Enemy.art("treasure")}\n";

                    if(curTre.wDrop != null)
                        output += string.Join(", ", curTre.wDrop.getStats()) + "\n";
                    if(curTre.aDrop != null)
                        output += string.Join(", ", curTre.aDrop.getStats()) + "\n";

                    output += $"{curTre.gold} gold\n";

                    WriteColored(ConsoleColor.Yellow, output);
                    break;

                case Situation.Dead:
                    WriteColored(ConsoleColor.Red, "You Died! --- R to refresh\n");
                    break;

                case Situation.Done:
                    WriteColored(ConsoleColor.Green, "You are done! --- R to refresh\n");
                    break;
            }

            Console.WriteLine("------------------------------------------------------------" +
                    $"\nPosition: {user.axisX}X {user.axisY}Y\n");

            Console.WriteLine(user.getStats());
        }

        public void pickUp()
        {
            user.gold += curTre.gold;
            if(curTre.aDrop != null && curTre.aDrop.def > user.armorHold.def)
                user.armorHold = curTre.aDrop;
            if (curTre.wDrop != null && curTre.wDrop.dmg > user.weaponHold.dmg)
                user.weaponHold = curTre.wDrop;

            user.writeStats();
            mapset[curTre.axisX, curTre.axisY].fieldType = Field.Type.Ground;
            allTreasures.Remove(curTre);
            playerAction = Situation.Walk;
        }

        public void healUp()
        {
            if (user.gold >= 10)
            {
                user.gold -= 10;
                user.curHp += 10;
            }
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
                    allTreasures.Add(curMob.uponDeath());

                    allEnemies.Remove(curMob);
                    user.exp += curMob.exp;

                    user.writeStats();

                    curMob = null;

                    if(allEnemies.Count == 0)
                    {
                        playerAction = Situation.Done;
                    }

                    drawMap(false);
                }
                else if(user.curHp <= 0)
                {
                    playerAction = Situation.Dead;
                    user.weaponHold = Weapon.getWeapon("Nothing");
                    user.armorHold = Armor.getArmor("Nothing");
                    user.exp -= user.exp / 10;
                    user.writeStats();

                    drawMap(false);
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
