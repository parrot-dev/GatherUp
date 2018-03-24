using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatherUp
{
    public partial class MultiAetheryteIdDialogue : Form
    {
        public uint returnAid = uint.MaxValue; //if returned with maxvalue, something failed, ex: user closed the window. todo, replace with better error handling.
        private class Aetheryte
        {
            public readonly uint Id;
            public readonly Clio.Utilities.Vector3 Coord;
            public readonly String LocationName;

            public Aetheryte(uint id, string name, Clio.Utilities.Vector3 coord)
            {
                Id = id;
                Coord = coord;
                LocationName = name;
            }

            public override string ToString()
            {
                return String.Format("{0} : {1}", this.Id.ToString(), this.LocationName);
            }

        }
        public MultiAetheryteIdDialogue(Tuple<uint, Clio.Utilities.Vector3>[] aIdArr)
        {
            InitializeComponent();
            var aetheryteList = new List<Aetheryte>();
            foreach(var aId in aIdArr)
            {
                uint id = aId.Item1;                
                var coord = aId.Item2;
                string name;
                try
                {
                    var locationNameList = ff14bot.Managers.WorldManager.AvailableLocations.Where(o => o.AetheryteId == id);
                    name = (locationNameList.Count() > 0) ? locationNameList.First().Name : "Location Name not found";
                }
                catch (Exception err) {
                    MessageBox.Show(err.InnerException + "\r\n" + err.Message); //debug
                    name = string.Empty; 
                }
                aetheryteList.Add(new Aetheryte(id, name, coord));
            }
            listBox1.DataSource = aetheryteList;
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Select an Aetheryte");
                return;
            }
            uint aId;
            try
            {
                aId = ((Aetheryte)listBox1.SelectedItem).Id;
            }
            catch (Exception err) { MessageBox.Show(err.Message); return; }
            returnAid = aId;
            this.Close();
        }

        private void MultiAetheryteIdDialogue_Load(object sender, EventArgs e)
        {

        }

    }
}
