using System;
using System.Collections.Generic;
using Single.Core;

namespace Trainer.net.Library
{
    public class PokemonEntry : IRepointable
    {
        private int _originalSize;

        public List<SinglePokemon> Entries = new List<SinglePokemon>(); 

        public PokemonEntry(UInt32 position, TrainerEntry trainerBase)
        {
            Position = position;
            TrainerBase = trainerBase;
            trainerBase.Rom.SetStreamOffset(Position);

            for (int i = 0; i < trainerBase.PokeCount; ++i)
            {
                Entries.Add(new SinglePokemon(trainerBase.Rom, trainerBase));
            }

            _originalSize = GetSize();
        }

        public UInt32 Position { get; set; }

        public TrainerEntry TrainerBase { get; set; }

        public int GetSize()
        {
            return GetRawData().Length;
        }

        public uint GetCurrentOffset()
        {
            return Position;
        }

        public void SetCurrentOffset(uint newOffset)
        {
            Position = newOffset;
            _originalSize = GetSize();
        }

        public byte[] GetRawData()
        {
            var data = new List<byte>();
            Entries.ForEach(element => data.AddRange(element.GetRawData()));
            return data.ToArray();
        }

        public int GetOriginalSize()
        {
            return _originalSize;
        }
    }
}