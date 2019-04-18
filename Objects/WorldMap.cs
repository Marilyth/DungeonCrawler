using System;
using DungeonCrawler.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DungeonCrawler.Objects
{
    class WorldMap
    {
        public Field[,] Fields;
        public Player User;

        public WorldMap(int xTotal, int yTotal)
        {
            Fields = new Field[yTotal, xTotal];
            for (int i = 0; i < xTotal; i++)
            {
                for (int j = 0; j < yTotal; j++)
                {
                    Fields[j, i] = new Field(FieldType.None);
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
                    partners.Add(xc == 0 ? FieldType.None : Fields[yc, xc - 1].Type);
                    partners.Add(yc == 0 ? FieldType.None : Fields[yc - 1, xc].Type);
                    partners.Add(yc == Fields.GetLength(0) - 1 ? FieldType.None : Fields[yc + 1, xc].Type);
                    partners.Add(xc == Fields.GetLength(1) - 1 ? FieldType.None : Fields[yc, xc + 1].Type);
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

                    Fields[yc, xc] = new Field(curType);
                    curType = FieldType.None;
                }
            }
        }

        public HashSet<Tuple<int, int>> GetShadows(){
            //Calculate line of sight
            var shadowFields = new HashSet<Tuple<int, int>>();
            //First line
            for(int x = Math.Max(User.XAxis - 10, 0); x <= Math.Min(Fields.GetLength(1) - 1, User.XAxis + 10); x++){
                var y = Math.Max(User.YAxis - 10, 0);
                var normal = Math.Sqrt(Math.Pow(x - User.XAxis, 2) + Math.Pow(y - User.YAxis, 2));
                Tuple<double, double> shadowVector = Tuple.Create((x - User.XAxis)/(normal*1.1), (y - User.YAxis)/(normal*1.1));

                bool inLineOfSight = true;
                int i = 1;
                Tuple<int, int> curField;
                Tuple<int, int> lastField = Tuple.Create(User.XAxis, User.YAxis);
                Tuple<int, int> goal = Tuple.Create(x, y);
                while((curField = Tuple.Create(User.XAxis + (int)(i*shadowVector.Item1), User.YAxis + (int)(i*shadowVector.Item2))).Item1 != goal.Item1 || curField.Item2 != goal.Item2){
                    i++;
                    if(lastField.Item1 == curField.Item1 && lastField.Item2 == curField.Item2) continue;
                    
                    if(inLineOfSight)
                        inLineOfSight = Fields[curField.Item2, curField.Item1].Occupant == null;
                    else
                        shadowFields.Add(curField);

                    lastField = curField;
                }

                if(!inLineOfSight)
                    shadowFields.Add(goal);
            }

            //Last line
            for(int x = Math.Max(User.XAxis - 10, 0); x <= Math.Min(Fields.GetLength(1) - 1, User.XAxis + 10); x++){
                var y = Math.Min(Fields.GetLength(0) - 1, User.YAxis + 10);
                var normal = Math.Sqrt(Math.Pow(x - User.XAxis, 2) + Math.Pow(y - User.YAxis, 2));
                Tuple<double, double> shadowVector = Tuple.Create((x - User.XAxis)/(normal*1.1), (y - User.YAxis)/(normal*1.1));

                bool inLineOfSight = true;
                int i = 1;
                Tuple<int, int> curField;
                Tuple<int, int> lastField = Tuple.Create(User.XAxis, User.YAxis);
                Tuple<int, int> goal = Tuple.Create(x, y);
                while((curField = Tuple.Create(User.XAxis + (int)(i*shadowVector.Item1), User.YAxis + (int)(i*shadowVector.Item2))).Item1 != goal.Item1 || curField.Item2 != goal.Item2){
                    i++;
                    if(lastField.Item1 == curField.Item1 && lastField.Item2 == curField.Item2) continue;
                    
                    if(inLineOfSight)
                        inLineOfSight = Fields[curField.Item2, curField.Item1].Occupant == null;
                    else
                        shadowFields.Add(curField);

                    lastField = curField;
                }

                if(!inLineOfSight)
                    shadowFields.Add(goal);
            }

            //Left line
            for(int y = Math.Max(User.YAxis - 10, 0); y <= Math.Min(Fields.GetLength(0) - 1, User.YAxis + 10); y++){
                var x = Math.Max(User.XAxis - 10, 0);
                var normal = Math.Sqrt(Math.Pow(x - User.XAxis, 2) + Math.Pow(y - User.YAxis, 2));
                Tuple<double, double> shadowVector = Tuple.Create((x - User.XAxis)/(normal*1.1), (y - User.YAxis)/(normal*1.1));

                bool inLineOfSight = true;
                Tuple<int, int> LOSBreaker;
                int i = 1;
                Tuple<int, int> curField;
                Tuple<int, int> lastField = Tuple.Create(User.XAxis, User.YAxis);
                Tuple<int, int> goal = Tuple.Create(x, y);
                while((curField = Tuple.Create(User.XAxis + (int)(i*shadowVector.Item1), User.YAxis + (int)(i*shadowVector.Item2))).Item1 != goal.Item1 || curField.Item2 != goal.Item2){
                    i++;
                    if(lastField.Item1 == curField.Item1 && lastField.Item2 == curField.Item2) continue;
                    
                    if(inLineOfSight)
                        inLineOfSight = Fields[curField.Item2, curField.Item1].Occupant == null;
                    else
                        shadowFields.Add(curField);

                    lastField = curField;
                }
                if(!inLineOfSight)
                    shadowFields.Add(goal);
            }

            //Right line
            for(int y = Math.Max(User.YAxis - 10, 0); y <= Math.Min(Fields.GetLength(0) - 1, User.YAxis + 10); y++){
                var x = Math.Min(Fields.GetLength(1) - 1, User.XAxis + 10);
                var normal = Math.Sqrt(Math.Pow(x - User.XAxis, 2) + Math.Pow(y - User.YAxis, 2));
                Tuple<double, double> shadowVector = Tuple.Create((x - User.XAxis)/(normal*1.1), (y - User.YAxis)/(normal*1.1));

                bool inLineOfSight = true;
                int i = 1;
                Tuple<int, int> curField;
                Tuple<int, int> lastField = Tuple.Create(User.XAxis, User.YAxis);
                Tuple<int, int> goal = Tuple.Create(x, y);
                while((curField = Tuple.Create(User.XAxis + (int)(i*shadowVector.Item1), User.YAxis + (int)(i*shadowVector.Item2))).Item1 != goal.Item1 || curField.Item2 != goal.Item2){
                    i++;
                    if(lastField.Item1 == curField.Item1 && lastField.Item2 == curField.Item2) continue;
                    
                    if(inLineOfSight)
                        inLineOfSight = Fields[curField.Item2, curField.Item1].Occupant == null;
                    else
                        shadowFields.Add(curField);

                    lastField = curField;
                }

                if(!inLineOfSight)
                    shadowFields.Add(goal);
            }

            return shadowFields;
        }

        public void DrawVisibleMap()
        {
            //Calculate line of sight
            var shadowFields = GetShadows();

            for (int y = Math.Max(User.YAxis - 10, 0); y < Math.Min(Fields.GetLength(0) - 1, User.YAxis + 10); y++)
            {
                for (int x = Math.Max(User.XAxis - 10, 0); x < Math.Min(Fields.GetLength(1) - 1, User.XAxis + 10); x++)
                {
                    var fieldString = !shadowFields.Contains(Tuple.Create(x, y)) ? (Fields[y, x].Occupant?.ToString() ?? "   ") : "   ";
                    var fieldColour = !shadowFields.Contains(Tuple.Create(x, y)) ? Field.FieldTextColour(Fields[y, x].Type) : ConsoleColor.Black;

                    Console.BackgroundColor = fieldColour;
                    Console.ForegroundColor = Fields[y, x].Occupant?.GetColour() ?? ConsoleColor.Gray;

                    Console.Write(fieldString);
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        public void DrawMap()
        {
            for (int j = 0; j < Fields.GetLength(0); j++)
            {
                for (int i = 0; i < Fields.GetLength(1); i++)
                {
                    var fieldString = "   ";//Field.FieldToString(Fields[i, j].Type);
                    var fieldColour = Field.FieldTextColour(Fields[j, i].Type);

                    Console.BackgroundColor = fieldColour;
                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write(fieldString);
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        public void PlayerMove(int x = 0, int y = 0)
        {
            if (User.XAxis + x < 0 || User.XAxis + x >= Fields.GetLength(1)) return;
            if (User.YAxis + y < 0 || User.YAxis + y >= Fields.GetLength(0)) return;

            var previousField = Fields[User.YAxis, User.XAxis];
            var nextField = Fields[User.YAxis + y, User.XAxis + x];
            if (nextField.Occupant == null)
            {
                User.XAxis += x;
                User.YAxis += y;
                nextField.Occupant = User;
                if (previousField.Occupant?.Name.Equals(User.Name) ?? false) previousField.Occupant = null;
            }
            else if (User.Name.Equals("God"))
            {
                User.XAxis += x;
                User.YAxis += y;
                if (previousField.Occupant?.Name.Equals(User.Name) ?? false) previousField.Occupant = null;
            }
        }

        public void SetPlayer(int x = 0, int y = 0, string name = "God")
        {
            User = new Player(x, y);
            User.Name = name;
            Fields[y, x].Occupant = User;
        }

        public void SetField(FieldType type)
        {
            if (User.Name?.Equals("God") ?? false){
                Fields[User.YAxis, User.XAxis] = new Field(type);
            }
        }

        public void SetField(int type)
        {
            if (User.Name?.Equals("God") ?? false)
            {
                var types = Enum.GetValues(typeof(FieldType)).Cast<FieldType>().ToArray();
                Fields[User.YAxis, User.XAxis] = new Field(types[type % types.Length]);
            }
        }

        public void SaveMap()
        {
            using (var sw = new System.IO.StreamWriter("data\\map.json"))
            {
                sw.WriteLine(JsonConvert.SerializeObject(this));
            }
        }

        public static WorldMap LoadMap()
        {
            using (var sr = new System.IO.StreamReader("data\\map.json"))
            {
                var map = JsonConvert.DeserializeObject<WorldMap>(sr.ReadToEnd());
                return map;
            }
        }
    }
    public class Field
    {
        public int XAxis, YAxis;
        public FieldType Type;
        public Object Occupant;
        public Object Burried;
        public Object Hidden;

        public Field(FieldType type)
        {
            Type = type;
            if (Type == FieldType.Wall)
                Occupant = new Object()
                {
                    Name = "Wall"
                };
        }

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
                    return ConsoleColor.DarkGreen;
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
    public enum BiomeType { Cave, Swamp, Grasslands, Desert }

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
