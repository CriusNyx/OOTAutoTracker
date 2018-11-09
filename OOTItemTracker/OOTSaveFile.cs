using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OOTItemTracker.Program;

namespace OOTItemTracker
{
    /// <summary>
    /// Get the bytes from an OOT save file, and sort generate some auxilary data structures to make analysis easier.
    /// </summary>
    public class OOTSaveFile
    {
        public readonly byte[] bytes;
        public readonly ushort naviCounter;

        public readonly SceneData[] scenes = new SceneData[SCENE_COUNT];

        public readonly byte[] event_chk_table = new byte[EVENT_CHECK_LENGTH];
        public readonly byte[] item_get_table = new byte[ITEM_GET_TABLE_LENGTH];
        public readonly byte[] inf_table = new byte[INF_LENGTH];

        

        public OOTSaveFile(byte[] bytes)
        {
            this.bytes = bytes;

            naviCounter = BitConverter.ToUInt16(bytes, NAVI_ADDRESS);

            //read scenes
            for(int i = 0; i < SCENE_COUNT; i++)
            {
                SceneIndex index = (SceneIndex)i;
                scenes[i] = new SceneData(index, bytes);
            }

            Array.Copy(bytes, EVENT_CHECK_ADDRESS, event_chk_table, 0, EVENT_CHECK_LENGTH);
            SwapEdian(event_chk_table);

            Array.Copy(bytes, ITEM_GET_ADDRESS, item_get_table, 0, ITEM_GET_TABLE_LENGTH);
            SwapEdian(item_get_table);

            Array.Copy(bytes, INF_ADDRESS, inf_table, 0, INF_LENGTH);
            SwapEdian(inf_table);
        }

        public uint GetWord(int index)
        {
            return BitConverter.ToUInt32(bytes, index);
        }

        public bool GetEvent(OOTEvent e)
        {
            return e.Check(event_chk_table, item_get_table, inf_table);
        }
    }
}
