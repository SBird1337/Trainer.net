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
            Rom = rom;
            Position = (uint)rom.CurrentPosition;
            byte dataStructure = rom.ReadByte();
            UsesCustomMoves = Convert.ToBoolean(dataStructure & 1);
            UsesCustomItems = Convert.ToBoolean(dataStructure & 2);
            TrainerClass = rom.ReadByte();
            byte genderMusic = rom.ReadByte();
            IsFemale = Convert.ToBoolean(genderMusic & 128);
            _music = (byte) (genderMusic & 127);
            Sprite = rom.ReadByte();
            byte[] sequence = RomStringHelper.ReadRomString(rom);
            Name = encoder.GetParsedString(sequence);
            rom.SetStreamOffset(rom.CurrentPosition + (12 - sequence.Length - 1));
            ItemOne = rom.ReadUInt16();
            ItemTwo = rom.ReadUInt16();
            ItemThree = rom.ReadUInt16();
            ItemFour = rom.ReadUInt16();
            DualBattle = Convert.ToBoolean(rom.ReadByte());
            rom.SetStreamOffset(rom.CurrentPosition + 3);

            AiValue = rom.ReadUInt16();

            rom.SetStreamOffset(rom.CurrentPosition + 2);

            PokeCount = rom.ReadByte();
            rom.SetStreamOffset(rom.CurrentPosition + 3);

            uint pos = rom.ReadUInt32() & 0x1FFFFFF;
            long cpos = rom.CurrentPosition;
            if(pos > 0)
                PokemonData = new PokemonEntry(pos, this);
            rom.SetStreamOffset(cpos);
            
        }

        public bool UsesCustomItems { get; set; }
        public bool UsesCustomMoves { get; set; }
        public byte TrainerClass { get; set; }
        public bool IsFemale { get; set; }
        public UInt32 Position { get; set; }

        public Rom Rom { get; set; }

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

        public bool RequiresRepoint { get; set; }

        public ushort ItemOne { get; set; }

        public ushort ItemTwo { get; set; }

        public ushort ItemThree { get; set; }

        public ushort ItemFour { get; set; }

        public bool DualBattle { get; set; }

        public ushort AiValue { get; set; }

        public byte PokeCount { get; set; }

        public PokemonEntry PokemonData { get; set; }

        public byte[] GetRawData()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                var dataStructure =
                    (byte) (0 | ((Convert.ToByte(UsesCustomItems) << 1) | (Convert.ToByte(UsesCustomMoves))));
                writer.Write(dataStructure);
                writer.Write(TrainerClass);
                writer.Write((byte) (Music | (Convert.ToByte(IsFemale) << 7)));
                writer.Write(Sprite);
                writer.Write(BuildName());
                writer.Write(ItemOne);
                writer.Write(ItemTwo);
                writer.Write(ItemThree);
                writer.Write(ItemFour);
                writer.Write(Convert.ToByte(DualBattle));
                writer.Write(new byte[] { 0, 0, 0 });
                writer.Write(AiValue);
                writer.Write(new byte[] {0,0});
                writer.Write(PokeCount);
                writer.Write(new byte[] { 0, 0, 0 });
                if(PokemonData != null)
                    writer.Write(PokemonData.Position | 0x08000000);
                else
                    writer.Write((uint)0);
                return ms.ToArray();
            }
        }

        private byte[] BuildName()
        {
            List<byte> output = _encoder.GetParsedBytes(Name).ToList();
            output.Add(0xFF);
            while(output.Count() < 12)
                output.Add(0);
            return output.ToArray();
        }
    }
}