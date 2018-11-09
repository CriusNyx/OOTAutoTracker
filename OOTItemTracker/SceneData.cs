using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OOTItemTracker.Program;

namespace OOTItemTracker
{
    /// <summary>
    /// Access the data of a scene.
    /// </summary>
    public class SceneData
    {
        public readonly SceneIndex index;
        public readonly byte[] sceneBytes;


        public SceneData(SceneIndex index, byte[] saveFile)
        {
            this.index = index;
            int startIndex = SCENE_DATA_HEAD + (int)index * SCENE_DATA_SIZE;
            sceneBytes = new byte[SCENE_DATA_SIZE];
            for(int i = 0; i < SCENE_DATA_SIZE; i++)
            {
                sceneBytes[i] = saveFile[startIndex + i];
            }
        }

        public uint GetChestWord()
        {
            return BitConverter.ToUInt32(sceneBytes, CHEST_WORD_HEAD);
        }
        public uint GetCollectableWord()
        {
            return BitConverter.ToUInt32(sceneBytes, COLLECTABLE_WORD_HEAD);
        }

        public bool CheckScene(MemoryMap map, out string[] chests, out int collCount)
        {
            List<string> chestList = new List<string>();
            bool output = true;
            collCount = 0;
            foreach(var item in map.itemsByScene[index])
            {
                if(item.CheckWord(GetChestWord(), GetCollectableWord()))
                {
                    chestList.Add("[X] " + item.ToString());
                    collCount++;
                }
                else
                {
                    chestList.Add("[ ] " + item.ToString());
                    output = false;
                }
            }
            chests = chestList.ToArray();
            return output;
        }
    }
}