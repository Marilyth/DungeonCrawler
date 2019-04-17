using System;
using DungeonCrawler.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Objects
{
    class WorldMap
    {
        public Field[,] Fields;
        public Player User;

        public WorldMap(int xTotal, int yTotal){
            Fields = new Field[yTotal, xTotal];
            for(int i = 0; i < xTotal; i++){
                for(int j = 0; j < yTotal; j++){
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
            switch(biome){
                case BiomeType.Swamp:
                    creationChance = MapGeneration.FieldCreationChanceSwamp;
                    duplicationChance = MapGeneration.FieldDuplicationChanceSwamp;
                    break;
                case BiomeType.Desert:
                    creationChance = MapGeneration.FieldCreationChanceDesert;
                    duplicationChance = MapGeneration.FieldDuplicationChanceDesert;
                    break;
                case BiomeType.Grasslands:
                    creationChance = MapGeneration.FieldCreationChanceGrasslands;
                    duplicationChance = MapGeneration.FieldDuplicationChanceGrasslands;
                    break;
                case BiomeType.Cave:
                    creationChance = MapGeneration.FieldCreationChanceCave;
                    duplicationChance = MapGeneration.FieldDuplicationChanceCave;
                    break;
            }

            FieldType curType = MapGeneration.CreateField(creationChance);

            for (int xc = xRel; xc - xRel < width && xc < Fields.GetLength(1); xc++)
            {
                for (int yc = yRel; yc - yRel < height && yc < Fields.GetLength(0); yc++)
                {   
                    List<FieldType> partners = new List<FieldType>();
                    partners.Add(xc == 0 ? FieldType.None : Fields[yc, xc - 1].Type);
                    partners.Add(yc == 0 ? FieldType.None : Fields[yc - 1, xc].Type);
                    partners.Add(yc == Fields.GetLength(0) - 1 ? FieldType.None : Fields[yc+1, xc].Type);
                    partners.Add(xc == Fields.GetLength(1) - 1 ? FieldType.None : Fields[yc, xc+1].Type);
                    partners = partners.OrderBy(t => duplicationChance[t]).ToList();

                    var roll = Program.ran.NextDouble();
                    foreach(var field in partners){
                        if(roll < duplicationChance[field]){
                            curType = field;
                            break;
                        }
                    }
                    if(curType == FieldType.None) curType = MapGeneration.CreateField(creationChance);

                    Fields[yc, xc] = new Field(curType);
                    curType = FieldType.None;
                }
            }
        }

        public void DrawVisibleMap(){
            for(int x = Math.Max(User.XAxis - 10, 0); x < Math.Min(Fields.GetLength(1) - 1, User.XAxis + 10); x++){
                for (int y = Math.Max(User.YAxis - 10, 0); y < Math.Min(Fields.GetLength(0) - 1, User.YAxis + 10); y++)
                {
                    var fieldString = Fields[y, x].Occupant?.ToString() ?? "   ";
                    var fieldColour = Field.FieldTextColour(Fields[y, x].Type);

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
            for (int i = 0; i < Fields.GetLength(1); i++)
            {
                for (int j = 0; j < Fields.GetLength(0); j++)
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

        public void PlayerMove(int y = 0, int x = 0){
            if(User.XAxis + x < 0 || User.XAxis + x >= Fields.GetLength(1)) return;
            if(User.YAxis + y < 0 || User.YAxis + y >= Fields.GetLength(0)) return;

            User.XAxis += x;
            User.YAxis += y;
            Fields[User.YAxis, User.XAxis].Occupant = User;
            Fields[User.YAxis - y, User.XAxis - x].Occupant = null;
        }

        public void SetPlayer(int x = 0, int y = 0){
            User = new Player(x, y);
            Fields[y, x].Occupant = User;
        }
    }
    public class Field
    {
        public int XAxis, YAxis;
        public FieldType Type;
        public Object Occupant;

        public Field(FieldType type)
        {
            Type = type;
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
                    return ConsoleColor.Black;
                default:
                    return ConsoleColor.Black;
            }
        }
    }

    public enum FieldType { Dirt, Grass, Water, Sand, Wall, Stone, None };
    public enum BiomeType {Cave, Swamp, Grasslands, Desert}

    public static class MapGeneration{
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

        public static FieldType CreateField(Dictionary<FieldType, double> biome){
            FieldType curType = FieldType.None;

            while(curType == FieldType.None){
                var roll = Program.ran.NextDouble();
                var type = biome.ToList()[Program.ran.Next(0, biome.Count)];
                if(roll < type.Value) curType = type.Key;
            }

            return curType;
        }
    }
}
