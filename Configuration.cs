using System;
using System.Xml.Serialization;

namespace Trainer.net
{
    [Serializable]
    public class Configuration
    {
        public string GameCode { get; set; }

        [XmlIgnore]
        public UInt32 TrainerPointer { get; set; }

        [XmlIgnore]
        public UInt32 SpritePointer { get; set; }

        [XmlIgnore]
        public UInt32 PalettePointer { get; set; }

        [XmlIgnore]
        public UInt32 PokemonNamePointer { get; set; }

        [XmlIgnore]
        public UInt32 TrainerClassPointer { get; set; }

        [XmlIgnore]
        public UInt32 ItemNamePointer { get; set; }
       
        [XmlIgnore]
        public UInt32 TrainerClassNamePointer { get; set; }

        [XmlIgnore]
        public UInt32 AttackNamePointer { get; set; }

        [XmlIgnore]
        public UInt32 PokemonSpritePointer { get; set; }

        [XmlIgnore]
        public UInt32 PokemonPalettePointer { get; set; }

        [XmlElement(ElementName = "TrainerPointer")]
        public string HexValue
        {
            get
            {
                // convert int to hex representation
                return "0x" + TrainerPointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                TrainerPointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "PokemonNamePointer")]
        public string HexValue2
        {
            get
            {
                // convert int to hex representation
                return "0x" + PokemonNamePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                PokemonNamePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "TrainerClassPointer")]
        public string HexValue3
        {
            get
            {
                // convert int to hex representation
                return "0x" + TrainerClassPointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                TrainerClassPointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "ItemNamePointer")]
        public string HexValue4
        {
            get
            {
                // convert int to hex representation
                return "0x" + ItemNamePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                ItemNamePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "TrainerClassNamePointer")]
        public string HexValue5
        {
            get
            {
                // convert int to hex representation
                return "0x" + TrainerClassNamePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                TrainerClassNamePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "SpritePointer")]
        public string HexValue6
        {
            get
            {
                // convert int to hex representation
                return "0x" + SpritePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                SpritePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "PalettePointer")]
        public string HexValue7
        {
            get
            {
                // convert int to hex representation
                return "0x" + PalettePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                PalettePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "AttackNamePointer")]
        public string HexValue8
        {
            get
            {
                // convert int to hex representation
                return "0x" + AttackNamePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                AttackNamePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "PokemonSpritePointer")]
        public string HexValue9
        {
            get
            {
                // convert int to hex representation
                return "0x" + PokemonSpritePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                PokemonSpritePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        [XmlElement(ElementName = "PokemonPalettePointer")]
        public string HexValue10
        {
            get
            {
                // convert int to hex representation
                return "0x" + PokemonPalettePointer.ToString("x");
            }
            set
            {
                // convert hex representation back to int
                PokemonPalettePointer = uint.Parse(value.Remove(0, 2),
                    System.Globalization.NumberStyles.HexNumber);
            }
        }

        public int TrainerCount { get; set; }

        public int ItemCount { get; set; }

        public int SpriteCount { get; set; }

        public int PokemonCount { get; set; }

        public int AttackCount { get; set; }
    }
}
