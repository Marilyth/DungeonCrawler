using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler
{
    class AllEnemies
    {
        public Dictionary<string, int> enemyLevel = new Dictionary<string, int>();
        public Dictionary<string, int> bossLevel = new Dictionary<string, int>();
        public Random ran = new Random();

        public AllEnemies()
        {
            enemyLevel.Add("Rat", 1);
            enemyLevel.Add("Snake", 3);
            enemyLevel.Add("Bat", 5);
            enemyLevel.Add("Ghost", 12);
            bossLevel.Add("Skeleton", 20);
            enemyLevel.Add("Phoenix", 30);
            bossLevel.Add("Vampire", 27);
            bossLevel.Add("Spider", 6);
            bossLevel.Add("Dragon", 40);

            enemyLevel.OrderBy(x => x.Value);
            bossLevel.OrderBy(x => x.Value);
        }

        public List<string> getEligable(int level)
        {
            return enemyLevel.Where(x => x.Value <= level && x.Value >= level - 10).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
        }
        public List<string> getEligableBoss(int level)
        {
            return bossLevel.Where(x => x.Value <= level && x.Value >= level - 10).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
        }

        public string getEnemy(int level)
        {
            List<string> enemies = getEligable(level);
            return enemies[ran.Next(0, enemies.Count)];
        }
        public string getBoss(int level)
        {
            List<string> bosses = getEligableBoss(level);
            return bosses[ran.Next(0, bosses.Count)];
        }
    }
}
