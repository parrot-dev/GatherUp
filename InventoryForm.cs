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
    public partial class InventoryForm : Form
    {
        private class Item : IEquatable<Item>
        {
            public string name;
            public uint id;
            public Item(string Name, uint Id)
            {
                this.name = Name;
                this.id = Id;
            }
            public override string ToString()
            {
                return string.Format("{0} : {1}", name, id);
            }
            public bool Equals(Item other)
            {
                if (ReferenceEquals(other, null)) 
                    return false;
                if (ReferenceEquals(this, other)) 
                    return true;
                return name.Equals(other.name) && id.Equals(other.id);
            }

            public override int GetHashCode()
            {
                int hashItemName = name == null ? 0 : name.GetHashCode();
                int hashItemId = id.GetHashCode();
                return hashItemName ^ hashItemId;
            }

        }
        private List<Item> itemList;
        public uint itemId = 0;
        public string itemName = string.Empty;
        public InventoryForm()
        {
            InitializeComponent();
            itemList = new List<Item>();            
            foreach(var bagslot in InventoryManager.FilledSlots) 
            {                
                var item = new Item(bagslot.Item.CurrentLocaleName, bagslot.RawItemId);
                itemList.Add(item);
            }

            itemList = itemList.Distinct().ToList();
            itemList.Sort((x, y) => x.name.CompareTo(y.name));            
            
            itemList.Add(new Item("Earth Shard", 5));
            itemList.Add(new Item("Earth Crystal", 11));
            itemList.Add(new Item("Earth Cluster", 17));
            itemList.Add(new Item("Fire Shard", 2));
            itemList.Add(new Item("Fire Crystal", 8));
            itemList.Add(new Item("Fire Cluster", 14));
            itemList.Add(new Item("Ice Shard", 3));
            itemList.Add(new Item("Ice Crystal", 9));
            itemList.Add(new Item("Ice Cluster", 15));            
            itemList.Add(new Item("Lightning Shard",6));
            itemList.Add(new Item("Lightning Crystal", 12));
            itemList.Add(new Item("Lightning Cluster", 18));
            itemList.Add(new Item("Water Shard", 7));
            itemList.Add(new Item("Water Crystal", 13));
            itemList.Add(new Item("Water Cluster", 19));
            itemList.Add(new Item("Wind Shard", 4));
            itemList.Add(new Item("Wind Crystal", 10));
            itemList.Add(new Item("Wind Cluster", 16));     
            
            listBox1.DataSource = itemList; 
           
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex != -1)
            {
                this.itemName = ((Item)listBox1.SelectedItem).name;
                this.itemId = ((Item)listBox1.SelectedItem).id;
                this.Close();
            }
        }

        
    }
}
