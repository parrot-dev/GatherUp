using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ff14bot.Managers;


namespace GatherUp
{
    public partial class GatherUpForm : Form
    {
        public GatherUpForm()
        {
            InitializeComponent();
            _order = new Order();

            this.Text = "GatherUp - " + GatherUp.version.ToString();
            numRadius.Value = new Order.HotSpot(new Clio.Utilities.Vector3()).Radius;
            numRadiusBlackSpot.Value = 10;
            lblPath.Text = Settings.current.SavePath;

            //Gatheringskills 
            foreach (var spell in DataManager.SpellCache.Values.Where(o => o.Job == ff14bot.Enums.ClassJobType.Miner || o.Job == ff14bot.Enums.ClassJobType.Botanist))
            {
                cbBoxGatheringSkills.Items.Add(spell.Name);
            }
            cbBoxGatheringSkills.Sorted = true;

            //teleportLocations:
            foreach (var teleport in ff14bot.Managers.WorldManager.AvailableLocations)
            {
                cbBoxLocationNameOnStart.Items.Add(teleport);
                cbBoxLocationNameOnComplete.Items.Add(teleport);
            }
            cbBoxLocationNameOnStart.DisplayMember = "Name";
            cbBoxLocationNameOnComplete.DisplayMember = "Name";

            cbBoxCordialType.Items.Add(Order.CordialType.None);
            cbBoxCordialType.Items.Add(Order.CordialType.Cordial);
            cbBoxCordialType.Items.Add(Order.CordialType.HiCordial);
            cbBoxCordialType.Items.Add(Order.CordialType.Auto);
            cbBoxCordialType.SelectedIndex = 0;

            this.refreshForm();
        }
        private Order _order;

        private void refreshForm()
        {
            //teleport
            chkboxTeleportOnStart.Checked = _order.TeleportOnStart.Enabled;
            cbBoxLocationNameOnStart.Text = _order.TeleportOnStart.Name;
            chkBoxTeleportOnComplete.Checked = _order.TeleportOnComplete.Enabled;
            cbBoxLocationNameOnComplete.Text = _order.TeleportOnComplete.Name;

            //General
            txtboxName.Text = _order.name;
            txtBoxTarget.Text = _order.gather.Target;
            txtboxItemId.Text = _order.gather.ItemId;
            chkBoxQuantity.Checked = !_order.gather.Infinite;
            numQuantity.Value = _order.gather.Quantity;

            if (_order.gather.Infinite)
            {
                lblItemId.Hide();
                txtboxItemId.Hide();
                btnListInventory.Hide();
            }
            else
            {
                lblItemId.Show();
                txtboxItemId.Show();
                btnListInventory.Show();
            }

            chkBoxGearSet.Checked = _order.gear.Enabled;
            numGearSet.Value = _order.gear.GearSet;

            //items:

            txtboxItemNames.Text = string.Empty;
            listBox2Items.DataSource = null;
            listBox2Items.DataSource = _order.Items;


            //hotspots
            listBoxHotSpots.DataSource = null;
            listBoxHotSpots.DataSource = _order.Hotspots;
            //blackspots
            listBoxBlackSpots.DataSource = null;
            listBoxBlackSpots.DataSource = _order.Blackspots;
            if (_order.gather.exGather.Enabled)
            {
                btnAddBlackspot.Enabled = false;
                btnAddBlackspot.Text = "ExGather: not supported";
            }
            else
            {
                btnAddBlackspot.Enabled = true;
                btnAddBlackspot.Text = "Add current location";
            }

            //gatheringskills            
            listBoxGatherSkills.DataSource = null;
            listBoxGatherSkills.DataSource = _order.Gatherskills;

            //Exgather
            chkboxExGather.Checked = _order.gather.exGather.Enabled;
            chkboxDiscoverUnknowns.Enabled = _order.gather.exGather.Enabled;
            chkboxDiscoverUnknowns.Checked = _order.gather.exGather.DiscoverUnknowns;
            cbBoxCordialType.SelectedItem = _order.gather.exGather.CordialType;
            cbBoxCordialType.Enabled = _order.gather.exGather.Enabled;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            _order.Items.Add(txtboxItemNames.Text);
            listBox2Items.DataSource = null;
            listBox2Items.DataSource = _order.Items;

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            _order.gather.ItemId = txtboxItemId.Text;
        }

        private bool profileHasErrors(out string errorMsg)
        {
            errorMsg = string.Empty;
            if (!_order.gather.Infinite && string.IsNullOrEmpty(_order.gather.ItemId))
            {
                errorMsg = "Item Id hasn't been specified";
                return true;
            }
            if (string.IsNullOrEmpty(_order.name))
            {
                errorMsg = "Profile hasn't got a name.";
                return true;
            }
            if (string.IsNullOrEmpty(_order.gather.Target))
            {
                errorMsg = "Target node hasn't been specified.";
                return true;
            }
            if (_order.Items.Count() == 0)
            {
                errorMsg = "No items to gather have been specified";
                return true;
            }
            if (_order.Hotspots.Count() == 0)
            {
                errorMsg = "Profile hasn't got any hotspots";
                return true;
            }
            return false;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string errorMsg = string.Empty;
            if (this.profileHasErrors(out errorMsg))
            {
                MessageBox.Show(errorMsg);
                return;
            }
            if (_order.gather.exGather.Enabled && _order.Blackspots.Count() > 0)
            {
                MessageBox.Show("Warning: All blackspots will be omitted.");
            }            
            string fullSavePath = Settings.current.SavePath + string.Format("\\{0}.xml", _order.name);
            if (System.IO.File.Exists(fullSavePath))
            {
                var confirmResult = MessageBox.Show("A file with this name already exists.\r\nDo you want to overwrite it?",
                    "Overwrite?",
                    MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.No)
                    return;
            }

            var xmlOrder = _order.ToXml();
            xmlOrder.Save(fullSavePath);
        }

        private void txtboxName_TextChanged(object sender, EventArgs e)
        {
            _order.name = txtboxName.Text;
        }

        private void txtBoxTarget_TextChanged(object sender, EventArgs e)
        {
            _order.gather.Target = txtBoxTarget.Text;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            GameObjectManager.Update();
            if (GameObjectManager.LocalPlayer.HasTarget)
            {
                try
                {
                    var target = GameObjectManager.LocalPlayer.CurrentTarget;
                    txtBoxTarget.Text = target.Name;
                    _order.gather.Target = target.Name;
                }
                catch (Exception err) { MessageBox.Show(err.Message); }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            using (var iForm = new InventoryForm())
            {
                iForm.ShowDialog();
                uint itemId = iForm.itemId;
                string itemName = iForm.itemName;
                if (itemId != 0)
                {
                    _order.gather.ItemId = itemId.ToString();
                    txtboxItemId.Text = _order.gather.ItemId;
                    if (!_order.Items.Contains(itemName))
                        txtboxItemNames.Text = itemName;
                }
            }

        }

        private void chkBoxQuantity_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxQuantity.Checked)
            {
                lblItemId.Show();
                txtboxItemId.Show();
                btnListInventory.Show();
            }
            else
            {
                lblItemId.Hide();
                txtboxItemId.Hide();
                btnListInventory.Hide();
            }

            _order.gather.Infinite = !chkBoxQuantity.Checked;
        }

        private void numQuantity_ValueChanged(object sender, EventArgs e)
        {
            _order.gather.Quantity = (int)numQuantity.Value;
        }

        private void chkBoxGearSet_CheckedChanged(object sender, EventArgs e)
        {
            _order.gear.Enabled = chkBoxGearSet.Checked;

        }

        private void numGearSet_ValueChanged(object sender, EventArgs e)
        {
            _order.gear.GearSet = (int)numGearSet.Value;
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBox2Items.SelectedIndex;
            if (listBox2Items.Focused && index != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _order.Items.RemoveAt(listBox2Items.SelectedIndex);
                    listBox2Items.DataSource = null;
                    listBox2Items.DataSource = _order.Items;
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Down)
                {
                    if (index != _order.Items.Count() - 1)
                    {
                        _order.Items = swap(_order.Items, index, index + 1);
                        refreshForm();
                        listBox2Items.SelectedIndex = index + 1;
                        e.Handled = true;
                    }
                }
                if (e.KeyCode == Keys.Up)
                {
                    if (index > 0)
                    {
                        _order.Items = swap(_order.Items, index, index - 1);
                        refreshForm();
                        listBox2Items.SelectedIndex = index - 1;
                        e.Handled = true;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var hotspot = GameObjectManager.LocalPlayer.Location;
            _order.Hotspots.Add(new Order.HotSpot(hotspot, (int)numRadius.Value));
            listBoxHotSpots.DataSource = null;
            listBoxHotSpots.DataSource = _order.Hotspots;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBoxHotSpots.SelectedIndex;
            if (listBoxHotSpots.Focused && index != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _order.Hotspots.RemoveAt(listBoxHotSpots.SelectedIndex);
                    listBoxHotSpots.DataSource = null;
                    listBoxHotSpots.DataSource = _order.Hotspots;
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Down)
                {
                    if (index != _order.Hotspots.Count() - 1)
                    {
                        _order.Hotspots = swap(_order.Hotspots, index, index + 1);
                        refreshForm();
                        listBoxHotSpots.SelectedIndex = index + 1;
                        e.Handled = true;
                    }
                }
                if (e.KeyCode == Keys.Up)
                {
                    if (index > 0)
                    {
                        _order.Hotspots = swap(_order.Hotspots, index, index - 1);
                        refreshForm();
                        listBoxHotSpots.SelectedIndex = index - 1;
                        e.Handled = true;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (cbBoxGatheringSkills.SelectedIndex != -1)
            {
                _order.Gatherskills.Add(cbBoxGatheringSkills.SelectedItem.ToString());
                listBoxGatherSkills.DataSource = null;
                listBoxGatherSkills.DataSource = _order.Gatherskills;
            }
        }

        private void listBox3_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBoxGatherSkills.SelectedIndex;
            if (listBoxGatherSkills.Focused && index != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _order.Gatherskills.RemoveAt(listBoxGatherSkills.SelectedIndex);
                    listBoxGatherSkills.DataSource = null;
                    listBoxGatherSkills.DataSource = _order.Gatherskills;
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Down)
                {
                    if (index != _order.Gatherskills.Count() - 1)
                    {
                        _order.Gatherskills = swap(_order.Gatherskills, index, index + 1);
                        refreshForm();
                        listBoxGatherSkills.SelectedIndex = index + 1;
                        e.Handled = true;
                    }
                }
                if (e.KeyCode == Keys.Up)
                {
                    if (index > 0)
                    {
                        _order.Gatherskills = swap(_order.Gatherskills, index, index - 1);
                        refreshForm();
                        listBoxGatherSkills.SelectedIndex = index - 1;
                        e.Handled = true;
                    }
                }
            }
        }

        private void chkboxTeleportOnStart_CheckedChanged(object sender, EventArgs e)
        {
            _order.TeleportOnStart.Enabled = chkboxTeleportOnStart.Checked;
        }

        private bool getTeleportCurrentZone(out Order.Teleport teleport, bool enabled)
        {
            var zoneId = WorldManager.ZoneId;
            var aIds = WorldManager.AetheryteIdsForZone(zoneId);
            teleport = new Order.Teleport();
            uint aId;

            if (aIds.Count() == 0)
            {
                MessageBox.Show("There is no aetheryte in this zone.");
                return false;
            }
            else if (aIds.Count() == 1)
            {
                aId = aIds.ElementAt(0).Item1;
            }
            else
            {
                //override id's for cities to avoid listing all mini aetherytes.
                var tempAidCityList = aIds.Where(o => new uint[] { 2, 8, 9, 70 }.Contains(o.Item1));
                if (tempAidCityList.Count() > 0)
                {
                    aId = tempAidCityList.First().Item1;
                }
                else
                {
                    using (var MADialougeForm = new MultiAetheryteIdDialogue(aIds))
                    {
                        MADialougeForm.ShowDialog();
                        aId = MADialougeForm.returnAid;
                        if (aId == uint.MaxValue)
                            return false;
                    }
                }
            }

            string locationName;
            try
            {
                locationName = WorldManager.AvailableLocations.Where(o =>
                    o.AetheryteId == aId).ElementAt(0).Name;
            }
            catch (ArgumentOutOfRangeException) { locationName = string.Empty; }

            teleport.Name = locationName;
            teleport.AetheryteId = aId;
            teleport.ZoneId = zoneId;
            teleport.Enabled = enabled;
            return true;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            Order.Teleport tele;
            if (this.getTeleportCurrentZone(out tele, chkboxTeleportOnStart.Checked))
            {
                _order.TeleportOnStart = tele;
                cbBoxLocationNameOnStart.Text = _order.TeleportOnStart.Name;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Order.Teleport tele;
            if (this.getTeleportCurrentZone(out tele, chkBoxTeleportOnComplete.Checked))
            {
                _order.TeleportOnComplete = tele;
                cbBoxLocationNameOnComplete.Text = _order.TeleportOnComplete.Name;
            }
        }

        private void chkBoxTeleportOnComplete_CheckedChanged(object sender, EventArgs e)
        {
            if (_order.gather.Infinite && chkBoxTeleportOnComplete.Checked)
            {
                MessageBox.Show("Gather quantity hasnt been set, in infinite mode the profile will never finish.");
            }
            _order.TeleportOnComplete.Enabled = chkBoxTeleportOnComplete.Checked;
        }


        private void button7_Click(object sender, EventArgs e)
        {
            string xml = System.Xml.Linq.XElement.Parse(_order.ToXml().OuterXml).ToString();
            var inspectForm = new InspectXmlForm(xml);
            inspectForm.Show();

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                Settings.current.SavePath = fd.SelectedPath;
                Settings.save();
            }
            lblPath.Text = Settings.current.SavePath;

        }

        private void GatherUpForm_Load(object sender, EventArgs e)
        {
            if (Settings.current.DisableBotbaseWarning)
                return;

            if (!ff14bot.TreeRoot.IsRunning)
            {
                MessageBox.Show("Warning\r\nGameobjects might not update properly without a botbase running.");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            using (var iForm = new InventoryForm())
            {
                iForm.ShowDialog();
                string iName = iForm.itemName;
                if (!string.IsNullOrEmpty(iName))
                    txtboxItemNames.Text = iName;
            }
        }

        private void cbBoxLocationNameOnStart_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBoxLocationNameOnStart.SelectedIndex == -1)
                return;

            var teleport = (WorldManager.TeleportLocation)cbBoxLocationNameOnStart.SelectedItem;
            _order.TeleportOnStart.Name = teleport.Name;
            _order.TeleportOnStart.AetheryteId = teleport.AetheryteId;
            _order.TeleportOnStart.ZoneId = teleport.ZoneId;

        }

        private void cbBoxLocationNameOnComplete_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBoxLocationNameOnComplete.SelectedIndex == -1)
                return;

            var teleport = (WorldManager.TeleportLocation)cbBoxLocationNameOnComplete.SelectedItem;
            _order.TeleportOnComplete.Name = teleport.Name;
            _order.TeleportOnComplete.AetheryteId = teleport.AetheryteId;
            _order.TeleportOnComplete.ZoneId = teleport.ZoneId;
        }

        private void btnImportProfile_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Filter = "Orderbot profile (.xml)|*.xml";
            fd.FilterIndex = 1;
            fd.Multiselect = false;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                var orderParser = new OrderParser(fd.FileName);

                if (orderParser.IsValidVersion)
                {
                    string errorMessage;
                    if (!orderParser.ToOrder(out this._order, out errorMessage))
                        MessageBox.Show(errorMessage);
                    this.refreshForm();
                }
                else
                    MessageBox.Show("GatherUp can only import profiles generated with this tool from version: " + GatherUp.version.ToString() + " and below.");

            }
        }

        private async void btnActivate_Click(object sender, EventArgs e)
        {
            string savePath = PluginManager.PluginDirectory + @"\GatherUp\tempProfile.xml";
            string errorMsg = string.Empty;
            if (this.profileHasErrors(out errorMsg))
            {
                MessageBox.Show(errorMsg);
                return;
            }

            _order.ToXml().Save(savePath);

            if (ff14bot.TreeRoot.IsRunning)
            {
                await ff14bot.TreeRoot.StopGently("[GatherUp] Preparing to load new profile.");                
            }

            ff14bot.NeoProfiles.NeoProfileManager.Load(savePath);
            if (BotManager.Current.Name != "Order Bot")
            {
                try
                {
                    BotManager.SetCurrent(BotManager.Bots.FirstOrDefault(r => r.Name == "Order Bot"));
                }
                catch (Exception err)
                {
                    Log.Bot.print("Error while switching botbase: " +err.Message);
                    return;
                }
            }
            ff14bot.TreeRoot.Start();
        }


        private List<T> swap<T>(List<T> list, int a, int b)
        {
            var holder = list.ElementAt(b);
            list[b] = list.ElementAt(a);
            list[a] = holder;
            return list;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            var blackspot = GameObjectManager.LocalPlayer.Location;
            _order.Blackspots.Add(new Order.HotSpot(blackspot, (int)numRadiusBlackSpot.Value));
            refreshForm();
        }

        private void listBoxBlackSpots_KeyDown(object sender, KeyEventArgs e)
        {
            if (listBoxBlackSpots.Focused && listBoxBlackSpots.SelectedIndex != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _order.Blackspots.RemoveAt(listBoxBlackSpots.SelectedIndex);
                    refreshForm();
                    e.Handled = true;
                }
            }
        }

        private void chkboxExGather_CheckedChanged(object sender, EventArgs e)
        {
            _order.gather.exGather.Enabled = chkboxExGather.Checked;
            if(_order.gather.exGather.Enabled)
            {
                if (_order.Blackspots.Count() > 0)
                {
                    MessageBox.Show("Warning: The ExGatherTag does not support blackspots.");
                }
            }
            refreshForm();
        }

        private void chkboxDiscoverUnknowns_CheckedChanged(object sender, EventArgs e)
        {
            _order.gather.exGather.DiscoverUnknowns = chkboxDiscoverUnknowns.Checked;
        }

        private void cbBoxCordialType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBoxCordialType.SelectedIndex != -1)
            {
                _order.gather.exGather.CordialType = (Order.CordialType)cbBoxCordialType.SelectedItem;
                refreshForm();
            }
        }
    }
}