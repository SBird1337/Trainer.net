using System.Collections.Generic;
using System.IO;
using Single.Core;

namespace Trainer.net.Library
{
    public class MoneyData : IRepointable
    {
        private readonly byte _lastValue;
        private readonly Dictionary<byte, byte> _moneyValues;
        private int _originalSize;
        private uint _currentOffset;

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
            _currentOffset = rom.ReadUInt32() & 0x1FFFFFF;
            rom.SetStreamOffset(_currentOffset);
            byte currentId = rom.ReadByte();
            _moneyValues = new Dictionary<byte, byte>();
            while (currentId != 0xFF)
            {
                _moneyValues.Add(currentId, rom.ReadByte());
                rom.ReadUInt16();
                currentId = rom.ReadByte();
            }
            _lastValue = rom.ReadByte();
            _originalSize = GetSize();
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

        public int GetSize()
        {
            return GetRawData().Length;
        }

        public uint GetCurrentOffset()
        {
            return _currentOffset;
        }

        public void SetCurrentOffset(uint newOffset)
        {
            _currentOffset = newOffset;
            _originalSize = GetSize();
        }

        public int GetOriginalSize()
        {
            return _originalSize;
        }
    }
}