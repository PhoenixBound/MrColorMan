using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GBAHL;
using GBAHL.Drawing;

namespace MrColorMan
{
    public partial class FormMain : Form
    {
        private const int PAL_LENGTH_BYTES_STANDARD = 32;
        private const int PAL_LENGTH_BYTES_M12 = 0; // fixme
        static bool m12 = false;

        TextBox[] colorActual;
        TextBox[] displayActual;
        TextBox[] colorChanged;
        TextBox[] displayChanged;

        private GBAHL.IO.ROM_old r;
        private Palette actualPalette;
        private Palette changedPalette;

        public Palette ActualPalette
        {
            get => actualPalette;
            set
            {
                actualPalette = value;
                DisplayActualPalette();
            }
        }
        public Palette ChangedPalette
        {
            get => changedPalette;
            set
            {
                changedPalette = value;
                DisplayChangedPalette();
            }
        }

        public static int PAL_LENGTH_BYTES
        {
            get
            {
                return m12 ? PAL_LENGTH_BYTES_M12 : PAL_LENGTH_BYTES_STANDARD;
            }
        }

        public FormMain()
        {
            InitializeComponent();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Open ROM...
            DialogResult result = openRomDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                var f = FileAccess.Read;
                if (!openRomDialog.ReadOnlyChecked)
                {
                    f = FileAccess.ReadWrite;
                }
                    
                r = new GBAHL.IO.ROM_old(openRomDialog.FileName, f, FileShare.ReadWrite);
                Text = $"Mr. Color Man - {r.Name} [{r.Code}{r.Maker}]";
                EnableInitialControls();
            }
        }

        private void EnableInitialControls()
        {
            // TODO: Enable these controls as they are implemented
            radioButtonLoadFromOffset.Enabled = true;
            // radioButtonLoadBySearching.Enabled = true;
            // checkBoxCompressedPalette.Enabled = true;
            textBoxHexOffset.Enabled = true;
            buttonLoadPalette.Enabled = true;
        }

        private void DisplayActualPalette()
        {
            if (!textBoxColorActual1.Enabled)
            {
                foreach (TextBox t in colorActual)
                    t.Enabled = true;
            }

            for (int i = 0; i < actualPalette.Length; i++)
            {
                colorActual[i].Text = actualPalette[i].Gba555Color.ToString("X4");
                displayActual[i].BackColor = actualPalette[i].RgbColor;
            }

            WarnActualInvalidColors();
        }

        void WarnActualInvalidColors()
        {
            for (int i = 0; i < actualPalette.Length; i++)
            {
                WarnInvalidColor(actualIndex: i);
            }
        }

        void WarnChangedInvalidColors()
        {
            for (int i = 0; i < changedPalette.Length; i++)
            {
                WarnInvalidColor(changedIndex: i);
            }
        }

        void WarnInvalidColor(int actualIndex = -1, int changedIndex = -1)
        {
            if (actualIndex != -1)
            {
                if ((actualPalette[actualIndex].Gba555Color & 0x8000) != 0)
                {
                    displayActual[actualIndex].Text = "X";
                    displayActual[actualIndex].ForeColor = Color.FromArgb(actualPalette[actualIndex].RgbColor.ToArgb() ^ 0xFFFFFF);
                }
                else
                {
                    displayActual[actualIndex].Text = "";
                }
                return;
            }
            else if (changedIndex != -1)
            {
                if ((changedPalette[changedIndex].Gba555Color & 0x8000) != 0)
                {
                    displayChanged[changedIndex].Text = "X";
                    displayChanged[changedIndex].ForeColor = Color.FromArgb(changedPalette[changedIndex].RgbColor.ToArgb() ^ 0xFFFFFF);
                }
                else
                {
                    displayChanged[changedIndex].Text = "";
                }
                return;
            }
            else
            {
                throw new ArgumentException("Choose an array to warn about, dang it. Bad dev.");
            }
        }

        private void DisplayChangedPalette()
        {
            if (!textBoxColorChanged1.Enabled)
            {
                foreach (TextBox t in colorChanged)
                    t.Enabled = true;
            }
            // TODO: Display stuff using the array
            for (int i = 0; i < changedPalette.Length; i++)
            {
                colorChanged[i].Text = changedPalette[i].Gba555Color.ToString("X4");
                displayChanged[i].BackColor = changedPalette[i].RgbColor;
            }

            WarnChangedInvalidColors();
        }

        /// <summary>
        /// Changes the state of the "Load Palette" button based on if there is an offset to go to.
        /// </summary>
        private void textBoxHexOffset_TextChanged(object sender, EventArgs e)
        {
            if (buttonLoadPalette.Enabled)
            {
                if (textBoxHexOffset.TextLength == 0)
                    buttonLoadPalette.Enabled = false;
                return;
            }
            if (textBoxHexOffset.TextLength != 0)
                buttonLoadPalette.Enabled = true;
        }

        private void buttonLoadPalette_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxHexOffset.Text, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out int offset))
            {
                // Make sure offset actually exists. (If GBA Video type carts used these palettes,
                // they'd actually be supported.)
                if (offset + PAL_LENGTH_BYTES > r.Length || offset < 0)
                {
                    MessageBox.Show("Invalid palette offset.", "Offset invalid", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }
                r.Seek(offset);
                ActualPalette = r.ReadPalette(16);
                
                if (!buttonCopyToChanged.Enabled)
                {
                    buttonCopyToChanged.Enabled = true;
                    buttonReplacePalette.Enabled = true;
                    ChangedPalette = ActualPalette;
                }
            }
            else
            {
                MessageBox.Show("Please enter only hex digits in the text box.", "Offset invalid",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            colorActual = new TextBox[16];
            colorActual[0] = textBoxColorActual1;
            colorActual[1] = textBoxColorActual2;
            colorActual[2] = textBoxColorActual3;
            colorActual[3] = textBoxColorActual4;
            colorActual[4] = textBoxColorActual5;
            colorActual[5] = textBoxColorActual6;
            colorActual[6] = textBoxColorActual7;
            colorActual[7] = textBoxColorActual8;
            colorActual[8] = textBoxColorActual9;
            colorActual[9] = textBoxColorActual10;
            colorActual[10] = textBoxColorActual11;
            colorActual[11] = textBoxColorActual12;
            colorActual[12] = textBoxColorActual13;
            colorActual[13] = textBoxColorActual14;
            colorActual[14] = textBoxColorActual15;
            colorActual[15] = textBoxColorActual16;

            displayActual = new TextBox[16];
            displayActual[0] = textBoxDisplayActual1;
            displayActual[1] = textBoxDisplayActual2;
            displayActual[2] = textBoxDisplayActual3;
            displayActual[3] = textBoxDisplayActual4;
            displayActual[4] = textBoxDisplayActual5;
            displayActual[5] = textBoxDisplayActual6;
            displayActual[6] = textBoxDisplayActual7;
            displayActual[7] = textBoxDisplayActual8;
            displayActual[8] = textBoxDisplayActual9;
            displayActual[9] = textBoxDisplayActual10;
            displayActual[10] = textBoxDisplayActual11;
            displayActual[11] = textBoxDisplayActual12;
            displayActual[12] = textBoxDisplayActual13;
            displayActual[13] = textBoxDisplayActual14;
            displayActual[14] = textBoxDisplayActual15;
            displayActual[15] = textBoxDisplayActual16;

            colorChanged = new TextBox[16];
            colorChanged[0] = textBoxColorChanged1;
            colorChanged[1] = textBoxColorChanged2;
            colorChanged[2] = textBoxColorChanged3;
            colorChanged[3] = textBoxColorChanged4;
            colorChanged[4] = textBoxColorChanged5;
            colorChanged[5] = textBoxColorChanged6;
            colorChanged[6] = textBoxColorChanged7;
            colorChanged[7] = textBoxColorChanged8;
            colorChanged[8] = textBoxColorChanged9;
            colorChanged[9] = textBoxColorChanged10;
            colorChanged[10] = textBoxColorChanged11;
            colorChanged[11] = textBoxColorChanged12;
            colorChanged[12] = textBoxColorChanged13;
            colorChanged[13] = textBoxColorChanged14;
            colorChanged[14] = textBoxColorChanged15;
            colorChanged[15] = textBoxColorChanged16;

            displayChanged = new TextBox[16];
            displayChanged[0] = textBoxDisplayChanged1;
            displayChanged[1] = textBoxDisplayChanged2;
            displayChanged[2] = textBoxDisplayChanged3;
            displayChanged[3] = textBoxDisplayChanged4;
            displayChanged[4] = textBoxDisplayChanged5;
            displayChanged[5] = textBoxDisplayChanged6;
            displayChanged[6] = textBoxDisplayChanged7;
            displayChanged[7] = textBoxDisplayChanged8;
            displayChanged[8] = textBoxDisplayChanged9;
            displayChanged[9] = textBoxDisplayChanged10;
            displayChanged[10] = textBoxDisplayChanged11;
            displayChanged[11] = textBoxDisplayChanged12;
            displayChanged[12] = textBoxDisplayChanged13;
            displayChanged[13] = textBoxDisplayChanged14;
            displayChanged[14] = textBoxDisplayChanged15;
            displayChanged[15] = textBoxDisplayChanged16;
        }

        private void textBoxHexOffset_Enter(object sender, EventArgs e) 
            => ActiveForm.AcceptButton = buttonLoadPalette;

        private void textBoxHexOffset_Leave(object sender, EventArgs e) 
            => ActiveForm.AcceptButton = null;

        private void buttonCopyToChanged_Click(object sender, EventArgs e)
        {
            ChangedPalette = ActualPalette;
        }

        private void LiveUpdateChangedPalette(int index)
        {
            if (colorChanged[index].TextLength == 4
                && ushort.TryParse(colorChanged[index].Text, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out ushort c))
            {
                ChangedPalette[index] = new GbaColor(c);
                // ...But the setter doesn't get called for accessing the array, so we need to do
                // this manually anyway. *Joy.*
                DisplayChangedPalette();
            }
        }

        private void textBoxColorChanged1_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(0);
        }

        private void textBoxColorChanged2_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(1);
        }

        private void textBoxColorChanged3_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(2);
        }

        private void textBoxColorChanged4_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(3);
        }

        private void textBoxColorChanged5_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(4);
        }

        private void textBoxColorChanged6_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(5);
        }

        private void textBoxColorChanged7_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(6);
        }

        private void textBoxColorChanged8_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(7);
        }

        private void textBoxColorChanged9_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(8);
        }

        private void textBoxColorChanged10_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(9);
        }

        private void textBoxColorChanged11_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(10);
        }

        private void textBoxColorChanged12_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(11);
        }

        private void textBoxColorChanged13_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(12);
        }

        private void textBoxColorChanged14_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(13);
        }

        private void textBoxColorChanged15_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(14);
        }

        private void textBoxColorChanged16_TextChanged(object sender, EventArgs e)
        {
            LiveUpdateChangedPalette(15);
        }

        private void buttonReplacePalette_Click(object sender, EventArgs e)
        {
            r.Position -= PAL_LENGTH_BYTES;
            r.WritePalette(ChangedPalette);
            ActualPalette = ChangedPalette;
        }

        private void textBoxDisplayChanged1_Click(object sender, EventArgs e)
        {
            var result = colorDialog1.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                    MessageBox.Show("The color is " + colorDialog1.Color);
                    break;
                case DialogResult.Cancel:
                    break;
                default:
                    break;
            }
        }
    }
}
