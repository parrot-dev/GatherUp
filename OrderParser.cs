using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GatherUp
{
    class OrderParser
    {
        /// <summary>
        /// Parse orderbot xml into an Order object.
        /// </summary>
        /// <param name="Path">File path to orderbot profile</param>
        public OrderParser(string path)
        {
            try
            {
                this._xRoot = XDocument.Load(path);
            }
            catch (Exception e) {
                System.Windows.Forms.MessageBox.Show(e.Message);
                this._xRoot = new XDocument(); }
            this.version = this.getVersion();
        }

        private readonly XDocument _xRoot;
        private ErrorMessages _errorMessages;
        /// <summary>
        /// Version of gatherup the profile was generated with. 0.0.0 for unknown.
        /// </summary>
        public readonly Version version;
        public bool IsValidVersion
        {
            get {return (this.version.CompareTo(new Version(0, 0, 0)) > 0 && this.version.CompareTo(GatherUp.version) <= 0); }
        }

        /// <summary>
        /// Tries to parse the profile xml into an Order object.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="errorMessage"></param>
        /// <param name="ignoreVersion"></param>
        /// <returns>Returns false on errors and warnings.</returns>
        public bool ToOrder(out Order order, out string errMsg, bool ignoreVersion = false)
        {
            this._errorMessages = new ErrorMessages();   
            if (this.IsValidVersion || ignoreVersion)
            {
                order = this.ParseGatherUpXml();
                errMsg = this._errorMessages.ToString();
                return this._errorMessages.isEmpty;
            }

            order = new Order();
            errMsg = "Invalid version.";
            return false;
        }

        /// <summary>
        /// Get the version of gatherup the profile was generated with.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>0.0.0 for unknown. </returns>
        private Version getVersion()
        {
            Version retVersion;
            var xComments = _xRoot.DescendantNodes().OfType<XComment>().Where(c => c.Value.Contains("GatherUp"));
            if (xComments.Any())
            {
                string comment = xComments.First().ToString();
                var match = new Regex(@"(\d\.\d\.\d|\d\.\d)").Match(comment);
                if (match.Success)
                {
                    try
                    {
                        retVersion = new Version(match.Groups[1].Value);
                    }
                    catch (Exception err)
                    {
                        Log.Bot.print("getVersion() Failed. Error:\r\n" + err.Message);
                        return new Version(0, 0, 0);
                    }
                    return retVersion;
                }
            }
            return new Version(0, 0, 0);
        }
        private Order ParseGatherUpXml()
        {
            var order = new Order();
            order.name = this.getName();
            order.Killradius = this.getKillRadius();
            order.gear = this.GetGearSetChange();
            order.TeleportOnStart = this.getTeleportOnStart();
            order.TeleportOnComplete = this.getTeleportOnComplete();
            order.gather = this.getGather();
            order.Hotspots = this.GetHotSpots();
            order.Blackspots = this.GetBlackSpots();
            order.Gatherskills = this.getGatheringSkills();
            order.Items = this.getItemNames();         
            return order;
        }

        private string getName()
        {
            var xNames = _xRoot.Descendants("Name");
            if (xNames.Count() > 0)
                return xNames.First().Value;
            else
            {
                this._errorMessages.add("Couldn't find Name element");
                return string.Empty;
            }
            
        }
        private string getKillRadius()
        {
            var xKillRadius = _xRoot.Descendants("KillRadius");
            if (xKillRadius.Count() > 0)
                return xKillRadius.First().Value;
            else
            {
                this._errorMessages.add("Couldn't find killradius element. Setting to 50.");
                return "50";
            }            
        }

        private List<string> getGatheringSkills()
        {
            var xGatheringSkills = this._xRoot.Descendants("GatheringSkill");
            var gatheringSkillList = new List<string>();
            foreach(XElement xGatheringSkill in xGatheringSkills)
            {
                string gSkill;
                if(this.tryGetGatheringSkill(xGatheringSkill, out gSkill))
                {
                    gatheringSkillList.Add(gSkill);
                }
            }

            if (!gatheringSkillList.Any())
                this._errorMessages.add("Warning: no gathering skills were found.");
            return gatheringSkillList;
        }
        private bool tryGetGatheringSkill(XElement xGatheringSkill, out string gatheringSkill)
        {
            if((string)xGatheringSkill.Attribute("SpellName") != null)
            {
                gatheringSkill = xGatheringSkill.Attribute("SpellName").Value;
                return true;
            }
            this._errorMessages.add("Couldn't find SpellName attribute in GatheringSkill tag. Skipping. Line: " + xGatheringSkill.ToString());
            gatheringSkill = string.Empty;
            return false;
        }

        private List<string> getItemNames()
        {
            var xItemNames = this._xRoot.Descendants("ItemName");
            if (!xItemNames.Any())
                this._errorMessages.add("Warning, no items to gather were found");
            var itemNames = new List<string>();

            foreach(var xItemName in xItemNames)            
                itemNames.Add(xItemName.Value);
            
            return itemNames;
        }

        private Order.Teleport getTeleportOnStart()
        {
            foreach (var xElement in this._xRoot.Descendants("TeleportTo"))
            {
                if (xElement.Parent.ElementsAfterSelf("Gather").Any()
                    || xElement.Parent.ElementsAfterSelf("ExGather").Any())                
                    return getTeleport(xElement);
            }
            return new Order.Teleport();
        }
        private Order.Teleport getTeleportOnComplete()
        {
            foreach (var xElement in this._xRoot.Descendants("TeleportTo"))
            {
                if (xElement.Parent.ElementsBeforeSelf("Gather").Any()
                    || xElement.Parent.ElementsBeforeSelf("ExGather").Any())
                    return getTeleport(xElement);
            }
            return new Order.Teleport();
        }

        private Order.Teleport getTeleport(XElement xElement)
        {
            if (xElement.Parent.Name.LocalName == "If" && xElement.Parent.FirstAttribute.Name.LocalName == "Condition")
            {
                ushort zoneId = 0;
                uint aetheryteId = 0;
                string name = String.Empty;

                //aetheryte id
                try
                {
                    aetheryteId = uint.Parse(xElement.Attribute("AetheryteId").Value);
                }
                catch (Exception)
                {
                    this._errorMessages.add("TeleportTag Parse failed. Skipping the tag. Line: " + xElement.ToString());
                    return new Order.Teleport();
                }

                //name
                if ((string)xElement.Attribute("Name") != null)
                    name = xElement.Attribute("Name").Value;

                //mapId
                var match = new Regex(@"not IsOnMap\((\d+)\)").Match(xElement.Parent.Attribute("Condition").Value);
                if (!match.Success || !ushort.TryParse(match.Groups[1].Value, out zoneId))
                {
                    this._errorMessages.add("Failed parsing zoneId for teleport. Skipping the tag. Line: " + xElement.ToString());
                    return new Order.Teleport();
                }

                var teleport = new Order.Teleport();
                teleport.AetheryteId = aetheryteId;
                teleport.Enabled = true;
                teleport.ZoneId = zoneId;
                teleport.Name = name;
                return teleport;

            }
            else
            {
                this._errorMessages.add("Expected If condition before TeleportTo tag wasnt found. Skipping the tag. Line: " + xElement.ToString());
                return new Order.Teleport();
            }
         
        }

        private Order.Gather getGather()
        {
            var gather = new Order.Gather();
            XElement xGather;
           
           try
            {
                xGather = _xRoot.Descendants().Single(e => e.Name == "Gather" || e.Name == "ExGather");
            }
            catch (Exception)
            {
                this._errorMessages.add("Multiple Gather tags are not supported. Gather tag and target set to default values");
                return new Order.Gather();
            }
            gather.exGather.Enabled = (xGather.Name == "ExGather");
            this.TryGetGatherWhileCondition(xGather, ref gather);
            this.tryGetGatherTarget(xGather, ref gather);
            if ((string)xGather.Attribute("DiscoverUnknowns") != null)
            {
                gather.exGather.DiscoverUnknowns = (xGather.Attribute("DiscoverUnknowns").Value == "True");
            }

            if ((string)xGather.Attribute("CordialType") != null)
            {
                try
                {
                    gather.exGather.CordialType = (Order.CordialType)Enum.Parse(typeof(Order.CordialType), xGather.Attribute("CordialType").Value);
                }
                catch (Exception) { _errorMessages.add("Failed parsing CordialType. Disabling."); }
            }
            return gather;
        }

        private bool tryGetGatherTarget(XElement xGather, ref Order.Gather gather)
        {
            var xGatherObjects = xGather.Descendants("GatherObject");
            if (xGatherObjects.Count() > 0)
            {
                gather.Target = xGatherObjects.First().Value;
                return true;
            }
            else
            {
                this._errorMessages.add("GatherObject tag not found");
                return false;
            }             
        }

        /// <summary>
        /// sets gather: infinite, itemdId and quantity.
        /// </summary>
        /// <returns></returns>
        private bool TryGetGatherWhileCondition(XElement xGather, ref Order.Gather gather)
        {
            if ((string)xGather.Attribute("while") != null)
            {
                if (xGather.Attribute("while").Value == "True")
                {
                    gather.Infinite = true;
                    return true;
                }
            }
            else
            {
                this._errorMessages.add("Missing while condition in gather tag");
            }

            var match = new Regex(@"ItemCount\((\d+)\) .* (\d+)").Match(xGather.Attribute("while").Value);
            if (match.Success)
            {
                gather.ItemId = match.Groups[1].Value;
                int quantity;
                if (Int32.TryParse(match.Groups[2].Value, out quantity))
                {
                    gather.Infinite = false;
                    gather.Quantity = quantity;
                    return true;
                }
                else
                {
                    this._errorMessages.add("Quantity parse failed.");
                }
            }
            else
            {
                this._errorMessages.add("Failed to parse gather while condition. Line: " + xGather.ToString());
            }
            gather.Infinite = true;
            return false;
        }

        private List<Order.HotSpot> GetHotSpots()
        {
            var xHotSpots = this._xRoot.Descendants("HotSpot");
            var hotSpots = new List<Order.HotSpot>();
            foreach(var xHotSpot in xHotSpots)
            {
                Order.HotSpot hotSpot;
                if(this.TryGetHotSpot(xHotSpot, out hotSpot))
                {
                    hotSpots.Add(hotSpot);
                }
            }
            return hotSpots;
        }
        private bool TryGetHotSpot(XElement xHotSpot, out Order.HotSpot hotSpot)
        {
            string xyz = string.Empty;
            if((string)xHotSpot.Attribute("XYZ") != null)
            {
                xyz = xHotSpot.Attribute("XYZ").Value;
            }
            else
            {
                this._errorMessages.add("Couldnt find XYZ attribute in hotspot tag. Skipping. line:" + xHotSpot.ToString());
                hotSpot = new Order.HotSpot(new Clio.Utilities.Vector3());
                return false;
            }
            int radius = 0;
            if ((string)xHotSpot.Attribute("Radius") != null)
            {
                try{
                    radius = Int32.Parse(xHotSpot.Attribute("Radius").Value);
                }catch(Exception){
                    this._errorMessages.add("Radius attribute parse failed. setting to 100. Line: "+xHotSpot.ToString());
                    radius = 100;
                }   
                hotSpot = new Order.HotSpot(new Clio.Utilities.Vector3(xyz), radius);
            }
            else
            {
                this._errorMessages.add("Radius attribute not found in hotspot tag. Setting to 100. line: "+xHotSpot.ToString());
                hotSpot = new Order.HotSpot(new Clio.Utilities.Vector3(xyz));
            }            
            
            return true;
          
        }

        private List<Order.HotSpot> GetBlackSpots()
        {
            var xBlackSpots = this._xRoot.Descendants("BlackSpot");
            var BlackSpots = new List<Order.HotSpot>();
            foreach (var xBlackSpot in xBlackSpots)
            {
                Order.HotSpot blackSpot;
                if (this.TryGetBlackSpot(xBlackSpot, out blackSpot))
                {
                    BlackSpots.Add(blackSpot);
                }
            }
            return BlackSpots;
        }

        private bool TryGetBlackSpot(XElement xBlackSpot, out Order.HotSpot blackSpot)
        {
            string xyz = string.Empty;
            if ((string)xBlackSpot.Attribute("XYZ") != null)
            {
                xyz = xBlackSpot.Attribute("XYZ").Value;
            }
            else
            {
                this._errorMessages.add("Couldnt find XYZ attribute in blackspot tag. Skipping. line:" + xBlackSpot.ToString());
                blackSpot = new Order.HotSpot(new Clio.Utilities.Vector3());
                return false;
            }
            int radius = 0;
            if ((string)xBlackSpot.Attribute("Radius") != null)
            {
                try
                {
                    radius = Int32.Parse(xBlackSpot.Attribute("Radius").Value);
                }
                catch (Exception)
                {
                    this._errorMessages.add("Radius attribute parse failed. setting to 10. Line: " + xBlackSpot.ToString());
                    radius = 10;
                }
                blackSpot = new Order.HotSpot(new Clio.Utilities.Vector3(xyz), radius);
            }
            else
            {
                this._errorMessages.add("Radius attribute not found in Blackspot tag. Setting to 100. line: " + xBlackSpot.ToString());
                blackSpot = new Order.HotSpot(new Clio.Utilities.Vector3(xyz));
            }

            return true;

        }
        
        private Order.Gear GetGearSetChange()
        {
            var xGearSetChangeElements = this._xRoot.DescendantNodes().Where(n =>
                n.NodeType == XmlNodeType.CDATA &&
                n.Parent.Name == "CodeChunk" &&
                n.Parent.Attribute("Name").Value == "GearSetChange");

            var gearSet = new Order.Gear();
            if(xGearSetChangeElements.Count() > 0)
            {
                string cdata = xGearSetChangeElements.First().ToString();
                var match = new Regex("ff14bot\\.Managers\\.ChatManager\\.SendChat\\(\"\\/gs change (\\d+)").Match(cdata);
                if(match.Success)
                {
                    int gearSetNr;
                    if(Int32.TryParse(match.Groups[1].Value, out gearSetNr))
                    {
                        gearSet.Enabled = true;
                        gearSet.GearSet = gearSetNr;
                        return gearSet;
                    }
                    else
                    {
                        this._errorMessages.add("The profile appears to have a specified gearset but the parsing failed. Gear change: disabled.");
                        return gearSet;
                    }
                }                
            }

            return gearSet;           

        }

        private class ErrorMessages
        {

            private List<string> _messages = new List<string>();
            public List<string> messages { get { return _messages; } set { } }
            public bool isEmpty { get { return (_messages.Count() == 0); } set { } }
            public void add(string msg) { _messages.Add(msg); }

            /// <summary>
            /// Combines the error messages into a string.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                for (int i = 0; i < _messages.Count(); i++)
                    stringBuilder.AppendFormat("{0}. {1}\r\n", i + 1, _messages[i]);
                return stringBuilder.ToString().TrimEnd();

            }
        }
    }
}
