using System;
using DungeonCrawler.Objects.Individual;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Objects
{
    class Dungeon
    {
        private Random ran = new Random();

        public Situation playerAction;

        public static Player user;
        private Enemy curMob;
        private Treasure curTre;

        public List<Enemy> AllEnemies;
        public List<Treasure> AllTreasures;
        private AllEnemies Information;

        public Field[,] Worldmap;

        public Dungeon(int lengthX, int lengthY, int difficulty)
        {
            Boolean playerExist = false;
            Boolean bossExist = false;

            Information = new AllEnemies();

            user = new Player(0, 0);

            Worldmap = new Field[lengthX, lengthY];
            playerAction = Situation.Walk;

            AllEnemies = new List<Enemy>();
            AllTreasures = new List<Treasure>();

            for (int x = 0; x < Worldmap.GetLength(0); x++)
            {
                for (int y = 0; y < Worldmap.GetLength(1); y++)
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
                        AllEnemies.Add(new Enemy(x, y, false, Information.getEnemy(difficulty)));
                    }
                    else if (decision < 395)
                    {
                        if (!bossExist)
                        {
                            type = Field.Type.Boss;
                            AllEnemies.Add(new Enemy(x, y, true, Information.getBoss(difficulty)));
                            bossExist = true;
                        }
                    }
                    else if (decision < 397)
                    {
                        type = Field.Type.Treasure;
                        AllTreasures.Add(new Treasure(ran.Next(1, difficulty*4 + 1), x, y));
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

                    Worldmap[x, y] = new Field(x, y, type);
                }
            }
            user.HP = user.HPMax;
            return;
        }

        public void playerMove(int x, int y)
        {
            if(playerAction == Situation.Walk)
            {
                try
                {
                    Field movedTo = Worldmap[user.axisX + x, user.axisY + y], currentAt = Worldmap[user.axisX, user.axisY];

                    if (user.axisX + x >= 0 && user.axisX + x < Worldmap.GetLength(0) && user.axisY + y < Worldmap.GetLength(1) && user.axisY + y >= 0)
                    {
                        if (movedTo.fieldType == Field.Type.Ground)
                        {
                            currentAt.fieldType = Field.Type.Ground;
                            user.axisX += x;
                            user.axisY += y;
                            movedTo.fieldType = Field.Type.Player;

                            foreach (Enemy mob in AllEnemies)
                            {
                                int xAx = ran.Next(-1, 2);
                                int yAx = ran.Next(-1, 2);
                                enemyMove(xAx, yAx, mob);
                            }
                        }

                        else if (movedTo.fieldType == Field.Type.Enemy || movedTo.fieldType == Field.Type.Boss)
                        {
                            playerAction = Situation.Fight;
                            curMob = AllEnemies.Find(n => n.axisX == user.axisX + x && n.axisY == user.axisY + y);
                        }

                        else if (movedTo.fieldType == Field.Type.Treasure)
                        {
                            playerAction = Situation.Loot;
                            curTre = AllTreasures.Find(n => n.axisX == user.axisX + x && n.axisY == user.axisY + y);
                        }
                    }
                }
                catch (Exception)
                {
                    Program.menuCommand();
                }
            }
        }

        private void enemyMove(int x, int y, Enemy mob)
        {
            Field.Type mobType = Worldmap[mob.axisX, mob.axisY].fieldType;

            if (mob.axisX + x >= 0 && mob.axisX + x < Worldmap.GetLength(0) && mob.axisY + y < Worldmap.GetLength(1) && mob.axisY + y >= 0)
            {
                if (Worldmap[mob.axisX + x, mob.axisY + y].fieldType == Field.Type.Ground)
                {
                    Worldmap[mob.axisX, mob.axisY].fieldType = Field.Type.Ground;
                    mob.axisX += x;
                    mob.axisY += y;
                    Worldmap[mob.axisX, mob.axisY].fieldType = mobType;
                }

                else if (Worldmap[mob.axisX + x, mob.axisY + y].fieldType == Field.Type.Player)
                {
                    curMob = mob;
                    playerAction = Situation.Fight;
                }
            }
        }

        public void drawMap(Boolean slowLoad)
        {
            Console.Clear();

            switch (playerAction)
            {
                case Situation.Walk:
                    for (int i = 0; i < Worldmap.GetLength(1); i++)
                    {
                        for (int j = 0; j < Worldmap.GetLength(0); j++)
                        {
                            if (slowLoad) System.Threading.Thread.Sleep(3);
                            Program.WriteColored(Worldmap[j, i].getColor(), $"{Worldmap[j, i].comfyView()}");
                        }
                        Console.Write("\n");
                    }
                    break;

                case Situation.Fight:
                    Program.WriteColored(curMob.spectrum, curMob.enemyArt + $"\n\n{ curMob.stats()}");
                    break;

                case Situation.Loot:
                    string output = $"{Enemy.art("treasure")}\n";

                    output += $"{curTre.gold} gold\n";

                    Program.WriteColored(ConsoleColor.Yellow, output);
                    break;

                case Situation.Dead:
                    Program.WriteColored(ConsoleColor.Red, "You Died! --- R to refresh\n");
                    break;

                case Situation.Done:
                    Program.WriteColored(ConsoleColor.Green, "You are done! --- R to refresh\n");
                    break;
            }

            Console.WriteLine("------------------------------------------------------------" +
                    $"\nPosition: {user.axisX}X {user.axisY}Y\n");

            Console.WriteLine(user.GetStats());
        }

        public void pickUp()
        {
            user.Gold += curTre.gold;

            user.SaveCharacter();
            Worldmap[curTre.axisX, curTre.axisY].fieldType = Field.Type.Ground;
            AllTreasures.Remove(curTre);
            playerAction = Situation.Walk;
        }

        public void playerAttack()
        {
            /*if(playerAction == Situation.Fight)
            {
                curMob.curHP -= ran.Next(user.dmg/2, user.dmg);
                user.curHp -= ran.Next(curMob.dmg / 2, curMob.dmg);

                if (curMob.curHP <= 0)
                {
                    playerAction = Situation.Walk;

                    Worldmap[curMob.axisX, curMob.axisY].fieldType = Field.Type.Treasure;
                    AllTreasures.Add(curMob.uponDeath());

                    AllEnemies.Remove(curMob);
                    user.exp += curMob.exp;

                    user.writeStats();

                    curMob = null;

                    if(AllEnemies.Count == 0)
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
            }*/
        }

        public enum Situation
        {
            Fight,
            Loot,
            Walk,
            Dead,
            Done
        }
    }
}
