using System.IO;
using Single.Core;

namespace Trainer.net.Library
{
    public class SinglePokemon : IRomWritable
    {
        public SinglePokemon(Rom input, TrainerEntry trainerBase)
        {
            AiLevel = input.ReadByte();
            input.ReadByte();
            Level = input.ReadByte();
            input.ReadByte();
            Species = input.ReadUInt16();
            if (trainerBase.UsesCustomItems)
                Item = input.ReadUInt16();
            if (trainerBase.UsesCustomMoves)
            {
                Attack1 = input.ReadUInt16();
                Attack2 = input.ReadUInt16();
                Attack3 = input.ReadUInt16();
                Attack4 = input.ReadUInt16();
            }
            input.ReadUInt16();
        }

        public byte[] GetRawData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(AiLevel);
            writer.Write((byte)0);
            writer.Write(Level);
            writer.Write((byte)0);
            writer.Write(Species);
            if(Item != 0)
                writer.Write(Item);
            if (Attack1 != 0 && Attack2 != 0 && Attack3 != 0 && Attack4 != 0)
            {
                writer.Write(Attack1);
                writer.Write(Attack2);
                writer.Write(Attack3);
                writer.Write(Attack4);
            }
            return ms.ToArray();
        }

        public byte AiLevel { get; set; }
        public byte Level { get; set; }
        public ushort Species { get; set; }
        public ushort Item { get; set; }
        public ushort Attack1 { get; set; }
        public ushort Attack2 { get; set; }
        public ushort Attack3 { get; set; }
        public ushort Attack4 { get; set; }
    }
}
