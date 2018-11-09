using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OOTItemTracker
{
    /// <summary>
    /// Represents an item that can be aquired by opening a chest, or collecting a heart.
    /// </summary>
    public class OOTItem
    {
        public readonly SceneIndex sceneIndex;
        public readonly string line;
        public readonly string collectableType;
        public readonly string containingItem;
        public readonly int flagIndex;

        public OOTItem(SceneIndex sceneIndex, string line, string collectableType, string containingItem, int flagIndex)
        {
            this.sceneIndex = sceneIndex;
            this.line = line;
            this.collectableType = collectableType;
            this.containingItem = containingItem;
            this.flagIndex = flagIndex;
        }

        public static OOTItem Parse(string line)
        {
            int sceneIndex = int.Parse(line.Split(':')[0]);

            if(new Regex("[0-9|A-F][0-9|A-F][0-9|A-F][0-9|A-F]:[0-9|A-F][0-9|A-F][0-9|A-F][0-9|A-F] Chest, ")
                .IsMatch(line))
            {
                return ParseChest(sceneIndex, line);
            }
            if(new Regex("[0-9|A-F][0-9|A-F][0-9|A-F][0-9|A-F]:[0-9|A-F][0-9|A-F][0-9|A-F][0-9|A-F] Collectable Heart Piece")
                .IsMatch(line))
            {
                return ParseCollectable(sceneIndex, line);
            }
            return null;
        }

        private static OOTItem ParseChest(int sceneIndex, string line)
        {
            string item = 
                new Regex(
                    "reward [0-9|A-F]*: (.*?),"
                    )
                    .Match(line)
                    .Groups[1]
                    .Value;

            string flagString = new Regex("chest flag: (.*?),").Match(line).Groups[1].Value;
            int flagIndex = int.Parse(flagString, System.Globalization.NumberStyles.HexNumber);

            return new OOTItem((SceneIndex)sceneIndex, line, "Chest", item, flagIndex);
        }

        private static OOTItem ParseCollectable(int sceneIndex, string line)
        {
            string collectableType = 
                new Regex(
                    "[0-9|A-F][0-9|A-F][0-9|A-F][0-9|A-F]:[0-9|A-F][0-9|A-F][0-9|A-F][0-9|A-F] (.*?),"
                    )
                    .Match(line)
                    .Groups[1]
                    .Value;

            string temp = new Regex("Perm: (.*?),").Match(line).Groups[1].Value;
            int flagIndex = int.Parse(new Regex("[Temp|Perm]: (.*?),").Match(line).Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

            return new OOTItem((SceneIndex)sceneIndex, line, collectableType, "", flagIndex);
        }

        public override string ToString()
        {
            return collectableType + ": { SceneIndex: " + sceneIndex + " Type: " + collectableType + " Item: " + containingItem + " Index: " + flagIndex + " }";
        }

        public bool CheckWord(uint chestWord, uint collWord)
        {
            if(collectableType == "Chest")
            {
                return ((1u << flagIndex) & chestWord) != 0;
            }
            return ((1u << flagIndex) & collWord) != 0;
        }
    }
}