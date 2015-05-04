using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trainer.net.Library
{
    public class ShortBitField
    {
        private readonly bool[] _bits = new bool[16];

        public bool this[int i]
        {
            get { return _bits[i]; }
            set { _bits[i] = value; }
        }

        public ShortBitField(ushort values)
        {
            for (int i = 0; i < _bits.Length; ++i)
            {
                _bits[i] = ((values >> i) & 1) > 0;
            }
        }

        public ShortBitField(bool[] values)
        {
            if (values.Length == _bits.Length)
            {
                values.CopyTo(_bits, 0);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public ushort ToUshort()
        {
            ushort output = 0;
            for (int i = 0; i < _bits.Length; ++i)
            {
                output |= (ushort) ((_bits[i] ? 1 : 0) << i);
            }
            return output;
        }
    }
}
