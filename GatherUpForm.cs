using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ff14bot.Managers;
using GatherUp.Order.Parsing;
using GatherUp.Order.Parsing.Exceptions;
using Profile = GatherUp.Order.Profile;


namespace GatherUp
{
    public partial class GatherUpForm : Form
    {
        private Profile _profile;

        public GatherUpForm()
        {
            InitializeComponent();
            _profile = new Profile();

            Text = "GatherUp - " + GatherUp.version.ToString();
            numRadius.Value = new Profile.HotSpot(new Clio.Utilities.Vector3()).Radius;
            numRadiusBlackSpot.Value = 10;
            lblPath.Text = Settings.Current.SavePath;

            //Gatheringskills 
            foreach (var spell in DataManager.SpellCache.Values.Where(o =>
                o.Job == ff14bot.Enums.ClassJobType.Miner || o.Job == ff14bot.Enums.ClassJobType.Botanist))
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

            cbBoxCordialType.Items.Add(Profile.CordialType.None);
            cbBoxCordialType.Items.Add(Profile.CordialType.Cordial);
            cbBoxCordialType.Items.Add(Profile.CordialType.HiCordial);
            cbBoxCordialType.Items.Add(Profile.CordialType.Auto);
            cbBoxCordialType.SelectedIndex = 0;

            this.refreshForm();
        }


        private void refreshForm()
        {
            //teleport
            chkboxTeleportOnStart.Checked = _profile.TeleportOnStart.Enabled;
            cbBoxLocationNameOnStart.Text = _profile.TeleportOnStart.Name;
            chkBoxTeleportOnComplete.Checked = _profile.TeleportOnComplete.Enabled;
            cbBoxLocationNameOnComplete.Text = _profile.TeleportOnComplete.Name;

            //General
            txtboxName.Text = _profile.Name;
            txtBoxTarget.Text = _profile.gather.Target;
            txtboxItemId.Text = _profile.gather.ItemId;
            chkBoxQuantity.Checked = !_profile.gather.Infinite;
            numQuantity.Value = _profile.gather.Quantity;
            chkBoxHq.Checked = _profile.gather.Hq;

            if (_profile.gather.Infinite)
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

            chkBoxGearSet.Checked = _profile.gear.Enabled;
            numGearSet.Value = _profile.gear.GearSet;

            //items:

            txtboxItemNames.Text = string.Empty;
            listBox2Items.DataSource = null;
            listBox2Items.DataSource = _profile.Items;


            //hotspots
            listBoxHotSpots.DataSource = null;
            listBoxHotSpots.DataSource = _profile.Hotspots;
            //blackspots
            listBoxBlackSpots.DataSource = null;
            listBoxBlackSpots.DataSource = _profile.Blackspots;
            if (_profile.gather.exGather.Enabled)
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
            listBoxGatherSkills.DataSource = _profile.Gatherskills;

            //Exgather
            chkboxExGather.Checked = _profile.gather.exGather.Enabled;
            chkboxDiscoverUnknowns.Enabled = _profile.gather.exGather.Enabled;
            chkboxDiscoverUnknowns.Checked = _profile.gather.exGather.DiscoverUnknowns;
            cbBoxCordialType.SelectedItem = _profile.gather.exGather.CordialType;
            cbBoxCordialType.Enabled = _profile.gather.exGather.Enabled;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _profile.Items.Add(txtboxItemNames.Text);
            listBox2Items.DataSource = null;
            listBox2Items.DataSource = _profile.Items;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            _profile.gather.ItemId = txtboxItemId.Text;
        }

        private bool profileHasErrors(out string errorMsg)
        {
            errorMsg = string.Empty;
            if (!_profile.gather.Infinite && string.IsNullOrEmpty(_profile.gather.ItemId))
            {
                errorMsg = "Item Id hasn't been specified";
                return true;
            }

            if (string.IsNullOrEmpty(_profile.Name))
            {
                errorMsg = "Profile hasn't got a Name.";
                return true;
            }

            if (string.IsNullOrEmpty(_profile.gather.Target))
            {
                errorMsg = "Target node hasn't been specified.";
                return true;
            }

            if (_profile.Items.Count() == 0)
            {
                errorMsg = "No items to gather have been specified";
                return true;
            }

            if (_profile.Hotspots.Count() == 0)
            {
                errorMsg = "Profile hasn't got any hotspots";
                return true;
            }

            if (_profile.gear.Enabled && _profile.gear.GearSet < 1)
            {
                errorMsg = "Gearset must be larger than 0";
                return true;
            }
            if (_profile.gear.Enabled && _profile.gear.GearSet > GearsetManager.GearsetLimit)
            {
                errorMsg = "invalid Gearset";
                return true;
            }

            return false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string errorMsg = string.Empty;
            if (profileHasErrors(out errorMsg))
            {
                MessageBox.Show(errorMsg);
                return;
            }

            if (_profile.gather.exGather.Enabled && _profile.Blackspots.Count() > 0)
            {
                MessageBox.Show("Warning: All blackspots will be omitted.");
            }

            string fullSavePath = Settings.Current.SavePath + string.Format("\\{0}.xml", _profile.Name);
            if (System.IO.File.Exists(fullSavePath))
            {
                var confirmResult = MessageBox.Show(
                    "A file with this Name already exists.\r\nDo you want to overwrite it?",
                    "Overwrite?",
                    MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.No)
                    return;
            }

            var xmlOrder = _profile.ToXml();
            xmlOrder.Save(fullSavePath);
        }

        private void txtboxName_TextChanged(object sender, EventArgs e)
        {
            _profile.Name = txtboxName.Text;
        }

        private void txtBoxTarget_TextChanged(object sender, EventArgs e)
        {
            _profile.gather.Target = txtBoxTarget.Text;
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
                    _profile.gather.Target = target.Name;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
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
                    _profile.gather.ItemId = itemId.ToString();
                    txtboxItemId.Text = _profile.gather.ItemId;
                    if (!_profile.Items.Contains(itemName))
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

            _profile.gather.Infinite = !chkBoxQuantity.Checked;
        }

        private void numQuantity_ValueChanged(object sender, EventArgs e)
        {
            _profile.gather.Quantity = (int) numQuantity.Value;
        }

        private void chkBoxGearSet_CheckedChanged(object sender, EventArgs e)
        {
            _profile.gear.Enabled = chkBoxGearSet.Checked;
        }

        private void numGearSet_ValueChanged(object sender, EventArgs e)
        {
            _profile.gear.GearSet = (int) numGearSet.Value;
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBox2Items.SelectedIndex;
            if (listBox2Items.Focused && index != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _profile.Items.RemoveAt(listBox2Items.SelectedIndex);
                    listBox2Items.DataSource = null;
                    listBox2Items.DataSource = _profile.Items;
                    e.Handled = true;
                }

                if (e.KeyCode == Keys.Down)
                {
                    if (index != _profile.Items.Count() - 1)
                    {
                        _profile.Items = swap(_profile.Items, index, index + 1);
                        refreshForm();
                        listBox2Items.SelectedIndex = index + 1;
                        e.Handled = true;
                    }
                }

                if (e.KeyCode == Keys.Up)
                {
                    if (index > 0)
                    {
                        _profile.Items = swap(_profile.Items, index, index - 1);
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
            _profile.Hotspots.Add(new Profile.HotSpot(hotspot, (int) numRadius.Value));
            listBoxHotSpots.DataSource = null;
            listBoxHotSpots.DataSource = _profile.Hotspots;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBoxHotSpots.SelectedIndex;
            if (listBoxHotSpots.Focused && index != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _profile.Hotspots.RemoveAt(listBoxHotSpots.SelectedIndex);
                    listBoxHotSpots.DataSource = null;
                    listBoxHotSpots.DataSource = _profile.Hotspots;
                    e.Handled = true;
                }

                if (e.KeyCode == Keys.Down)
                {
                    if (index != _profile.Hotspots.Count() - 1)
                    {
                        _profile.Hotspots = swap(_profile.Hotspots, index, index + 1);
                        refreshForm();
                        listBoxHotSpots.SelectedIndex = index + 1;
                        e.Handled = true;
                    }
                }

                if (e.KeyCode == Keys.Up)
                {
                    if (index > 0)
                    {
                        _profile.Hotspots = swap(_profile.Hotspots, index, index - 1);
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
                _profile.Gatherskills.Add(cbBoxGatheringSkills.SelectedItem.ToString());
                listBoxGatherSkills.DataSource = null;
                listBoxGatherSkills.DataSource = _profile.Gatherskills;
            }
        }

        private void listBox3_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBoxGatherSkills.SelectedIndex;
            if (listBoxGatherSkills.Focused && index != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _profile.Gatherskills.RemoveAt(listBoxGatherSkills.SelectedIndex);
                    listBoxGatherSkills.DataSource = null;
                    listBoxGatherSkills.DataSource = _profile.Gatherskills;
                    e.Handled = true;
                }

                if (e.KeyCode == Keys.Down)
                {
                    if (index != _profile.Gatherskills.Count() - 1)
                    {
                        _profile.Gatherskills = swap(_profile.Gatherskills, index, index + 1);
                        refreshForm();
                        listBoxGatherSkills.SelectedIndex = index + 1;
                        e.Handled = true;
                    }
                }

                if (e.KeyCode == Keys.Up)
                {
                    if (index > 0)
                    {
                        _profile.Gatherskills = swap(_profile.Gatherskills, index, index - 1);
                        refreshForm();
                        listBoxGatherSkills.SelectedIndex = index - 1;
                        e.Handled = true;
                    }
                }
            }
        }

        private void chkboxTeleportOnStart_CheckedChanged(object sender, EventArgs e)
        {
            _profile.TeleportOnStart.Enabled = chkboxTeleportOnStart.Checked;
        }

        private bool getTeleportCurrentZone(out Profile.Teleport teleport, bool enabled)
        {
            var zoneId = WorldManager.ZoneId;
            var aIds = WorldManager.AetheryteIdsForZone(zoneId);
            teleport = new Profile.Teleport();
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
                var tempAidCityList = aIds.Where(o => new uint[] {2, 8, 9, 70}.Contains(o.Item1));
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
            catch (ArgumentOutOfRangeException)
            {
                locationName = string.Empty;
            }

            teleport.Name = locationName;
            teleport.AetheryteId = aId;
            teleport.ZoneId = zoneId;
            teleport.Enabled = enabled;
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Profile.Teleport tele;
            if (this.getTeleportCurrentZone(out tele, chkboxTeleportOnStart.Checked))
            {
                _profile.TeleportOnStart = tele;
                cbBoxLocationNameOnStart.Text = _profile.TeleportOnStart.Name;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Profile.Teleport tele;
            if (this.getTeleportCurrentZone(out tele, chkBoxTeleportOnComplete.Checked))
            {
                _profile.TeleportOnComplete = tele;
                cbBoxLocationNameOnComplete.Text = _profile.TeleportOnComplete.Name;
            }
        }

        private void chkBoxTeleportOnComplete_CheckedChanged(object sender, EventArgs e)
        {
            if (_profile.gather.Infinite && chkBoxTeleportOnComplete.Checked)
            {
                MessageBox.Show(@"Teleport on complete is enabled but the profile is set to gather indefinitely");
            }

            _profile.TeleportOnComplete.Enabled = chkBoxTeleportOnComplete.Checked;
        }


        private void button7_Click(object sender, EventArgs e)
        {
            string xml = _profile.ToXml().ToString();
            var inspectForm = new InspectXmlForm(xml);
            inspectForm.Show();
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                Settings.Current.SavePath = fd.SelectedPath;
                Settings.Save();
            }

            lblPath.Text = Settings.Current.SavePath;
        }

        private void GatherUpForm_Load(object sender, EventArgs e)
        {
            if (Settings.Current.DisableBotbaseWarning)
                return;

            if (!ff14bot.TreeRoot.IsRunning)
            {
                MessageBox.Show("Warning\r\nGameobjects might not update properly without a botbase running.");
                Settings.Current.DisableBotbaseWarning = true;
                Settings.Save();
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

            var teleport = (WorldManager.TeleportLocation) cbBoxLocationNameOnStart.SelectedItem;
            _profile.TeleportOnStart.Name = teleport.Name;
            _profile.TeleportOnStart.AetheryteId = teleport.AetheryteId;
            _profile.TeleportOnStart.ZoneId = teleport.ZoneId;
        }

        private void cbBoxLocationNameOnComplete_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBoxLocationNameOnComplete.SelectedIndex == -1)
                return;

            var teleport = (WorldManager.TeleportLocation) cbBoxLocationNameOnComplete.SelectedItem;
            _profile.TeleportOnComplete.Name = teleport.Name;
            _profile.TeleportOnComplete.AetheryteId = teleport.AetheryteId;
            _profile.TeleportOnComplete.ZoneId = teleport.ZoneId;
        }

        private void btnImportProfile_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog
            {
                Filter = "Orderbot profile (.xml)|*.xml",
                FilterIndex = 1,
                Multiselect = false
            };

            if (fd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var profileParser = new ProfileParser(fd.FileName);
                _profile = profileParser.ToProfile();
                refreshForm();
            }
            catch (NoParserException)
            {
                MessageBox.Show($"GatherUp can only import profiles generated with this tool from version: {GatherUp.version} and below.");
            }
            catch (ParsingException err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private async void btnActivate_Click(object sender, EventArgs e)
        {
            var savePath = PluginManager.PluginDirectory + @"\GatherUp\tempProfile.xml";
            var errorMsg = string.Empty;
            if (this.profileHasErrors(out errorMsg))
            {
                MessageBox.Show(errorMsg);
                return;
            }

            _profile.ToXml().Save(savePath);

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
                    Log.Bot.print("Error while switching botbase: " + err.Message);
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
            _profile.Blackspots.Add(new Profile.HotSpot(blackspot, (int) numRadiusBlackSpot.Value));
            refreshForm();
        }

        private void listBoxBlackSpots_KeyDown(object sender, KeyEventArgs e)
        {
            if (listBoxBlackSpots.Focused && listBoxBlackSpots.SelectedIndex != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    _profile.Blackspots.RemoveAt(listBoxBlackSpots.SelectedIndex);
                    refreshForm();
                    e.Handled = true;
                }
            }
        }

        private void chkboxExGather_CheckedChanged(object sender, EventArgs e)
        {
            _profile.gather.exGather.Enabled = chkboxExGather.Checked;
            if (_profile.gather.exGather.Enabled)
            {
                if (_profile.Blackspots.Count() > 0)
                {
                    MessageBox.Show("Warning: The ExGatherTag does not support blackspots.");
                }
            }

            refreshForm();
        }

        private void chkboxDiscoverUnknowns_CheckedChanged(object sender, EventArgs e)
        {
            _profile.gather.exGather.DiscoverUnknowns = chkboxDiscoverUnknowns.Checked;
        }

        private void cbBoxCordialType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBoxCordialType.SelectedIndex != -1)
            {
                _profile.gather.exGather.CordialType = (Profile.CordialType) cbBoxCordialType.SelectedItem;
                refreshForm();
            }
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
          

        }

        private void button12_Click(object sender, EventArgs e)
        {
            ShowHotSpotOptions();
        }

        private void ShowHotSpotOptions()
        {
            if (!(listBoxHotSpots.SelectedItem is Profile.HotSpot hotspot)) return;
            using (var hotspotForm = new HotSpotForm(hotspot))
            {
                hotspotForm.ShowDialog();
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            foreach (var hotspot in _profile.Hotspots)
            {
                if (!hotspot.FlyTo.Destinations.Any(dest => dest.Position.Equals(hotspot.Coord)))
                {
                    hotspot.FlyTo.Destinations.Add(new Profile.FlyTo.Destination
                    {
                        Position = hotspot.Coord
                    });
                }
            }
        }

        private void listBoxHotSpots_DoubleClick(object sender, EventArgs e)
        {
            ShowHotSpotOptions();
        }

        private void chkBoxHq_CheckedChanged(object sender, EventArgs e)
        {
            _profile.gather.Hq = chkBoxHq.Checked;
        }
    }
}