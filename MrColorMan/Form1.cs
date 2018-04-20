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
        private GBAHL.IO.ROM_old r;
        private Palette actualPalette;
        private Palette changedPalette;

        public Palette ActualPalette
        {
            get => actualPalette;
            set
            {
                actualPalette = value;
            }
        }
        public Palette ChangedPalette
        {
            get => changedPalette;
            set
            {
                changedPalette = value;
                DisplayActualPalette();
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
                r = new GBAHL.IO.ROM_old(openRomDialog.FileName);
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
                textBoxColorActual1.Enabled = true;
                textBoxColorActual2.Enabled = true;
                textBoxColorActual3.Enabled = true;
                textBoxColorActual4.Enabled = true;
                textBoxColorActual5.Enabled = true;
                textBoxColorActual6.Enabled = true;
                textBoxColorActual7.Enabled = true;
                textBoxColorActual8.Enabled = true;
                textBoxColorActual9.Enabled = true;
                textBoxColorActual10.Enabled = true;
                textBoxColorActual11.Enabled = true;
                textBoxColorActual12.Enabled = true;
                textBoxColorActual13.Enabled = true;
                textBoxColorActual14.Enabled = true;
                textBoxColorActual15.Enabled = true;
                textBoxColorActual16.Enabled = true;
            }

            // First, the text boxes themselves, the most important part.
            textBoxColorActual1.Text = actualPalette[0].Gba555Color.ToString("X4");
            textBoxColorActual2.Text = actualPalette[1].Gba555Color.ToString("X4");
            textBoxColorActual3.Text = actualPalette[2].Gba555Color.ToString("X4");
            textBoxColorActual4.Text = actualPalette[3].Gba555Color.ToString("X4");
            textBoxColorActual5.Text = actualPalette[4].Gba555Color.ToString("X4");
            textBoxColorActual6.Text = actualPalette[5].Gba555Color.ToString("X4");
            textBoxColorActual7.Text = actualPalette[6].Gba555Color.ToString("X4");
            textBoxColorActual8.Text = actualPalette[7].Gba555Color.ToString("X4");
            textBoxColorActual9.Text = actualPalette[8].Gba555Color.ToString("X4");
            textBoxColorActual10.Text = actualPalette[9].Gba555Color.ToString("X4");
            textBoxColorActual11.Text = actualPalette[10].Gba555Color.ToString("X4");
            textBoxColorActual12.Text = actualPalette[11].Gba555Color.ToString("X4");
            textBoxColorActual13.Text = actualPalette[12].Gba555Color.ToString("X4");
            textBoxColorActual14.Text = actualPalette[13].Gba555Color.ToString("X4");
            textBoxColorActual15.Text = actualPalette[14].Gba555Color.ToString("X4");
            textBoxColorActual16.Text = actualPalette[15].Gba555Color.ToString("X4");

            // Next, we'll display the actual colors.
            // TODO: See how this interacts with invalid colors
            textBoxDisplayActual1.BackColor = actualPalette[0].RgbColor;
            textBoxDisplayActual2.BackColor = actualPalette[1].RgbColor;
            textBoxDisplayActual3.BackColor = actualPalette[2].RgbColor;
            textBoxDisplayActual4.BackColor = actualPalette[3].RgbColor;
            textBoxDisplayActual5.BackColor = actualPalette[4].RgbColor;
            textBoxDisplayActual6.BackColor = actualPalette[5].RgbColor;
            textBoxDisplayActual7.BackColor = actualPalette[6].RgbColor;
            textBoxDisplayActual8.BackColor = actualPalette[7].RgbColor;
            textBoxDisplayActual9.BackColor = actualPalette[8].RgbColor;
            textBoxDisplayActual10.BackColor = actualPalette[9].RgbColor;
            textBoxDisplayActual11.BackColor = actualPalette[10].RgbColor;
            textBoxDisplayActual12.BackColor = actualPalette[11].RgbColor;
            textBoxDisplayActual13.BackColor = actualPalette[12].RgbColor;
            textBoxDisplayActual14.BackColor = actualPalette[13].RgbColor;
            textBoxDisplayActual15.BackColor = actualPalette[14].RgbColor;
            textBoxDisplayActual16.BackColor = actualPalette[15].RgbColor;
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
                if (offset + 16 > r.Length || offset < 0)
                {
                    MessageBox.Show("Invalid palette offset.", "Offset invalid", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }
                r.Seek(offset);
                ActualPalette = r.ReadPalette(16);
                ChangedPalette = ActualPalette;
            }
            else
            {
                MessageBox.Show("Please enter only hex digits in the text box.", "Offset invalid",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }
    }
}
