using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Single.Core;

namespace Trainer.net.Library
{


    public class MoneyData : IRomWritable
    {
        public Dictionary<byte, byte> MoneyValues { get; set; }
        public byte LastValue { get; set; }

        public MoneyData(Configuration config, Rom rom)
        {
            rom.SetStreamOffset(config.TrainerClassPointer);
            rom.SetStreamOffset(rom.ReadUInt32() & 0x1FFFFFF);
            byte currentId = rom.ReadByte();
            MoneyValues = new Dictionary<byte, byte>();
            while (currentId != 0xFF)
            {
                MoneyValues.Add(currentId, rom.ReadByte());
                rom.ReadUInt16();
                currentId = rom.ReadByte();
            }
            LastValue = rom.ReadByte();
        }

        public byte[] GetRawData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter br = new BinaryWriter(ms);
            foreach (byte id in MoneyValues.Keys)
            {
                if (id < 0x3A)
                {
                    br.Write(id);
                    br.Write(MoneyValues[id]);
                    br.Write((ushort)0);
                }
            }
            br.Write(0xFF);
            br.Write(LastValue);
            return ms.ToArray();
        }
    }
}
