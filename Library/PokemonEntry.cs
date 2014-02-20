using System;
using Single.Core;

namespace Trainer.net.Library
{
    public class PokemonEntry : IRepointable
    {
        private readonly int _originalSize;

        public PokemonEntry(UInt32 position, TrainerEntry trainerBase)
        {
            Position = position;
            TrainerBase = trainerBase;
            //_originalSize = trainerBase.GetRawData().Length;
        }

        public UInt32 Position { get; set; }

        public TrainerEntry TrainerBase { get; set; }

        public int GetSize()
        {
            return TrainerBase.GetRawData().Length;
        }

        public uint GetCurrentOffset()
        {
            return Position;
        }

        public void SetCurrentOffset(uint offset)
        {
            Position = offset;
        }

        public byte[] GetRawData()
        {
            throw new NotImplementedException(); //TODO Implement
        }

        public int GetOriginalSize()
        {
            return _originalSize;
        }
    }
}