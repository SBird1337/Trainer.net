using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Single.Core;
using Single.Core.Text;

namespace Trainer.net.Library
{
    public class TrainerEntry : IRomWritable
    {
        private readonly HexEncoder _encoder;
        private byte _music;
        private string _name;

        public TrainerEntry(HexEncoder encoder, Rom rom)
        {
            _encoder = encoder;

            byte dataStructure = rom.ReadByte();
            UsesCustomMoves = Convert.ToBoolean(dataStructure & 1);
            UsesCustomItems = Convert.ToBoolean(dataStructure & 2);
            TrainerClass = rom.ReadByte();
            byte genderMusic = rom.ReadByte();
            IsFemale = Convert.ToBoolean(genderMusic & 128);
            _music = (byte) (genderMusic & 127);
            Sprite = rom.ReadByte();
            Name = encoder.GetParsedString(RomStringHelper.ReadRomString(rom));
            rom.SetStreamOffset(rom.CurrentPosition + (12 - Name.Length - 1));
            ItemOne = rom.ReadUInt16();
            ItemTwo = rom.ReadUInt16();
            ItemThree = rom.ReadUInt16();
            ItemFour = rom.ReadUInt16();
            UnknownOne = rom.ReadUInt32();
            UnknownTwo = rom.ReadUInt32();
            PokeCount = rom.ReadByte();
            PaddingOne = rom.ReadByte();
            PaddingTwo = rom.ReadByte();
            PaddingThree = rom.ReadByte();
            PokemonData = new PokemonEntry(rom.ReadUInt32() & 0x1FFFFFF, this);
        }

        public bool UsesCustomItems { get; set; }
        public bool UsesCustomMoves { get; set; }
        public byte TrainerClass { get; set; }
        public bool IsFemale { get; set; }

        public byte Music
        {
            get { return _music; }
            set
            {
                if (value > 127)
                    throw new ArgumentOutOfRangeException();
                _music = value;
            }
        }

        public byte Sprite { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Length > 11)
                    throw new ArgumentOutOfRangeException();
                _name = value;
            }
        }

        public UInt16 ItemOne { get; set; }

        public UInt16 ItemTwo { get; set; }

        public UInt16 ItemThree { get; set; }

        public UInt16 ItemFour { get; set; }

        public UInt32 UnknownOne { get; set; }

        public UInt32 UnknownTwo { get; set; }

        public byte PokeCount { get; set; }

        public byte PaddingOne { get; set; }

        public byte PaddingTwo { get; set; }

        public byte PaddingThree { get; set; }

        public PokemonEntry PokemonData { get; set; }

        public byte[] GetRawData()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                var dataStructure =
                    (byte) (0 & ((Convert.ToByte(UsesCustomItems) << 1) & (Convert.ToByte(UsesCustomMoves))));
                writer.Write(dataStructure);
                writer.Write(TrainerClass);
                writer.Write((byte) (Music & (Convert.ToByte(IsFemale) << 7)));
                writer.Write(Sprite);
                writer.Write(BuildName());
                writer.Write(ItemOne);
                writer.Write(ItemTwo);
                writer.Write(ItemThree);
                writer.Write(ItemFour);
                writer.Write(UnknownOne);
                writer.Write(UnknownTwo);
                writer.Write(PokeCount);
                writer.Write(PaddingOne);
                writer.Write(PaddingTwo);
                writer.Write(PaddingThree);
                writer.Write(PokemonData.Position | 0x08000000);
                return ms.ToArray();
            }
        }

        private byte[] BuildName()
        {
            List<byte> output = _encoder.GetParsedBytes(Name).ToList();
            output.Add(0xFF);
            for (int i = 0; i < 12 - _name.Length - 1; ++i)
                output.Add(0);
            return output.ToArray();
        }
    }
}