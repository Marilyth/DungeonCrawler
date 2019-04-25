using System;
using DungeonCrawler.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Timers;

namespace DungeonCrawler.Objects
{
    public class WorldMap
    {
        public FieldType[,] Fields;
        private Dictionary<Tuple<int, int>, BaseObject> BaseObjectDict;
        public List<BaseObject> BaseObjects;
        private Player user;
        public enum PlayerAction { Walk, Look, Use, Push, Pull, Combat }
        private DateTime Timer = DateTime.Now;

        public WorldMap(int xTotal, int yTotal)
        {
            Fields = new FieldType[yTotal, xTotal];
            BaseObjectDict = new Dictionary<Tuple<int, int>, BaseObject>();
            BaseObjects = new List<BaseObject>();

            for (int i = 0; i < xTotal; i++)
            {
                for (int j = 0; j < yTotal; j++)
                {
                    Fields[j, i] = FieldType.None;
                }
            }
        }

        public void FillMapRandom(int height, int width, BiomeType biome, int x = -1, int y = -1)
        {
            int xRel = x == -1 ? Program.ran.Next(Fields.GetLength(1) - width) : x;
            int yRel = y == -1 ? Program.ran.Next(Fields.GetLength(0) - height) : y;
            Dictionary<FieldType, double> creationChance = null;
            Dictionary<FieldType, double> duplicationChance = null;
            switch (biome)
            {
                case BiomeType.Swamp:
                    creationChance = RNGMapGeneration.FieldCreationChanceSwamp;
                    duplicationChance = RNGMapGeneration.FieldDuplicationChanceSwamp;
                    break;
                case BiomeType.Desert:
                    creationChance = RNGMapGeneration.FieldCreationChanceDesert;
                    duplicationChance = RNGMapGeneration.FieldDuplicationChanceDesert;
                    break;
                case BiomeType.Grasslands:
                    creationChance = RNGMapGeneration.FieldCreationChanceGrasslands;
                    duplicationChance = RNGMapGeneration.FieldDuplicationChanceGrasslands;
                    break;
                case BiomeType.Cave:
                    creationChance = RNGMapGeneration.FieldCreationChanceCave;
                    duplicationChance = RNGMapGeneration.FieldDuplicationChanceCave;
                    break;
            }

            FieldType curType = RNGMapGeneration.CreateField(creationChance);

            for (int xc = xRel; xc - xRel < width && xc < Fields.GetLength(1); xc++)
            {
                for (int yc = yRel; yc - yRel < height && yc < Fields.GetLength(0); yc++)
                {
                    List<FieldType> partners = new List<FieldType>();
                    partners.Add(xc == 0 ? FieldType.None : Fields[yc, xc - 1]);
                    partners.Add(yc == 0 ? FieldType.None : Fields[yc - 1, xc]);
                    partners.Add(yc == Fields.GetLength(0) - 1 ? FieldType.None : Fields[yc + 1, xc]);
                    partners.Add(xc == Fields.GetLength(1) - 1 ? FieldType.None : Fields[yc, xc + 1]);
                    partners = partners.OrderBy(t => duplicationChance[t]).ToList();

                    var roll = Program.ran.NextDouble();
                    foreach (var field in partners)
                    {
                        if (roll < duplicationChance[field])
                        {
                            curType = field;
                            break;
                        }
                    }
                    if (curType == FieldType.None) curType = RNGMapGeneration.CreateField(creationChance);

                    SetField(curType, xc, yc);
                    curType = FieldType.None;
                }
            }
        }

        public HashSet<Tuple<int, int>> GetLineOfSight()
        {
            //Calculate line of sight
            var visibleFields = new HashSet<Tuple<int, int>>() { Tuple.Create(user.XAxis, user.YAxis) };

            //First line
            for (int x = Math.Max(user.XAxis - 10, 0); x <= Math.Min(Fields.GetLength(1) - 1, user.XAxis + 10); x++)
            {
                var y = Math.Max(user.YAxis - 10, 0);
                Tuple<int, int> goal = Tuple.Create(x, y);
                visibleFields.UnionWith(getVisibleFields(goal));
            }

            //Last line
            for (int x = Math.Max(user.XAxis - 10, 0); x <= Math.Min(Fields.GetLength(1) - 1, user.XAxis + 10); x++)
            {
                var y = Math.Min(Fields.GetLength(0) - 1, user.YAxis + 10);
                Tuple<int, int> goal = Tuple.Create(x, y);
                visibleFields.UnionWith(getVisibleFields(goal));
            }

            //Left line
            for (int y = Math.Max(user.YAxis - 10, 0); y <= Math.Min(Fields.GetLength(0) - 1, user.YAxis + 10); y++)
            {
                var x = Math.Max(user.XAxis - 10, 0);
                Tuple<int, int> goal = Tuple.Create(x, y);
                visibleFields.UnionWith(getVisibleFields(goal));
            }

            //Right line
            for (int y = Math.Max(user.YAxis - 10, 0); y <= Math.Min(Fields.GetLength(0) - 1, user.YAxis + 10); y++)
            {
                var x = Math.Min(Fields.GetLength(1) - 1, user.XAxis + 10);
                Tuple<int, int> goal = Tuple.Create(x, y);
                visibleFields.UnionWith(getVisibleFields(goal));
            }

            return visibleFields;
        }

        private HashSet<Tuple<int, int>> getVisibleFields(Tuple<int, int> goal)
        {
            HashSet<Tuple<int, int>> visibleFields = new HashSet<Tuple<int, int>>();

            var normal = Math.Sqrt(Math.Pow(goal.Item1 - user.XAxis, 2) + Math.Pow(goal.Item2 - user.YAxis, 2));
            Tuple<double, double> shadowVector = Tuple.Create((goal.Item1 - user.XAxis) / (normal * 1.1), (goal.Item2 - user.YAxis) / (normal * 1.1));

            bool inLineOfSight = true;
            int i = 1;
            Tuple<int, int> curField;
            Tuple<int, int> lastField = Tuple.Create(user.YAxis, user.XAxis);
            while (!(curField = Tuple.Create(user.XAxis + (int)(i * shadowVector.Item1), user.YAxis + (int)(i * shadowVector.Item2))).Equals(goal) && inLineOfSight)
            {
                i++;
                if (lastField.Equals(curField)) continue;
                visibleFields.Add(curField);
                var worked = BaseObjectDict.TryGetValue(Tuple.Create(curField.Item2, curField.Item1), out BaseObject occupant);
                if (worked)
                {
                    inLineOfSight = occupant?.Visibility != ObjectVisibility.Occupying;
                }

                lastField = curField;
            }

            if (inLineOfSight)
                visibleFields.Add(goal);

            return visibleFields;
        }

        public async Task DrawField(int x, int y)
        {
            var xConsole = (x % 20) * 3;
            var yConsole = y % 20;

            BaseObjectDict.TryGetValue(Tuple.Create(y, x), out BaseObject occupant);
            if (user.XAxis == x && user.YAxis == y)
            {
                occupant = user;
            }

            var fieldString = (occupant?.ToString() ?? "   ");
            var fieldColour = Field.FieldTextColour(Fields[y, x]);

            Console.SetCursorPosition(xConsole, yConsole);
            Console.BackgroundColor = fieldColour;
            Console.ForegroundColor = occupant?.GetColour() ?? ConsoleColor.Gray;
            Console.Write(fieldString);

            Console.ResetColor();
        }

        public async Task DrawMapSegment()
        {
            await Program.Client.InterpretLog();
            Console.SetCursorPosition(0, 0);

            var xSegment = (int)user.XAxis / 20;
            var ySegment = (int)user.YAxis / 20;
            for (int y = ySegment * 20; y < Math.Min(Fields.GetLength(0), ySegment * 20 + 20); y++)
            {
                for (int x = xSegment * 20; x < Math.Min(Fields.GetLength(1), xSegment * 20 + 20); x++)
                {
                    BaseObjectDict.TryGetValue(Tuple.Create(y, x), out BaseObject occupant);
                    var fieldString = (occupant?.ToString() ?? "   ");
                    var fieldColour = Field.FieldTextColour(Fields[y, x]);

                    if (user.XAxis == x && user.YAxis == y)
                    {
                        occupant = user;
                        fieldString = user.ToString();
                    }

                    Console.BackgroundColor = fieldColour;
                    Console.ForegroundColor = occupant?.GetColour() ?? ConsoleColor.Gray;

                    Console.Write(fieldString);
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        public async Task PlayerMove(int x = 0, int y = 0, PlayerAction action = PlayerAction.Walk)
        {
            if (user.XAxis + x < 0 || user.XAxis + x >= Fields.GetLength(1)) return;
            if (user.YAxis + y < 0 || user.YAxis + y >= Fields.GetLength(0)) return;

            switch (action)
            {
                case PlayerAction.Walk:
                    var previousField = Tuple.Create(user.YAxis, user.XAxis);
                    var nextField = Tuple.Create(user.YAxis + y, user.XAxis + x);
                    if (!BaseObjectDict.ContainsKey(nextField) || BaseObjectDict[nextField].isWalkThrough || user.Name.Equals("God"))
                    {
                        var xSegmentPrevious = (int)user.XAxis / 20;
                        var ySegmentPrevious = (int)user.YAxis / 20;
                        user.XAxis += x;
                        user.YAxis += y;
                        var xSegmentAfter = (int)user.XAxis / 20;
                        var ySegmentAfter = (int)user.YAxis / 20;

                        if (xSegmentAfter != xSegmentPrevious || ySegmentAfter != ySegmentPrevious)
                        {
                            await DrawMapSegment();
                        }
                        else
                        {
                            DrawField(user.XAxis, user.YAxis);
                            DrawField(user.XAxis - x, user.YAxis - y);
                        }

                        WritePlayerStats();

                        await Program.Client.StatsChanged(user);
                    }
                    break;

                case PlayerAction.Look:
                    WriteLookAction(user.XAxis + x, user.YAxis + y);
                    break;
            }
        }

        public void WritePlayerStats()
        {
            Console.SetCursorPosition(0, 21);
            Console.WriteLine(user.GetStats());
        }

        public void WriteLookAction(int x, int y){
            Console.SetCursorPosition(0, 24);
            Console.WriteLine($"You see {Fields[y, x].ToString()}.");
            if(BaseObjectDict.ContainsKey(Tuple.Create(y, x))){
                Console.WriteLine($"On top, you see a {BaseObjectDict[Tuple.Create(y, x)].Name}.");
            }
        }

        public void SetPlayer(int x = -1, int y = -1, string name = "")
        {
            user = new Player(x, y);
            user.Name = name;
        }

        public void SetPlayer(Player player)
        {
            user = player;
        }

        public Player GetPlayer()
        {
            return user;
        }

        public async Task SetField(FieldType type, int x, int y)
        {
            Fields[y, x] = type;
            if (type == FieldType.Wall)
            {
                AddBaseObject(new BaseObject(x, y, "Wall"));
            }

            //await DrawField(x, y);
        }

        public async Task SetField(int type)
        {
            if (user.Name?.StartsWith("God") ?? false)
            {
                var types = Enum.GetValues(typeof(FieldType)).Cast<FieldType>().ToArray();
                Fields[user.YAxis, user.XAxis] = types[type % types.Length];
                if (types[type % types.Length] == FieldType.Wall)
                {
                    AddBaseObject(new BaseObject(user.XAxis, user.YAxis, "Wall"));
                }

                await Program.Client.FieldChanged(user.XAxis, user.YAxis, Fields[user.YAxis, user.XAxis]);
                await DrawField(user.XAxis, user.YAxis);
            }
        }

        public void LoadBaseObjects()
        {
            foreach (BaseObject o in BaseObjects)
            {
                BaseObjectDict[Tuple.Create(o.YAxis, o.XAxis)] = o;
            }
        }

        public void AddBaseObject(BaseObject o)
        {
            BaseObjects.Add(o);
            BaseObjectDict[Tuple.Create(o.YAxis, o.XAxis)] = o;
        }

        public void RemoveBaseObject(int x, int y)
        {
            if (BaseObjectDict.ContainsKey(Tuple.Create(y, x)))
            {
                var obj = BaseObjectDict[Tuple.Create(y, x)];
                BaseObjectDict.Remove(Tuple.Create(y, x));
                BaseObjects.Remove(obj);
            }
        }

        public Dictionary<Tuple<int, int>, BaseObject> GetBaseObjectDict()
        {
            return BaseObjectDict;
        }

        public void SaveMap()
        {
            var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var targetDir = Path.Combine(curDir, "map");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            using (var sw = new System.IO.StreamWriter($"{targetDir}\\map.json"))
            {
                sw.WriteLine(JsonConvert.SerializeObject(this));
            }
        }

        public static WorldMap LoadMap(string mapJson = null)
        {
            if (mapJson == null)
            {
                var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var targetDir = Path.Combine(curDir, "map");
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                using (var sr = new System.IO.StreamReader($"{targetDir}\\map.json"))
                {
                    var map = JsonConvert.DeserializeObject<WorldMap>(sr.ReadToEnd());
                    map.LoadBaseObjects();
                    return map;
                }
            }
            else
            {
                var map = JsonConvert.DeserializeObject<WorldMap>(mapJson);
                map.LoadBaseObjects();
                return map;
            }
        }
    }
    public static class Field
    {

        public static ConsoleColor FieldTextColour(FieldType type)
        {
            switch (type)
            {
                case FieldType.Dirt:
                    return ConsoleColor.DarkYellow;
                case FieldType.Sand:
                    return ConsoleColor.Yellow;
                case FieldType.Water:
                    return ConsoleColor.Blue;
                case FieldType.Grass:
                    return ConsoleColor.Green;
                case FieldType.Wall:
                    return ConsoleColor.DarkGray;
                case FieldType.Stone:
                    return ConsoleColor.Gray;
                default:
                    return ConsoleColor.Black;
            }
        }
    }

    public enum FieldType { Dirt, Grass, Water, Sand, Wall, Stone, None };
    public enum BaseObjectType { Wall };
    public enum BiomeType { Cave, Swamp, Grasslands, Desert };

    public static class RNGMapGeneration
    {
        public static Dictionary<FieldType, double> FieldDuplicationChanceSwamp = new Dictionary<FieldType, double>(){
            {FieldType.Dirt, 0.7},
            {FieldType.Water, 0.4},
            {FieldType.Sand, 0},
            {FieldType.Grass, 0.9},
            {FieldType.Stone, 0},
            {FieldType.Wall, 0.2},
            {FieldType.None, 0}
        };

        public static Dictionary<FieldType, double> FieldDuplicationChanceDesert = new Dictionary<FieldType, double>(){
            {FieldType.Dirt, 0.2},
            {FieldType.Water, 0.1},
            {FieldType.Sand, 0.95},
            {FieldType.Grass, 0.01},
            {FieldType.Stone, 0},
            {FieldType.Wall, 0.7},
            {FieldType.None, 0}
        };

        public static Dictionary<FieldType, double> FieldDuplicationChanceGrasslands = new Dictionary<FieldType, double>(){
            {FieldType.Dirt, 0.1},
            {FieldType.Water, 0.4},
            {FieldType.Sand, 0.1},
            {FieldType.Grass, 0.95},
            {FieldType.Stone, 0.1},
            {FieldType.Wall, 0.1},
            {FieldType.None, 0}
        };

        public static Dictionary<FieldType, double> FieldDuplicationChanceCave = new Dictionary<FieldType, double>(){
            {FieldType.Dirt, 0.1},
            {FieldType.Water, 0.1},
            {FieldType.Sand, 0.1},
            {FieldType.Grass, 0.1},
            {FieldType.Wall, 0.2},
            {FieldType.Stone, 0.8},
            {FieldType.None, 0}
        };

        public static Dictionary<FieldType, double> FieldCreationChanceDesert = new Dictionary<FieldType, double>(){
            {FieldType.Dirt, 0.2},
            {FieldType.Water, 0.1},
            {FieldType.Sand, 0.95},
            {FieldType.Grass, 0.1},
            {FieldType.Wall, 0}
        };

        public static Dictionary<FieldType, double> FieldCreationChanceSwamp = new Dictionary<FieldType, double>(){
            {FieldType.Dirt, 1},
            {FieldType.Water, 1},
            {FieldType.Sand, 0},
            {FieldType.Grass, 1}
        };

        public static Dictionary<FieldType, double> FieldCreationChanceGrasslands = new Dictionary<FieldType, double>(){
            {FieldType.Dirt, 0.3},
            {FieldType.Water, 0.5},
            {FieldType.Sand, 0},
            {FieldType.Grass, 1}
        };

        public static Dictionary<FieldType, double> FieldCreationChanceCave = new Dictionary<FieldType, double>(){
            {FieldType.Wall, 1},
            {FieldType.Stone, 1}
        };

        public static FieldType CreateField(Dictionary<FieldType, double> biome)
        {
            FieldType curType = FieldType.None;

            while (curType == FieldType.None)
            {
                var roll = Program.ran.NextDouble();
                var type = biome.ToList()[Program.ran.Next(0, biome.Count)];
                if (roll < type.Value) curType = type.Key;
            }

            return curType;
        }
    }
}
