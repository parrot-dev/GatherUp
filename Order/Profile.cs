using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Clio.Utilities;
using GatherUp.Order.Xml;
using ICSharpCode.SharpZipLib.Zip;
using static System.Globalization.CultureInfo;


namespace GatherUp.Order
{
    class Profile
    {
        public enum CordialType { None, Cordial, HiCordial, Auto }

        public string Name { get; set; }
        public bool DisableMount { get; set; } = true;
        public Teleport TeleportOnStart;
        public Teleport TeleportOnComplete;
        public List<HotSpot> Hotspots;
        public List<HotSpot> Blackspots; 
        public List<string> Items;
        public List<string> Gatherskills;
        public Gear gear;
        public Gather gather;
        public string Killradius = "50"; 

        public class Teleport {
           public bool Enabled = false;
           public uint AetheryteId = 0;
           public ushort ZoneId = 0;
           public string Name = "";
        }
       public class Gear {
            public bool Enabled = false;
            public int GearSet = 0;
        }

        public class FlyTo
        {
            public bool Enabled => Destinations != null && Destinations.Any();
            public List<Destination> Destinations { get; set; } = new List<Destination>();
            public class Destination
            {
                public Vector3 Position { get; set; }
                public double AllowedVariance { get; set; } = 0.0f;
                public bool Land { get; set; } = false;
                public string GetXYZ()
                {
                    return string.Format("{0}, {1}, {2}",
                        Position.X.ToString(InvariantCulture),
                        Position.Y.ToString(InvariantCulture),
                        Position.Z.ToString(InvariantCulture));
                }
            }
        }

       public class Gather {
            
            public bool Infinite = true; //gather forever.
            public ExGather exGather = new ExGather();
            public int Quantity = 0;
            public string ItemId;
            public string Target;         
        
           public class ExGather
           {               
               public bool Enabled = false;
               public bool UseCordial { get { return (CordialType != CordialType.None); } }
               public bool DiscoverUnknowns = false;
               public readonly string CordialTime = "Auto";
               public CordialType CordialType = CordialType.None;           
           }
       }

       public class HotSpot
       {
           public Vector3 Coord { get; set; }
           public int Radius { get; set; }
           public FlyTo FlyTo { get; set; }

           public HotSpot(Vector3 coord, int radius) : this(coord, radius, new FlyTo())
            {
           }
           public HotSpot(Vector3 coord, int radius, FlyTo flyTo)
           {
               Coord = coord;
               Radius = radius;
                FlyTo = flyTo;
           }
           public HotSpot(Vector3 coord) : this(coord, 100, new FlyTo())
           {
           }
            public string GetXYZ()
            {
                return string.Format("{0}, {1}, {2}",
                    Coord.X.ToString(InvariantCulture),
                    Coord.Y.ToString(InvariantCulture),
                    Coord.Z.ToString(InvariantCulture));
            }

            public override string ToString()
            {
                return string.Format("{0}, {1}, {2} | {3}",
                    Coord.X.ToString(InvariantCulture),
                    Coord.Y.ToString(InvariantCulture),
                    Coord.Z.ToString(InvariantCulture),
                    Radius.ToString(InvariantCulture));
            }
        }
        

        public Profile()
        {
            Hotspots = new List<HotSpot>();
            Blackspots = new List<HotSpot>();
            Items = new List<string>();
            Gatherskills = new List<string>();
            TeleportOnComplete = new Teleport();
            TeleportOnStart = new Teleport();
            gear = new Gear();
            gather = new Gather();
        }

        public XDocument ToXml()
        {
            return new ProfileTransformer().Transform(this, GatherUp.version);
            
            /*
            XmlDocument doc = new XmlDocument();
                    
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);                      
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);
            
            //profile            
            XmlComment xmlComment = doc.CreateComment("Generated with GatherUp "+GatherUp.version.ToString());
            var xmlProfile = doc.CreateElement("Profile");
            xmlProfile.AppendChild(xmlComment);
            doc.AppendChild(xmlProfile);            

            //Name
            xmlProfile.AppendChild(XmlHelpers.GetTextElement("Name",this.Name, doc));

            //killrad            
            xmlProfile.AppendChild(XmlHelpers.GetTextElement("KillRadius", this.Killradius, doc));

            //order
            var xmlOrder = doc.CreateElement("Profile");
            
            //gearset
            if (gear.Enabled)
            {
                xmlOrder.AppendChild(XmlHelpers.GetLogMessageElement(string.Format("Changing gear to set: {0}", this.gear.GearSet.ToString()), doc));
                var xmlRunCodeGear = doc.CreateElement("RunCode");
                xmlRunCodeGear.SetAttribute("Name", "GearSetChange");
                xmlOrder.AppendChild(xmlRunCodeGear);
                xmlOrder.AppendChild(XmlHelpers.GetWaitTimerElement(3,doc));               
            }

            //teleOnStart
            if(TeleportOnStart.Enabled)
            {
                var xmlIfNotOnMap = XmlHelpers.GetIfElement(string.Format("not IsOnMap({0})", TeleportOnStart.ZoneId.ToString()), doc);
                if (string.IsNullOrEmpty(TeleportOnStart.Name))
                {
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetLogMessageElement(string.Format("Teleporting to Aetheryte id: {0}", this.TeleportOnStart.AetheryteId.ToString()), doc));
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetTeleportToElement(TeleportOnStart.AetheryteId, doc));
                }
                else
                {
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetLogMessageElement(string.Format("Teleporting to {0}", this.TeleportOnStart.Name), doc));
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetTeleportToElement(TeleportOnStart.AetheryteId, TeleportOnStart.Name, doc));
                }
                xmlOrder.AppendChild(xmlIfNotOnMap);
            }
            //Gather
            XmlElement xmlGather = XmlHelpers.GetGatherElement(this.gather, doc);

            //gatherObject
            xmlGather.AppendChild(XmlHelpers.GetTextElement("GatherObject", this.gather.Target, doc));
            
            //hotspots
            xmlGather.AppendChild(XmlHelpers.GetHotSpotsElement(this.Hotspots, doc));

            //blackspots
            if(Blackspots.Count() > 0)
            {
                if(!gather.exGather.Enabled)
                {
                    xmlGather.AppendChild(XmlHelpers.GetBlacSpotsElement(Blackspots, doc));
                }                
            }

            //itemNames
            xmlGather.AppendChild(XmlHelpers.GetItemNamesElement(this.Items, doc));

            //gatheringSkillOrder
            xmlGather.AppendChild(XmlHelpers.GetGatheringSkillOrderElement(this.Gatherskills, doc));

            //gatherEnd
            xmlOrder.AppendChild(xmlGather);

            //teleportOnComplete
            if(TeleportOnComplete.Enabled)
            {                
               var xmlIfNotOnMap = XmlHelpers.GetIfElement(string.Format("not IsOnMap({0})", TeleportOnComplete.ZoneId.ToString()), doc);          
                if (string.IsNullOrEmpty(TeleportOnComplete.Name))
                {
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetLogMessageElement(string.Format("Teleporting to Aetheryte id: {0}", this.TeleportOnComplete.AetheryteId.ToString()), doc));
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetTeleportToElement(TeleportOnComplete.AetheryteId, doc));
                }
                else
                {
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetLogMessageElement(string.Format("Teleporting to {0}", this.TeleportOnComplete.Name), doc));
                    xmlIfNotOnMap.AppendChild(XmlHelpers.GetTeleportToElement(TeleportOnComplete.AetheryteId, TeleportOnComplete.Name, doc));
                }
                xmlOrder.AppendChild(xmlIfNotOnMap);
            }         

            //order end
            xmlProfile.AppendChild(xmlOrder);
                        
            //codechunks
            if (this.gear.Enabled)
            {
                var xmlCodeChunks = doc.CreateElement("CodeChunks");
                var xmlCodeChunk = XmlHelpers.GetCodeChunkElement("GearSetChange", string.Format("ff14bot.Managers.ChatManager.SendChat(\"/gs change {0}\");", gear.GearSet), doc);
                xmlCodeChunks.AppendChild(xmlCodeChunk);
                xmlProfile.AppendChild(xmlCodeChunks);
            }


            return doc;*/

        }
    }

    internal static class XmlHelpers {

        internal static XmlElement GetWaitTimerElement(int seconds, XmlDocument doc)
        {
            var xmlWaitTimer = doc.CreateElement("WaitTimer");
            xmlWaitTimer.SetAttribute("WaitTime", seconds.ToString());
            return xmlWaitTimer;
        }

        internal static XmlElement GetTeleportToElement(uint AetheryteId, string name, XmlDocument doc)
        {
            var xmlTeleportTo = doc.CreateElement("TeleportTo"); 
            xmlTeleportTo.SetAttribute("AetheryteId", AetheryteId.ToString());
            if(!string.IsNullOrEmpty(name))
                xmlTeleportTo.SetAttribute("Name", name);            
            return xmlTeleportTo;
        }
        internal static XmlElement GetTeleportToElement(uint aetheryteId, XmlDocument doc)
        {
             return GetTeleportToElement(aetheryteId, string.Empty, doc);
        }

        internal static XmlElement GetIfElement(string condition, XmlDocument doc)
        {
            var xmlIf = doc.CreateElement("If");
            xmlIf.SetAttribute("Condition", condition);
            return xmlIf;
        }

        internal static XmlElement GetGatherElement(Profile.Gather gather, XmlDocument doc)
        {

            XmlElement xmlGather = gather.exGather.Enabled ? doc.CreateElement("ExGather") : doc.CreateElement("Gather");  
            string condition = gather.Infinite ? "True" : string.Format("ItemCount({0}) < {1}", gather.ItemId, gather.Quantity.ToString());
            xmlGather.SetAttribute("while", condition);

            if(gather.exGather.Enabled)
            {
                if (gather.exGather.DiscoverUnknowns)
                {
                    xmlGather.SetAttribute("DiscoverUnknowns", "True");
                }
                if(gather.exGather.UseCordial)
                {
                    xmlGather.SetAttribute("CordialType", gather.exGather.CordialType.ToString());
                    xmlGather.SetAttribute("CordialTime", gather.exGather.CordialTime);
                }
            }
            return xmlGather;
        }


        internal static XmlElement GetLogMessageElement(string msg, XmlDocument doc)
        {
            var xmlLogMessage = doc.CreateElement("LogMessage");
            xmlLogMessage.SetAttribute("Message", msg);
            return xmlLogMessage;
        }

        internal static XmlElement GetTextElement(string name, string text, XmlDocument doc)
        {
            var xmlElement = doc.CreateElement(name);
            var xmlText = doc.CreateTextNode(text);
            xmlElement.AppendChild(xmlText);
            return xmlElement;
        }

        internal static XmlElement GetHotSpotsElement(List<Profile.HotSpot> hotspots, XmlDocument doc)
        {
            var xmlHotSpots = doc.CreateElement("HotSpots");
            foreach (var hotspot in hotspots)
            {
                var xmlHotSpot = doc.CreateElement("HotSpot");
                xmlHotSpot.SetAttribute("XYZ", hotspot.GetXYZ());
                xmlHotSpot.SetAttribute("Radius", hotspot.Radius.ToString());
                xmlHotSpots.AppendChild(xmlHotSpot);
            }
            return xmlHotSpots;
        }

        internal static XmlElement GetBlacSpotsElement(List<Profile.HotSpot> blacspots, XmlDocument doc)
        {
            var xmlBlackSpots = doc.CreateElement("BlackSpots");
            foreach (var blackspot in blacspots)
            {
                var xmlBlackSpot = doc.CreateElement("BlackSpot");
                xmlBlackSpot.SetAttribute("XYZ", blackspot.GetXYZ());
                xmlBlackSpot.SetAttribute("Radius", blackspot.Radius.ToString());
                xmlBlackSpots.AppendChild(xmlBlackSpot);
            }
            return xmlBlackSpots;
        }

        internal static XmlElement GetCodeChunkElement(string name, string code, XmlDocument doc)
        {
            var xmlCodeChunk = doc.CreateElement("CodeChunk");
            xmlCodeChunk.SetAttribute("Name", name);
            var cdata = doc.CreateCDataSection(code);
            xmlCodeChunk.AppendChild(cdata);
            return xmlCodeChunk;
        }

        internal static XmlElement GetItemNamesElement(List<string> itemNames, XmlDocument doc)
        {
            var xmlItemNames = doc.CreateElement("ItemNames");
            foreach (string itemName in itemNames)
            {
                xmlItemNames.AppendChild(XmlHelpers.GetTextElement("ItemName", itemName, doc));
            }
            return xmlItemNames;
        }

        internal static XmlElement GetGatheringSkillOrderElement(List<string> gatheringSkills, XmlDocument doc)
        {
            var xmlGatheringSkillOrder = doc.CreateElement("GatheringSkillOrder");
            foreach(string gatheringSkill in gatheringSkills) 
            {
                var xmlGatheringSkill = doc.CreateElement("GatheringSkill");
                xmlGatheringSkill.SetAttribute("SpellName", gatheringSkill);
                xmlGatheringSkill.SetAttribute("TimesToCast","1");
                xmlGatheringSkillOrder.AppendChild(xmlGatheringSkill);           
            }
            return xmlGatheringSkillOrder;
        }
    }
}