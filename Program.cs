﻿using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Globalization;
using DungeonCrawler.Objects;

namespace DungeonCrawler
{
    class Program
    {
        public Objects.WorldMap map;
        public static AllEnemies information;
        public static Random ran = new Random();

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();
        }

        public void Start()
        {
            Console.SetWindowSize(181, 30);
            information = new AllEnemies();

            Menu();

            InputLoop();
        }

        private static T ChooseEnum<T>() where T : System.Enum
        {
            var enumOptions = Enum.GetNames(typeof(T));
            string menu = String.Join("\n", enumOptions.Select((x, i) => $"[{i}] {x}"));
            Console.WriteLine(menu);
            Console.Write("Please enter one of the above numbers: ");
            bool properInput = false;
            int decision = -1;
            while (!(properInput = int.TryParse("" + Console.ReadLine(), out decision)) && decision > 0 && decision < enumOptions.Length)
                Console.Write("Wrong input.\nPlease enter one of the above numbers: ");

            return (T)Enum.Parse(typeof(T), enumOptions[decision]);
        }

        public void Menu()
        {
            switch (ChooseEnum<MenuOptions>())
            {
                case MenuOptions.CreateNewWorld:
                    Console.Write("How wide do you want your world to be?: ");
                    int width = int.Parse(Console.ReadLine());
                    Console.Write("How long do you want your world to be?: ");
                    int height = int.Parse(Console.ReadLine());
                    map = new Objects.WorldMap(width, height);
                    Console.WriteLine("Choose your biome");
                    var biome = ChooseEnum<Objects.BiomeType>();
                    map.FillMapRandom(width, height, biome, 0, 0);
                    Console.Write("What is your name?: ");
                    map.SetPlayer(map.Fields.GetLength(1) / 2, map.Fields.GetLength(0) / 2, Console.ReadLine());
                    break;
                case MenuOptions.LoadExistingWorld:
                    map = WorldMap.LoadMap();
                    break;
            }

            InputLoop();
        }

        public void InputLoop()
        {
            Console.Clear();
            Console.SetWindowSize(60, 30);
            map.DrawVisibleMap();
            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey();
                Console.Clear();

                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        map.PlayerMove(0, -1);
                        break;
                    case ConsoleKey.DownArrow:
                        map.PlayerMove(0, 1);
                        break;
                    case ConsoleKey.RightArrow:
                        map.PlayerMove(1, 0);
                        break;
                    case ConsoleKey.LeftArrow:
                        map.PlayerMove(-1, 0);
                        break;
                    case ConsoleKey.Delete:
                        Menu();
                        return;
                    case ConsoleKey.S:
                        map.SaveMap();
                        Console.WriteLine("Saved map successfully!");
                        break;
                    case ConsoleKey.L:
                        map = WorldMap.LoadMap();
                        Console.WriteLine("Loaded map successfully!");
                        break;
                    default:
                        var worked = int.TryParse("" + cki.KeyChar, out int number);
                        if (worked) map.SetField(number);
                        break;

                }
                map.DrawVisibleMap();
                //Console.WriteLine(map.User.GetStats());

            } while (cki.Key != ConsoleKey.Escape);
        }

        private enum MenuOptions { CreateNewWorld, LoadExistingWorld }
    }
}

