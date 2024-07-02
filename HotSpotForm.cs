using System;
using System.Linq;
using System.Windows.Forms;
using static GatherUp.Order.Profile;

namespace GatherUp
{
    public partial class HotSpotForm : Form
    {
        private readonly Order.Profile.HotSpot _hotspot;

        internal HotSpotForm(Order.Profile.HotSpot hotspot)
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
                radiusNumericUpDown.Value = _hotspot.Radius;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


        private void btnAddFlyingDest_Click(object sender, EventArgs e)
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

        private void listboxFlyingDest_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (listboxFlyingDest.SelectedItem == null) return;
            if (e.KeyCode != Keys.Delete) return;
            var selectedDestination = listboxFlyingDest.SelectedItem as FlyTo.Destination;
            _hotspot.FlyTo.Destinations.Remove(selectedDestination);
            RefreshForm();
        }

        private void listboxFlyingDest_SelectedValueChanged_1(object sender, EventArgs e)
        {
            chkboxLand.Checked = ((FlyTo.Destination)listboxFlyingDest.SelectedItem)?.Land ?? false;
        }

        private void chkboxDisableMount_CheckedChanged_1(object sender, EventArgs e)
        {
            _hotspot.DisableMount = chkboxDisableMount.Checked;
        }

        private void chkboxLand_CheckedChanged_1(object sender, EventArgs e)
        {
            var destination = (FlyTo.Destination)listboxFlyingDest.SelectedItem;
            if (destination == null)
            {
                chkboxLand.Checked = false;
                return;
            }

            destination.Land = chkboxLand.Checked;
        }

        private void radiusNumericUpDown_ValueChanged_1(object sender, EventArgs e)
        {
            _hotspot.Radius = (int)radiusNumericUpDown.Value;
        }
    }
}
