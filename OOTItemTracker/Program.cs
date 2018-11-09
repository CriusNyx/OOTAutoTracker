using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace OOTItemTracker
{
    /// <summary>
    /// OOT Item tracker
    /// This software is designed to automatically track an ocarina of time save.
    /// It works with emulators to make tracking an oot save easier.
    /// </summary>
    class Program
    {
        #region Program Constants
        public const int NAVI_ADDRESS = 0x3A;

        public const int EVENT_CHECK_ADDRESS = 0xED4;
        public const int EVENT_CHECK_LENGTH = 28;

        public const int ITEM_GET_ADDRESS = 0xEF0;
        public const int ITEM_GET_TABLE_LENGTH = 8;

        public const int INF_ADDRESS = 0xEF8;
        public const int INF_LENGTH = 60;

        public const int SCENE_COUNT = 101;

        public const int SCENE_DATA_SIZE = 0x1C;
        public const int SCENE_DATA_HEAD = 0xD4;
        public const int CHEST_WORD_HEAD = 0x0;
        public const int COLLECTABLE_WORD_HEAD = 0xC;

        public const int SAVE_FILE_HEAD = 0x20820;
        public const int SAVE_FILE_SIZE = 0x1450;
        #endregion

        #region Fields
        static string savePath;
        static FileMonitor fileMonitor;
        static OOTSaveFile save;
        static MemoryMap map;
        #endregion


        static void Main(string[] args)
        {
            Console.WriteLine("OOT Auto Tracker:");

            savePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/RetroArch/Saves";
            fileMonitor = new FileMonitor(savePath);
            Console.WriteLine("File Monitor Initialized");
            
            Console.WriteLine("Please wait while chest file is loaded");

            map = new MemoryMap();
            map.LoadItems();

            Console.WriteLine("Chest File Loaded");
            Console.WriteLine("");
            Console.WriteLine("Enter a command, or enter help for help");
            Console.WriteLine("If you are using this software to track your game, use the autotrack command");

            ConsoleLoop();

            fileMonitor.Dispose();
        }


        #region Program Loops
        private static void ConsoleLoop()
        {
            //enter command line
            while(true)
            {
                save = fileMonitor.SaveFile;

                Console.Write("-$:");
                string command = Console.ReadLine();
                if(command == "exit" || command == "quit" || command == "q")
                    return;

                //print scene data flag
                if(command.StartsWith("print "))
                {
                    PrintScene(command);
                }
                //Check if a scene is complete, and print that.
                if(command.StartsWith("check "))
                {
                    CheckScene(command);
                }
                //Check all items and print them.
                if(command == "checkAll")
                {
                    CheckAll(command);
                }
                //Print the contents of the memory map for debugging
                if(command == "printMap")
                {
                    PrintMap(command);
                }
                //Print the file path for the currently active file
                if(command == "printFilepath" || command == "printFile" || command == "printActive")
                {
                    PrintActiveFile(command);
                }
                //Find a scene by name
                if(command.StartsWith("find "))
                {
                    FindScene(command);
                }
                //Execute auto tracking
                if(command == "autotrack")
                {
                    AutoTrack();
                }
                //Clear the command console
                if(command == "cls")
                {
                    Console.Clear();
                }
                //Print the event list for debugging
                if(command == "events")
                {
                    PrintEvents(command);
                }
                if(command == "help")
                {
                    PrintCommands(command);
                }
            }
        }
        private static void AutoTrack()
        {
            //Trackers to remember auto track state
            bool printChests = true, printEvents = true;

            //unassign save to get it to rescan automatically
            save = null;


            while(true)
            {
                //check if save needs to be reloaded
                bool reload = fileMonitor.SaveFile != save;

                //reload logic
                if(reload)
                {
                    save = fileMonitor.SaveFile;

                    Console.Clear();
                    Console.WriteLine(save.naviCounter.ToString("X4"));
                    if(printChests)
                        PrintChests();
                    if(printEvents)
                        PrintEvents();
                    Console.WriteLine("Auto Track Engaged:");
                    Console.WriteLine("Q: quit");
                    Console.WriteLine("C: toggle chests");
                    Console.WriteLine("E: Toggle events");
                }
                //exits the loop of the console key is detected
                if(Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = System.Console.ReadKey(true);
                    if(key.Key == ConsoleKey.Q)
                    {
                        return;
                    }
                    //Toggle chests
                    if(key.Key == ConsoleKey.C)
                    {
                        printChests = !printChests;
                        save = null;
                    }
                    //Toggle events
                    if(key.Key == ConsoleKey.E)
                    {
                        printEvents = !printEvents;
                        save = null;
                    }
                }
                Thread.Sleep(50);
            }
        }
        public static void PrintChests()
        {
            foreach(var scene in save.scenes)
            {
                string[] chests;
                int collCount;
                bool sceneCheck = scene.CheckScene(map, out chests, out collCount);
                if(chests.Length > 0)
                {
                    if(sceneCheck)
                    {
                        Console.WriteLine("[X] " + scene.index.ToString());
                    }
                    else if(collCount == 0)
                    {
                        Console.WriteLine("[ ] " + scene.index.ToString());
                    }
                    else
                    {
                        Console.WriteLine("[-] " + scene.index.ToString());
                        foreach(var chest in chests)
                        {
                            Console.WriteLine("\t" + chest);
                        }
                    }
                }
            }
        }
        public static void PrintEvents()
        {
            Console.WriteLine("Events");
            foreach(var e in map.events)
            {
                if(e.Check(save.event_chk_table, save.item_get_table, save.inf_table))
                {
                    Console.WriteLine("\t[X] " + e.ToString());
                }
                else
                {
                    Console.WriteLine("\t[ ] " + e.ToString());
                }
            }
        }
        
        #endregion


        #region Console Commands
        private static void PrintScene(string command)
        {
            string pos = command.Remove(0, "print ".Length);
            try
            {
                int index = int.Parse(pos);
                SceneData scene = save.scenes[index];
                Console.WriteLine("\t" + scene.index.ToString());
                Console.WriteLine("\t " + scene.GetChestWord().ToString("X8"));
                Console.WriteLine("\t " + scene.GetCollectableWord().ToString("X8"));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void CheckScene(string command)
        {
            string pos = command.Remove(0, "check ".Length);
            try
            {
                int index = int.Parse(pos);
                SceneData scene = save.scenes[index];
                string[] chests;
                int collCount;
                bool check = scene.CheckScene(map, out chests, out collCount);
                Console.WriteLine(scene.index + ": " + check.ToString());
                foreach(var chest in chests)
                {
                    Console.WriteLine("\t" + chest);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void CheckAll(string command)
        {
            foreach(var item in map.items)
            {
                SceneIndex sceneIndex = item.sceneIndex;
                SceneData scene = save.scenes[(int)sceneIndex];
                string check = "\t[ ]";
                if(item.CheckWord(scene.GetChestWord(), scene.GetCollectableWord()))
                {
                    check = "\t[x]";
                }
                check += " " + item.ToString();
                Console.WriteLine(check);
            }
        }
        private static void PrintMap(string command)
        {
            foreach(var item in map.items)
            {
                Console.WriteLine("\t" + item.ToString());
            }
        }
        private static void PrintActiveFile(string command)
        {
            Console.WriteLine("\tLast write: " + fileMonitor.LastAccessTime + ":" + fileMonitor.ActiveFile);
        }
        private static void FindScene(string command)
        {
            command = command.Remove(0, "find ".Length);
            Regex regex = new Regex(command, RegexOptions.IgnoreCase);
            foreach(var sceneIndex in Enum.GetValues(typeof(SceneIndex)))
            {
                int num = (int)sceneIndex;
                string name = sceneIndex.ToString();
                if(regex.IsMatch(name))
                {
                    Console.WriteLine("\t" + num + ": " + sceneIndex.ToString());
                }
            }
        }
        private static void PrintEvents(string command)
        {
            foreach(var s in save.event_chk_table)
            {
                string temp = Convert.ToString(s, 2);
                while(temp.Length < 8)
                {
                    temp = "0" + temp;
                }
                Console.WriteLine(temp);
            }
            foreach(var e in map.events)
            {
                bool check = save.GetEvent(e);
                string output = "\t[ ] ";
                if(check)
                {
                    output = "\t[X] ";
                }
                Console.WriteLine(output + e.ToString());
            }
        }

        public static void PrintCommands(string command)
        {
            Console.WriteLine("\texit: exit the program");
            Console.WriteLine("\tquit: exit the program");
            Console.WriteLine("\tq: exit the program");
            Console.WriteLine("\tprint [sceneIndex]: Print the data at the specified scene index");
            Console.WriteLine("\tcheck [sceneIndex]: Prints if the specified scene is complete or not");
            Console.WriteLine("\tcheckAll: Prints the status of all chests");
            Console.WriteLine("\tprintMap: prints the memory map for the save file");
            Console.WriteLine("\tprintFilepath: print the filepath to the current active file");
            Console.WriteLine("\tprintFile: print the filepath to the current active file");
            Console.WriteLine("\tprintActive: print the filepath to the current active file");
            Console.WriteLine("\tfind [search pattern]: prints the scene index of all scenes that match the pattern");
            Console.WriteLine("\t\tfind . will print all scenes");
            Console.WriteLine("\tautotrack: begin auto tracking, and auto updating");
            Console.WriteLine("\tcls: clear the console screen");
            Console.WriteLine("\tevents: print a list of events");
            Console.WriteLine("\thelp: print the help string, with a list of commands");
        }
        #endregion


        #region Utility
        private static OOTSaveFile LoadFile(string path)
        {
            byte[] arr = File.ReadAllBytes(path);

            //get section of file with save bytes
            byte[] saveBytes = new byte[SAVE_FILE_SIZE];
            Array.Copy(arr, SAVE_FILE_HEAD, saveBytes, 0, SAVE_FILE_SIZE);

            //print the sve file in big edian, to a debug file
            PrintOutput(saveBytes);

            //Generate a save file from the output bytes.
            var output = new OOTSaveFile(saveBytes);
            return output;
        }

        private static void PrintOutput(byte[] saveFileBytes)
        {
            byte[] cp = saveFileBytes.Clone() as byte[];
            for(int i = 0; i < cp.Length; i += 4)
            {
                Swap(ref cp[i], ref cp[i + 3]);
                Swap(ref cp[i + 1], ref cp[i + 2]);
            }
            File.WriteAllBytes("output.srm", cp);
        }
        private static void Swap(ref byte a, ref byte b)
        {
            byte temp = a;
            a = b;
            b = temp;
        }
        #endregion

        public static void SwapEdian(byte[] array)
        {
            for(int i = 0; i < array.Length; i += 4)
            {
                SwapBytes(ref array[i], ref array[i + 3]);
                SwapBytes(ref array[i + 1], ref array[i + 2]);
            }
        }

        public static void SwapBytes(ref byte a, ref byte b)
        {
            byte c = a;
            a = b;
            b = c;
        }
    }
}