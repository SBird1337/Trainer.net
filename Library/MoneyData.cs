using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Single.Core;

namespace Trainer.net.Library
{
    public class MoneyData : IRomWritable
    {
        private readonly byte _lastValue;
        private readonly Dictionary<byte, byte> _moneyValues;

        public byte this[byte index]
        {
            get
            {
                return _moneyValues.ContainsKey(index) ? _moneyValues[index] : _lastValue;
            }
            set
            {
                if (_moneyValues.ContainsKey(index))
                    _moneyValues[index] = value;
                else
                    _moneyValues.Add(index, value);
            }
        }

        public MoneyData(Configuration config, Rom rom)
        {
            rom.SetStreamOffset(config.TrainerClassPointer);
            rom.SetStreamOffset(rom.ReadUInt32() & 0x1FFFFFF);
            byte currentId = rom.ReadByte();
            _moneyValues = new Dictionary<byte, byte>();
            while (currentId != 0xFF)
            {
                _moneyValues.Add(currentId, rom.ReadByte());
                rom.ReadUInt16();
                currentId = rom.ReadByte();
            }
            _lastValue = rom.ReadByte();
        }

        

        public byte[] GetRawData()
        {
            var ms = new MemoryStream();
            var br = new BinaryWriter(ms);
            foreach (byte id in _moneyValues.Keys)
            {
                br.Write(id);
                br.Write(_moneyValues[id]);
                br.Write((ushort)0);
            }
            br.Write((byte)0xFF);
            br.Write(_lastValue);
            return ms.ToArray();
        }
    }
}