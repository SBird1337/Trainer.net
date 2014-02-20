using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Single.Core;
using Single.Core.Text;
using Trainer.net.Library;
using Trainer.net.Properties;

namespace Trainer.net
{
    public partial class Form1 : Form
    {
        private const int TRAINER_CLASS_COUNT = 66; //TODO Move to Config File
        private readonly List<Configuration> _configurations = new List<Configuration>();
        private readonly List<TrainerEntry> _trainerEntries = new List<TrainerEntry>();
        private readonly BindingList<string> _trainerclassNames = new BindingList<string>();
        private Configuration _configuration;
        private int _currentDeepness;
        private TrainerEntry _currentEntry;
        private HexEncoder _encoder;
        private MoneyData _moneyData;
        private Rom _rom;
        private StaticElements _statics;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var deSerializer = new XmlSerializer(typeof (Configuration));
            if (!Directory.Exists(Constances.CONFIG_ROOT))
                Directory.CreateDirectory(Constances.CONFIG_ROOT);
            string[] files = Directory.GetFiles(Constances.CONFIG_ROOT);
            if (files.Length != 0)
            {
                foreach (string s in Directory.GetFiles(Constances.CONFIG_ROOT))
                {
                    try
                    {
                        _configurations.Add((Configuration) deSerializer.Deserialize(new FileStream(s, FileMode.Open)));
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show(
                            string.Format(
                                "Error while reading XML Configuration File, your file \"{0}\" is corrupted.", s),
                            Resources.Error_Global, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                    }
                }
                _encoder = new HexEncoder(new Table(Constances.TABLE_FILE), RomStringHelper.ParseNormal);
                //HexEncoder encoder = new HexEncoder(new Table(Constances.TABLE_FILE), RomStringHelper.ParseNormal);
                //Rom r = new Rom("sma.gba");
                //r.SetStreamOffset(0x3250A8);
                //var entry = new TrainerEntry(encoder, r);
                //MessageBox.Show(entry.Name);
                //MessageBox.Show(entry.PokemonData.Position.ToString("X"));
            }
            else
            {
                MessageBox.Show(Resources.No_Configuration_English, Resources.Error_Global, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
        }

        private void cTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(char.IsDigit(e.KeyChar) || (e.KeyChar.IsHexLetter())) && !char.IsControl(e.KeyChar);
        }

        private void UnloadAll()
        {
            grpTrainerSel.Enabled = false;
            _statics = null;
            _moneyData = null;
            _configuration = null;
            _rom = null;
            lblCodeDyn.Text = @"???";
            lblLangDyn.Text = @"???";
            lblVersionDyn.Text = @"???";
            lblNameDyn.Text = @"???";
        }

        private void openRomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog {Filter = @"Gamboy Advance ROMs|*.gba"};
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                UnloadAll();
                Rom r;
                try
                {
                    r = new Rom(openFile.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.Error_Global, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (_configurations.All(element => element.GameCode != r.Header.GameCode))
                {
                    MessageBox.Show(string.Format(Resources.ConfigurationNotFoundErrorEnglish, r.Header.GameCode),
                        Resources.Error_Global,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (_configurations.Where((element => element.GameCode == r.Header.GameCode)).Count() != 1)
                {
                    MessageBox.Show(string.Format(Resources.RedundantConfigurationErrorEnglish, r.Header.GameCode),
                        Resources.Error_Global,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _configuration = _configurations.Where((element => element.GameCode == r.Header.GameCode)).First();
                lblCodeDyn.Text = r.Header.GameCode;
                lblLangDyn.Text = r.Header.GameCode.Last().ToString(CultureInfo.InvariantCulture);
                lblNameDyn.Text = r.Header.Title;
                lblVersionDyn.Text = @"1." + r.Header.SoftwareVersion.ToString(CultureInfo.InvariantCulture);

                r.SetStreamOffset(_configuration.TrainerPointer);
                r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);

                for (int i = 0; i <= _configuration.TrainerCount; ++i)
                {
                    _trainerEntries.Add(new TrainerEntry(_encoder, r));
                }
                for (int i = 1; i < _trainerEntries.Count; ++i)
                {
                    lstTrainers.Items.Add(string.Format("{0}   {1}", i.ToString("x3").ToUpper(), _trainerEntries[i].Name));
                }
                _statics = new StaticElements(_configuration, r, _encoder);


                comAttackOne.Items.AddRange(_statics.AttackNames.ToArray());
                comAttackTwo.Items.AddRange(_statics.AttackNames.ToArray());
                comAttackThree.Items.AddRange(_statics.AttackNames.ToArray());
                comAttackFour.Items.AddRange(_statics.AttackNames.ToArray());
                comSpecies.Items.AddRange(_statics.PokemonNames.ToArray());
                comHeldItem.Items.AddRange(_statics.ItemNames.ToArray());
                numSprite.Maximum = _configuration.SpriteCount - 1;

                comItemOne.Items.AddRange(_statics.ItemNames.ToArray());
                comItemTwo.Items.AddRange(_statics.ItemNames.ToArray());
                comItemThree.Items.AddRange(_statics.ItemNames.ToArray());
                comItemFour.Items.AddRange(_statics.ItemNames.ToArray());
                _moneyData = new MoneyData(_configuration, r);
                r.SetStreamOffset(_configuration.TrainerClassNamePointer);
                r.SetStreamOffset(r.ReadUInt32() & 0x1FFFFFF);
                for (int i = 0; i < TRAINER_CLASS_COUNT; ++i)
                {
                    _trainerclassNames.Add(_encoder.GetParsedString(RomStringHelper.ReadRomString(r)));
                    r.SetStreamOffset(r.CurrentPosition + (12 - _trainerclassNames.Last().Length));
                }
                comClassname.DataSource = _trainerclassNames;
                //comClassname.Items.AddRange(_trainerclassNames.ToArray());

                lstTrainers.SelectedItems.Clear();
                lstTrainers.SelectedIndex = 0;


                grpTrainerSel.Enabled = true;
                tbcEditor.Enabled = true;

                _rom = r;
            }
        }

        private void GetCurrentTrainerEntry()
        {
            _currentEntry = _trainerEntries[lstTrainers.SelectedIndex + 1];
        }

        private void LoadTrainer()
        {
            GetCurrentTrainerEntry();
            txtTabId.Text = (lstTrainers.SelectedIndex + 1).ToString("x3");
            txtName.Text = _currentEntry.Name;
            if (_currentEntry.IsFemale)
                rdbFemale.Checked = true;
            else
                rdbMale.Checked = true;
            numMusic.Value = _currentEntry.Music;
            if (_currentEntry.Sprite == numSprite.Value)
                numSprite_ValueChanged(numSprite, null);
            numSprite.Value = _currentEntry.Sprite;
            txtUnknown.Text = _currentEntry.Unknown.ToString("x");
            numMoneyRate.Value = _currentEntry.TrainerClass < 0x30
                ? _moneyData.MoneyValues[_currentEntry.TrainerClass]
                : _moneyData.LastValue;
            comClassname.SelectedIndex = _currentEntry.TrainerClass;
            comItemOne.FormattingEnabled = false;
            comItemTwo.FormattingEnabled = false;
            comItemThree.FormattingEnabled = false;
            comItemFour.FormattingEnabled = false;
            comItemOne.SelectedIndex = _currentEntry.ItemOne;
            comItemTwo.SelectedIndex = _currentEntry.ItemTwo;
            comItemThree.SelectedIndex = _currentEntry.ItemThree;
            comItemFour.SelectedIndex = _currentEntry.ItemFour;

            //Load Pokemon Data
            numCurrentPokemon.Maximum = _currentEntry.PokeCount;
            numCurrentPokemon.Value = 1;
            numCountPokemon.Value = _currentEntry.PokeCount;
            chkDualBattle.Checked = _currentEntry.DualBattle;
            LoadPokemon();
        }

        private void LoadPokemon()
        {
            SinglePokemon _currentPokemon = _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1];
            txtPokemonOffset.Text = _currentEntry.PokemonData.Position.ToString("x").ToUpper();
            comSpecies.SelectedIndex = _currentPokemon.Species;
            comHeldItem.SelectedIndex = _currentPokemon.Item;
            numAi.Value = _currentPokemon.AiLevel;
            numLevel.Value = _currentPokemon.Level;
            comAttackOne.SelectedIndex = _currentPokemon.Attack1;
            comAttackTwo.SelectedIndex = _currentPokemon.Attack2;
            comAttackThree.SelectedIndex = _currentPokemon.Attack3;
            comAttackFour.SelectedIndex = _currentPokemon.Attack4;

        }

        private void lstTrainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTrainer();
        }

        private void txtId_TextChanged(object sender, EventArgs e)
        {
            int id;
            if (txtId.Text.Length > 0)
            {
                id = Convert.ToInt32(txtId.Text, 16);
                if (id < 1)
                    id = 1;
                if (id > _configuration.TrainerCount + 1)
                    id = _configuration.TrainerCount;
                lstTrainers.SelectedIndex = id - 1;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text.Length > 0)
            {
                _currentDeepness = 0;
                TrySelectingSearch();
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TrySelectingSearch();
            }
        }

        private void TrySelectingSearch()
        {
            int index = lstTrainers.Items.OfType<string>()
                .ToList()
                .FindIndex(_currentDeepness + 1,
                    element => element.ToLower().Remove(0, 6).StartsWith(txtSearch.Text.ToLower()));
            if (index != -1)
            {
                lstTrainers.SelectedIndex = index;
                _currentDeepness = index;
            }
        }

        private void comClassname_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtClassname.Text = comClassname.SelectedItem.ToString();
        }

        private void cmbSave_Click(object sender, EventArgs e)
        {
            _trainerclassNames[comClassname.SelectedIndex] = txtClassname.Text;
        }

        private void numSprite_ValueChanged(object sender, EventArgs e)
        {
            picSprite.Image = _statics.Sprites[(int) numSprite.Value];
            ColorPalette p = picSprite.Image.Palette;
            Color[] entries = p.Entries;
            entries[0] = Color.Transparent;
            picSprite.Image.Palette = p;
        }

        private void comSpecies_SelectedIndexChanged(object sender, EventArgs e)
        {
            picPokemon.Image = _statics.PokeSprites[comSpecies.SelectedIndex];
            ColorPalette p = picPokemon.Image.Palette;
            Color[] entries = p.Entries;
            entries[0] = Color.Transparent;
            picPokemon.Image.Palette = p;
        }

        private void numCountPokemon_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numCurrentPokemon_ValueChanged(object sender, EventArgs e)
        {
            LoadPokemon();
        }
    }
}