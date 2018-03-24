using System;
using System.Linq;
using System.Windows.Forms;
using static GatherUp.Order.Profile;

namespace GatherUp
{
    public partial class HotSpotForm : Form
    {
        private readonly HotSpot _hotspot;

        internal HotSpotForm(HotSpot hotspot)
        {
            InitializeComponent();
            _hotspot = hotspot;
            RefreshForm();
            if (_hotspot.FlyTo.Destinations.Any())
            {
                listboxFlyingDest.SelectedIndex = 0;
            }
        }

        private void RefreshForm()
        {
            try
            {
                listboxFlyingDest.DataSource = null;
                listboxFlyingDest.DataSource = _hotspot.FlyTo.Destinations;
                listboxFlyingDest.DisplayMember = "Position";
                chkboxDisableMount.Checked = _hotspot.DisableMount;
                chkboxStealth.Checked = _hotspot.IsStealth;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            var position = ff14bot.Core.Player.Location;
            _hotspot.FlyTo.Destinations.Add(new FlyTo.Destination
            {
                Position = position
            });
            RefreshForm();
            foreach (FlyTo.Destination item in listboxFlyingDest.Items)
            {
                if (!item.Position.Equals(position)) continue;
                listboxFlyingDest.SelectedItem = item;
                break;
            }
        }

        private void chkboxStealth_CheckedChanged(object sender, EventArgs e)
        {
            _hotspot.IsStealth = chkboxStealth.Checked;
        }

        private bool AlwaysLands()
        {
            return _hotspot.FlyTo.Destinations.TrueForAll(dest => dest.Land);
        }
        
        private void chkboxLand_CheckedChanged(object sender, EventArgs e)
        {
            var destination = (FlyTo.Destination) listboxFlyingDest.SelectedItem;
            if (destination == null)
            {
                chkboxLand.Checked = false;
                return;
            }

            destination.Land = chkboxLand.Checked;

        }

        private void chkboxDisableMount_CheckedChanged(object sender, EventArgs e)
        {
            _hotspot.DisableMount = chkboxDisableMount.Checked;
        }

        private void listboxFlyingDest_SelectedValueChanged(object sender, EventArgs e)
        {
            chkboxLand.Checked = ((FlyTo.Destination)listboxFlyingDest.SelectedItem)?.Land ?? false;
        }

        private void listboxFlyingDest_KeyDown(object sender, KeyEventArgs e)
        {
            if (listboxFlyingDest.SelectedItem == null) return;
            if (e.KeyCode != Keys.Delete) return;
            var selectedDestination = listboxFlyingDest.SelectedItem as FlyTo.Destination;
            _hotspot.FlyTo.Destinations.Remove(selectedDestination);
            RefreshForm();
        }

        private void HotSpotForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_hotspot.IsStealth)
            {
                if (!AlwaysLands())
                {
                    MessageBox.Show("To avoid problems with stealth, Landing has been enabled in this hotspot");
                    _hotspot.FlyTo.Destinations.ForEach(dest => dest.Land = true);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Disables/enables mounting when arriving/leaving a hotspot.\r\nUseful to avoid breaking stealth");
        }
    }
}
