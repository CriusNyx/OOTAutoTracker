using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOTItemTracker
{
    /// <summary>
    /// Represents an item that can be aquired from an NPC, or by completing an event.
    /// </summary>
    public class OOTEvent
    {
        public readonly Table table;
        public readonly string name;
        public readonly int bitNum, byteNum;

        public OOTEvent(Table table, string name, int byteNum, int bitNum)
        {
            this.table = table;
            this.name = name;
            this.bitNum = bitNum;
            this.byteNum = byteNum;
        }

        public override string ToString()
        {
            return name + "(" + byteNum + ", " + bitNum + ")";
        }

        public bool Check(byte[] eventTable, byte[] itemGetTable, byte[] infTable)
        {
            byte[] temp = null;
            switch(table)
            {
                case Table.EventTable:
                    temp = eventTable;
                    break;
                case Table.ItemGetTable:
                    temp = itemGetTable;
                    break;
                case Table.INFTable:
                    temp = infTable;
                    break;
            }
            return ((1 << bitNum) & temp[byteNum]) != 0;
        }

        public enum Table
        {
            EventTable,
            ItemGetTable,
            INFTable
        }
    }
}
