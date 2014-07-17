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
        private string _originalFile = "";
        private readonly List<Configuration> _configurations = new List<Configuration>();
        private List<TrainerEntry> _trainerEntries = new List<TrainerEntry>();
        private readonly BindingList<string> _trainerclassNames = new BindingList<string>();
        private Configuration _configuration;
        private int _currentDeepness;
        private TrainerEntry _currentEntry;
        private HexEncoder _encoder;
        private MoneyData _moneyData;
        private Rom _rom;
        private bool _suspendUpdate;
        private StaticElements _statics;
        private bool _isLoaded;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var deSerializer = new XmlSerializer(typeof(Configuration));
            if (!Directory.Exists(Constances.CONFIG_ROOT))
                Directory.CreateDirectory(Constances.CONFIG_ROOT);
            string[] files = Directory.GetFiles(Constances.CONFIG_ROOT);
            if (files.Length != 0)
            {
                foreach (string s in Directory.GetFiles(Constances.CONFIG_ROOT))
                {
                    try
                    {
                        _configurations.Add((Configuration)deSerializer.Deserialize(new FileStream(s, FileMode.Open)));
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
            tbcEditor.SelectTab(tbcEditor.TabPages[0]);
            _originalFile = "";
            picSprite.Image = null;
            txtTabId.Text = "";
            txtUnknown.Text = "";
            numMusic.Value = 0;
            numMoneyRate.Value = 0;
            grpTrainerSel.Enabled = false;
            cmbSave.Enabled = false;
            comSpecies.Items.Clear();
            comHeldItem.Items.Clear();
            comAttackOne.Items.Clear();
            comAttackTwo.Items.Clear();
            comAttackThree.Items.Clear();
            comAttackFour.Items.Clear();
            _isLoaded = false;
            tbcEditor.Enabled = false;
            lstTrainers.Items.Clear();
            grpTrainerSel.Enabled = false;
            _statics = null;
            _trainerEntries = new List<TrainerEntry>();
            _moneyData = null;
            _configuration = null;
            _rom = null;
            comAttackOne.Items.Clear();
            comAttackTwo.Items.Clear();
            comAttackThree.Items.Clear();
            comAttackFour.Items.Clear();
            comItemOne.Items.Clear();
            comItemTwo.Items.Clear();
            comItemThree.Items.Clear();
            comItemFour.Items.Clear();
            txtId.Text = "";
            txtName.Text = "";
            rdbFemale.Checked = false;
            rdbMale.Checked = false;
            numSprite.Value = 0;
            _trainerclassNames.Clear();
            txtClassname.Text = "";
            lblCodeDyn.Text = @"???";
            lblLangDyn.Text = @"???";
            lblVersionDyn.Text = @"???";
            lblNameDyn.Text = @"???";
        }

        private void openRomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog { Filter = @"Gamboy Advance ROMs|*.gba" };
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                UnloadAll();
                Rom r;
                try
                {
                    r = new Rom(openFile.FileName);
                    _originalFile = openFile.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.Error_Global, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    r.Save(Path.GetDirectoryName(openFile.FileName) +
                         string.Format("\\{0}.bak", Path.GetFileNameWithoutExtension(openFile.FileName)));
                }
                catch
                {
                    MessageBox.Show(
                        Resources.WarningBackupEnglish,
                        Resources.WarningEnglish, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                for (int i = 0; i < _configuration.TrainerClassCount; ++i)
                {
                    byte[] readString = RomStringHelper.ReadRomString(r);
                    _trainerclassNames.Add(_encoder.GetParsedString(readString));
                    r.SetStreamOffset(r.CurrentPosition + (12 - readString.Length));
                }
                comClassname.DataSource = _trainerclassNames;

                grpTrainerSel.Enabled = true;
                cmbSave.Enabled = true;
                tbcEditor.Enabled = true;

                _rom = r;

                lstTrainers.SelectedItems.Clear();
                _isLoaded = true;
                lstTrainers.SelectedIndex = 0;
            }
        }

        private void GetCurrentTrainerEntry()
        {
            _currentEntry = _trainerEntries[lstTrainers.SelectedIndex + 1];
        }

        private void LoadTrainer()
        {
            lbl_status.Text = string.Empty;
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
            numMoneyRate.Value = _moneyData[_currentEntry.TrainerClass];
            if (comClassname.SelectedIndex == _currentEntry.TrainerClass)
                comClassname_SelectedIndexChanged(null, null);
            else
                comClassname.SelectedIndex = _currentEntry.TrainerClass;
            comItemOne.FormattingEnabled = false;
            comItemTwo.FormattingEnabled = false;
            comItemThree.FormattingEnabled = false;
            comItemFour.FormattingEnabled = false;
            comItemOne.SelectedIndex = _currentEntry.ItemOne;
            comItemTwo.SelectedIndex = _currentEntry.ItemTwo;
            comItemThree.SelectedIndex = _currentEntry.ItemThree;
            comItemFour.SelectedIndex = _currentEntry.ItemFour;
            txtTrainerOffset.Text = _currentEntry.Position.ToString("x").ToUpper();

            //Load Pokemon Data
            numCurrentPokemon.Maximum = _currentEntry.PokeCount;
            numCurrentPokemon.Value = 1;
            numCountPokemon.Value = _currentEntry.PokeCount;
            chkDualBattle.Checked = _currentEntry.DualBattle;

            if (numCountPokemon.Value < 2)
            {
                chkDualBattle.Checked = false;
                chkDualBattle.Enabled = false;
            }
            else
            {
                chkDualBattle.Enabled = true;
            }

            _currentEntry.PokemonData = new PokemonEntry(_currentEntry.PokemonData.Position, _currentEntry);
            LoadPokemon();
            CheckRepoint();
        }

        private void LoadPokemon()
        {
            SinglePokemon currentPokemon = _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1];
            txtPokemonOffset.Text = _currentEntry.PokemonData.Position.ToString("x").ToUpper();
            comSpecies.SelectedIndex = currentPokemon.Species;
            comHeldItem.SelectedIndex = currentPokemon.Item;
            numAi.Value = currentPokemon.AiLevel;
            numLevel.Value = currentPokemon.Level;
            comAttackOne.SelectedIndex = currentPokemon.Attack1;
            comAttackTwo.SelectedIndex = currentPokemon.Attack2;
            comAttackThree.SelectedIndex = currentPokemon.Attack3;
            comAttackFour.SelectedIndex = currentPokemon.Attack4;

        }

        private void lstTrainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoaded || _suspendUpdate) return;
            LoadTrainer();
        }

        private void txtId_TextChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            if (txtId.Text.Length <= 0) return;
            int id = Convert.ToInt32(txtId.Text, 16);
            if (id < 1)
                id = 1;
            if (id > _configuration.TrainerCount + 1)
                id = _configuration.TrainerCount;
            lstTrainers.SelectedIndex = id - 1;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            if (txtSearch.Text.Length <= 0) return;
            _currentDeepness = 0;
            TrySelectingSearch();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isLoaded) return;
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
            if (index == -1) return;
            lstTrainers.SelectedIndex = index;
            _currentDeepness = index;
        }

        private void comClassname_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            if (_currentEntry == null) return;
            txtClassname.Text = comClassname.SelectedItem.ToString();
            numMoneyRate.Value = _moneyData[(byte)comClassname.SelectedIndex];
        }

        private void cmbSave_Click(object sender, EventArgs e)
        {
            if (_currentEntry.RequiresRepoint)
            {
                DialogResult result = MessageBox.Show(Resources.RepointPending, Resources.Information_Global,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result != DialogResult.Yes) return;
                _rom.SetStreamOffset(_configuration.FreespaceStart);
                _currentEntry.PokemonData.Position = _rom.GetFreeSpaceOffset(_currentEntry.PokemonData.GetSize(),
                    _configuration.FreespaceByte, 4);
            }

            _trainerclassNames[comClassname.SelectedIndex] = txtClassname.Text;
            _currentEntry.Unknown = uint.Parse(txtUnknown.Text);
            _currentEntry.Music = (byte)numMusic.Value;
            _currentEntry.PokeCount = (byte)numCountPokemon.Value;
            _currentEntry.Name = txtName.Text;
            _currentEntry.ItemOne = (ushort)comItemOne.SelectedIndex;
            _currentEntry.ItemTwo = (ushort)comItemTwo.SelectedIndex;
            _currentEntry.ItemThree = (ushort)comItemThree.SelectedIndex;
            _currentEntry.ItemFour = (ushort)comItemFour.SelectedIndex;
            _currentEntry.Sprite = (byte)numSprite.Value;
            _currentEntry.TrainerClass = (byte)comClassname.SelectedIndex;
            _currentEntry.IsFemale = rdbFemale.Checked;

            _rom.SetStreamOffset(_currentEntry.Position);
            _rom.WriteByteArray(_currentEntry.GetRawData());
            _rom.SetStreamOffset(_currentEntry.PokemonData.Position);
            _rom.WriteByteArray(_currentEntry.PokemonData.GetRawData());

            _rom.SetStreamOffset(_configuration.TrainerClassNamePointer);
            _rom.SetStreamOffset(_rom.ReadUInt32() & 0x1FFFFFF);

            _rom.SetStreamOffset(_rom.CurrentPosition + (comClassname.SelectedIndex * 13));
            _rom.WriteByteArray(_encoder.GetParsedBytes(txtClassname.Text));
            _rom.WriteByte(0xFF);

            _rom.SetStreamOffset(_configuration.TrainerClassPointer);
            _rom.SetStreamOffset(_rom.ReadUInt32() & 0x1FFFFFF);

            _rom.WriteToRom(_moneyData);


            _rom.Patch(_originalFile);
            //int index = lstTrainers.SelectedIndex;1
            //lstTrainers.Items.Clear();
            _suspendUpdate = true;
            lstTrainers.Items[lstTrainers.SelectedIndex] = string.Format("{0}   {1}",
                (lstTrainers.SelectedIndex + 1).ToString("x3").ToUpper(), _currentEntry.Name);
            _suspendUpdate = false;
            CheckRepoint();
            LoadTrainer();
            lbl_status.ForeColor = Color.Green;
            lbl_status.Text = Resources.Success_English;
            //for (int i = 1; i < _trainerEntries.Count; ++i)
            //{
            //    lstTrainers.Items.Add(string.Format("{0}   {1}", i.ToString("x3").ToUpper(), _trainerEntries[i].Name));
            //}
            //lstTrainers.SelectedIndex = index;
        }



        private void numSprite_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            picSprite.Image = _statics.Sprites[(int)numSprite.Value];
            ColorPalette p = picSprite.Image.Palette;
            Color[] entries = p.Entries;
            entries[0] = Color.Transparent;
            picSprite.Image.Palette = p;
        }

        private void comSpecies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].Species =
                (ushort)comSpecies.SelectedIndex;
            picPokemon.Image = _statics.PokeSprites[comSpecies.SelectedIndex];
            if (picPokemon.Image.Palette.Entries.Length <= 0) return;
            ColorPalette p = picPokemon.Image.Palette;
            Color[] entries = p.Entries;
            entries[0] = Color.Transparent;
            picPokemon.Image.Palette = p;
        }

        private void numCountPokemon_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            if (numCurrentPokemon.Value > numCountPokemon.Value)
                numCurrentPokemon.Value = numCountPokemon.Value;
            while (_currentEntry.PokemonData.Entries.Count > numCountPokemon.Value)
                _currentEntry.PokemonData.Entries.Remove(_currentEntry.PokemonData.Entries.Last());
            while (_currentEntry.PokemonData.Entries.Count < numCountPokemon.Value)
                _currentEntry.PokemonData.Entries.Add(SinglePokemon.BlankPokemon(_currentEntry));
            numCurrentPokemon.Maximum = numCountPokemon.Value;
            if (numCountPokemon.Value < 2)
            {
                chkDualBattle.Checked = false;
                chkDualBattle.Enabled = false;
            }
            else
            {
                chkDualBattle.Enabled = true;
            }
            CheckRepoint();
            //_currentEntry.PokemonData.Entries.Add(Something);
        }

        private void CheckRepoint()
        {
            _currentEntry.RequiresRepoint = _currentEntry.PokemonData.GetSize() > _currentEntry.PokemonData.GetOriginalSize();
            lblRepoint.Text = _currentEntry.RequiresRepoint ? Resources.Repoint_Required : "";
        }

        private void numCurrentPokemon_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            LoadPokemon();
        }

        private void numLevel_ValueChanged(object sender, EventArgs e)
        {
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].Level = (byte)numLevel.Value;
        }

        private void comHeldItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].Item = (ushort)comHeldItem.SelectedIndex;
            CheckRepoint();
        }

        private void numAi_ValueChanged(object sender, EventArgs e)
        {
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].AiLevel = (byte)numAi.Value;
        }

        private void chkDualBattle_CheckedChanged(object sender, EventArgs e)
        {
            _currentEntry.DualBattle = chkDualBattle.Checked;
        }

        private void comAttackOne_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].Attack1 = (ushort)comAttackOne.SelectedIndex;
            CheckRepoint();
        }

        private void comAttackTwo_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].Attack2 = (ushort)comAttackTwo.SelectedIndex;
            CheckRepoint();
        }

        private void comAttackThree_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].Attack3 = (ushort)comAttackThree.SelectedIndex;
            CheckRepoint();
        }

        private void comAttackFour_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentEntry.PokemonData.Entries[(int)numCurrentPokemon.Value - 1].Attack4 = (ushort)comAttackFour.SelectedIndex;
            CheckRepoint();
        }

        private void txtUnknown_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(char.IsDigit(e.KeyChar)) && !char.IsControl(e.KeyChar);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                string.Format("Version {0} of open Trainer.net, visit us on github for a copy of the source code.",
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version), Resources.About_English, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void cmbRepoint_Click_1(object sender, EventArgs e)
        {
            uint offset = 0;
            string result = "";
            if (FormHelper.InputBox("Repoint", "Enter manual offset (Hexadecimal representation)", ref result) == DialogResult.OK)
            {
                try
                {
                    offset = Convert.ToUInt32(result, 16);
                }
                catch (FormatException ex)
                {
                    MessageBox.Show("Illegal Characters found, make sure to only use correct hexadecimal characters without hex specifier", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (
                    MessageBox.Show(
                        "The file needs to be saved, any data at your given offset will be overwritten, do you wish to proceed?",
                        "Repoint", MessageBoxButtons.YesNo, MessageBoxIcon.None) == DialogResult.Yes)
                {
                    _currentEntry.PokemonData.Position = offset;
                    _currentEntry.RequiresRepoint = false;
                    cmbSave_Click(null, null);
                }
                
            }
        }
    }
}