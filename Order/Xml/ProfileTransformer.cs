using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GatherUp.Order.Xml
{
    class ProfileTransformer
    {
        public XDocument Transform(Profile profile, Version gatherUpVersion)
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                GetProfileElement(profile, gatherUpVersion)
            );
        }


        private static XElement GetProfileElement(Profile profile, Version gatherUpVersion)
        {
            var xProfile = new XElement("Profile");
            xProfile.Add(new XComment($"Generated with GatherUp {gatherUpVersion}"));
            xProfile.Add(new XElement("Name", new XText(profile.name)));
            xProfile.Add(new XElement("KillRadius", new XText(profile.Killradius)));
            xProfile.Add(GetOrderElement(profile));
            xProfile.Add(GetCodeChunks(profile));
            return xProfile;
        }


        private static XElement GetOrderElement(Profile profile)
        {
            var order = new XElement("Order");
            AddGearChangeElement(profile.gear, ref order);
            order.Add(GetTeleportElement(profile.TeleportOnStart));
            var outerLoop = GetOuterLoop(profile);
            foreach (var hotspot in profile.Hotspots)
            {
                outerLoop.Add(GetGatherChunk(profile, hotspot));
            }
            order.Add(outerLoop);
            order.Add(GetTeleportElement(profile.TeleportOnComplete));

            return order;
        }

        private static XElement GetGatherChunk(Profile profile, Profile.HotSpot hotSpot)
        {
            var xGather = GetGatherElement(profile.gather);
            xGather.Add(new XElement("GatherObject", profile.gather.Target));
            xGather.Add(new XElement("HotSpots", new XElement("HotSpot", new XAttribute("XYZ", hotSpot.GetXYZ()), new XAttribute("Radius", hotSpot.Radius))));
            if (!profile.gather.exGather.Enabled)
            {
                xGather.Add(new XElement("BlackSpots", 
                    profile.Blackspots.Select(bs => new XElement("BlackSpot", 
                        new XAttribute("XYZ", bs.GetXYZ()), 
                        new XAttribute("Radius", bs.Radius)
                    ))));
            }
            xGather.Add(new XElement("ItemNames", profile.Items.Select(item => new XElement("ItemName", new XText(item)))));
            xGather.Add(new XElement("GatheringSkillOrder", profile.Gatherskills.Select(gSkill =>
                new XElement("GatheringSkill", new XAttribute("TimesToCast", 1), new XAttribute("SpellName", gSkill)))));

            return xGather;

        }

        private static XElement GetGatherElement(Profile.Gather gather)
        {
            if (gather.exGather.Enabled)
            {
                object[] attributes = {
                    gather.exGather.UseCordial ? new XAttribute("CordialTime", gather.exGather.CordialTime) : null,
                    gather.exGather.UseCordial ? new XAttribute("CordialType", gather.exGather.CordialType) : null,
                    new XAttribute("DiscoverUnknowns", gather.exGather.DiscoverUnknowns)
                };
                return new XElement("ExGather", attributes);
                
            }
            return new XElement("Gather");
        }

        private static void AddGearChangeElement(Profile.Gear gear, ref XElement parent)
        {
            if (!gear.Enabled) return;
            parent.Add(GetLogMessageElement($"Using gearset: {gear.GearSet}"),
                new XElement("RunCode", new XAttribute("Name", "GearSetChange")));
        }

        private static XElement GetTeleportElement(Profile.Teleport teleport)
        {
            if (!teleport.Enabled) return null;
            var xIfElement = GetIfElement($"not IsOnMap({teleport.ZoneId})");
            xIfElement.Add(GetLogMessageElement($"Teleporting to {teleport.Name}"));
            xIfElement.Add(new XElement("TeleportTo", new XAttribute("Name", teleport.Name),
                new XAttribute("AetheryteId", teleport.AetheryteId)));
            return xIfElement;
        }


       

        private static XElement GetOuterLoop(Profile profile)
        {
            if (profile.gather.Infinite)
            {
                return GetWhileElement("True");
            }

            return GetWhileElement($"ItemCount({profile.gather.ItemId}) < {profile.gather.Quantity}");
        }

        private static XElement GetIfElement(string condition)
        {
            return new XElement("If", new XAttribute("Condition", condition));
        }

        private static XElement GetLogMessageElement(string msg)
        {
            return new XElement("LogMessage", new XAttribute("Message", msg));
        }


        private static XElement GetCodeChunks(Profile profile)
        {
            if (!profile.gear.Enabled) return null;
            try
            {
                var code = new[]
                {
                    "",
                    $"\tif (ff14bot.Managers.GearsetManager.ActiveGearset.Index != {profile.gear.GearSet})",
                    "\t{",
                    $"\t\tff14bot.Managers.GearsetManager.ChangeGearset({profile.gear.GearSet});",
                    "\t\tawait Buddy.Coroutines.Coroutine.Sleep(3000);",
                    "\t}",
                    ""
                };

                return new XElement("CodeChunks",
                    new XElement("CodeChunk", new XAttribute("Name", "GearSetChange"),
                        new XCData(String.Join("\r\n", code))));
            }
            catch (Exception)
            {
                //TODO
                throw;
            }
        }


        private static XElement GetWhileElement(string condition)
        {
            return new XElement("While", new XAttribute("Condition", condition));
        }
    }
}