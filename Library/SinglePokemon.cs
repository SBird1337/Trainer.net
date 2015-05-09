using System.IO;
using Single.Core;

namespace Trainer.net.Library
{
    public class SinglePokemon : IRomWritable
    {
        private ushort _item;
        private ushort _attack1;
        private ushort _attack2;
        private ushort _attack3;
        private ushort _attack4;

        private readonly TrainerEntry _trainerBase;

        public static SinglePokemon BlankPokemon(TrainerEntry entry)
        {
            return new SinglePokemon(0, 0, 0, 0, 0, 0, 0, 0, entry);
        }

        public SinglePokemon(byte ai, byte level, ushort species, ushort item, ushort attack1, ushort attack2,
            ushort attack3, ushort attack4, TrainerEntry trainerBase)
        {
            _trainerBase = trainerBase;
            AiLevel = ai;
            Level = level;
            Species = species;
            Item = item;
            Attack1 = attack1;
            Attack2 = attack2;
            Attack3 = attack3;
            Attack4 = attack4;
        }

        public SinglePokemon(Rom input, TrainerEntry trainerBase)
        {
            _trainerBase = trainerBase;
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
            if ((!trainerBase.UsesCustomItems))
                input.ReadUInt16();
        }

        public byte[] GetRawData()
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            writer.Write(AiLevel);
            writer.Write((byte)0);
            writer.Write(Level);
            writer.Write((byte)0);
            writer.Write(Species);
            if (_trainerBase.UsesCustomItems)
                writer.Write(Item);
            if (_trainerBase.UsesCustomMoves)
            {
                writer.Write(Attack1);
                writer.Write(Attack2);
                writer.Write(Attack3);
                writer.Write(Attack4);
            }
            if ((!_trainerBase.UsesCustomItems))
                writer.Write((ushort)0x0000);       //NOTE: This is a padding and could be any value, however 0000 seemed convenient
            return ms.ToArray();
        }

        public byte AiLevel { get; set; }
        public byte Level { get; set; }
        public ushort Species { get; set; }
        public ushort Item
        {
            get { return _item; }
            set
            {
                _item = value;
                ReCreateTrainer();
            }
        }
        public ushort Attack1
        {
            get { return _attack1; }
            set
            {
                _attack1 = value;
                ReCreateTrainer();
            }
        }
        public ushort Attack2
        {
            get { return _attack2; }
            set
            {
                _attack2 = value;
                ReCreateTrainer();
            }
        }
        public ushort Attack3
        {
            get { return _attack3; }
            set
            {
                _attack3 = value;
                ReCreateTrainer();
            }
        }
        public ushort Attack4
        {
            get { return _attack4; }
            set
            {
                _attack4 = value;
                ReCreateTrainer();
            }
        }

        private void ReCreateTrainer()
        {
            if (_trainerBase.PokemonData != null)
            {
                _trainerBase.UsesCustomItems = false;
                _trainerBase.UsesCustomMoves = false;
                foreach (SinglePokemon pkmn in _trainerBase.PokemonData.Entries)
                {
                    if (pkmn.Attack1 != 0 || pkmn.Attack2 != 0 || pkmn.Attack3 != 0 || pkmn.Attack4 != 0)
                        _trainerBase.UsesCustomMoves = true;
                    if (pkmn.Item != 0)
                        _trainerBase.UsesCustomItems = true;
                }
            }
        }
    }
}
