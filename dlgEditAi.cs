using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Trainer.net.Library;


namespace Trainer.net
{
    public partial class DlgEditAi : Form
    {
        private readonly CheckBox[] _aiValues = new CheckBox[16];

        public ShortBitField Value
        {
            get
            {
                return new ShortBitField(_aiValues.Select(cb => cb.Checked).ToArray());
            }
            set
            {
                for (int i = 0; i < _aiValues.Length; ++i)
                    _aiValues[i].Checked = value[i];
            }
        }

        public DlgEditAi(ShortBitField values)
        {
            InitializeComponent();
            for (int i = 0; i < 16; ++i)
            {
                _aiValues[i] = new CheckBox
                {
                    Checked = values[i],
                    Location = new Point(12, 12 + (23*i)),
                    Text = "Strategy " + i
                };
            }
        }

        private void dlgEditAi_Load(object sender, EventArgs e)
        {
            Controls.AddRange(_aiValues.Select(cb => cb as System.Windows.Forms.Control).ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        
        
    }
}
