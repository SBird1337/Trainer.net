using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Single.Core;
using Single.Core.Text;
using Single.Graphics;

namespace Trainer.net.Library
{
    public class StaticElements
    {
        public StaticElements(Configuration config, Rom r, HexEncoder encoder)
        {
            ItemNames = new List<string> {"---"};
            r.SetStreamOffset(config.ItemNamePointer);
            r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);
            r.SetStreamOffset(r.CurrentPosition + 0x2C);
            for (int i = 0; i < config.ItemCount; ++i)
            {
                ItemNames.Add(encoder.GetParsedString(RomStringHelper.ReadRomString(r)));
                r.SetStreamOffset(r.CurrentPosition + (43 - ItemNames.Last().Length));
            }
            var spritePointers = new List<uint>();
            var palPointers = new List<uint>();
            Sprites = new List<Image>();
            r.SetStreamOffset(config.SpritePointer);
            r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);
            for (int i = 0; i < config.SpriteCount; ++i)
            {
                spritePointers.Add(r.ReadUInt32() & 0x1FFFFFF);
                r.ReadUInt32();
            }
            r.SetStreamOffset(config.PalettePointer);
            r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);
            for (int i = 0; i < config.SpriteCount; ++i)
            {
                palPointers.Add(r.ReadUInt32() & 0x1FFFFFF);
                r.ReadUInt32();
            }
            for (int i = 0; i < config.SpriteCount; ++i)
            {
                Sprites.Add(Tileset.FromCompressedAddress(r, spritePointers[i])
                    .ToBitmap(8, new Palette(r, palPointers[i], true)));
            }

            spritePointers.Clear();
            palPointers.Clear();

            PokeSprites = new List<Image>();

            r.SetStreamOffset(config.PokemonSpritePointer);
            r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);
            for (int i = 0; i < config.PokemonCount; ++i)
            {
                spritePointers.Add(r.ReadUInt32() & 0x1FFFFFF);
                r.ReadUInt32();
            }

            r.SetStreamOffset(config.PokemonPalettePointer);
            r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);

            for (int i = 0; i < config.PokemonCount; ++i)
            {
                palPointers.Add(r.ReadUInt32() & 0x1FFFFFF);
                r.ReadUInt32();
            }

            for (int i = 0; i < config.PokemonCount; ++i)
            {
                PokeSprites.Add(Tileset.FromCompressedAddress(r, spritePointers[i])
                    .ToBitmap(8, new Palette(r, palPointers[i], true)));
            }

            PokemonNames = new List<string>();

            r.SetStreamOffset(config.PokemonNamePointer);
            r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);

            for (int i = 0; i < config.PokemonCount; ++i)
            {
                PokemonNames.Add(encoder.GetParsedString(RomStringHelper.ReadRomString(r)));
                r.SetStreamOffset(r.CurrentPosition + 10 - PokemonNames.Last().Count());
            }

            AttackNames = new List<string>();

            r.SetStreamOffset(config.AttackNamePointer);
            r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);

            for (int i = 0; i < config.AttackCount; ++i)
            {
                AttackNames.Add(encoder.GetParsedString(RomStringHelper.ReadRomString(r)));
                r.SetStreamOffset(r.CurrentPosition + 12 - AttackNames.Last().Count());
            }
        }

        public List<string> ItemNames { get; set; }
        public List<string> PokemonNames { get; set; }
        public List<string> AttackNames { get; set; }
        public List<Image> Sprites { get; set; }
        public List<Image> PokeSprites { get; set; }
    }
}