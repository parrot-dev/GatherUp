﻿using System;
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
            xProfile.Add(new XElement("Name", new XText(profile.Name)));
            xProfile.Add(new XElement("KillRadius", new XText(profile.Killradius)));
            xProfile.Add(GetOrderPart(profile));
            xProfile.Add(GetCodeChunks(profile));
            return xProfile;
        }


        private static XElement GetOrderPart(Profile profile)
        {
            var order = new XElement("Order");
            AddGearChangeElement(profile.gear, ref order);
            order.Add(GetTeleportElement(profile.TeleportOnStart));
            var outerLoop = GetOuterLoop(profile);
            foreach (var hotspot in profile.Hotspots)
            {
                outerLoop.Add(GetFlyToElement(hotspot.FlyTo));
                outerLoop.Add(profile.DisableMount
                    ? new XElement("RunCode", new XAttribute("Name", "DisableMount"))
                    : null);
                outerLoop.Add(GetGatherPart(profile, hotspot));
                outerLoop.Add(profile.DisableMount
                    ? new XElement("RunCode", new XAttribute("Name", "EnableMount"))
                    : null);
            }

            order.Add(outerLoop);
            order.Add(GetTeleportElement(profile.TeleportOnComplete));

            return order;
        }

        private static XElement GetFlyToElement(Profile.FlyTo flyTo)
        {
            if (!flyTo.Enabled) return null;
            //avoid bloat for simple flytos.
            if (flyTo.Destinations.Count == 1)
            {
                var destination = flyTo.Destinations.First();
                object[] attributes =
                {
                    new XAttribute("Land", destination.Land),
                    new XAttribute("AllowedVariance", destination.AllowedVariance),
                    new XAttribute("XYZ", destination.GetXYZ())
                };
                return new XElement("FlyTo", attributes);
            }

            return new XElement("FlyTo",
                new XElement("DestinationChoices",
                flyTo.Destinations.Select(dest =>
                    new XElement("HotSpot",
                        new XAttribute("Land", dest.Land),
                        new XAttribute("AllowedVariance", dest.AllowedVariance),
                        new XAttribute("XYZ", dest.GetXYZ()))
                )));
        }

        private static XElement GetGatherPart(Profile profile, Profile.HotSpot hotSpot)
        {
            var xGather = GetGatherElement(profile.gather);

            xGather.Add(new XElement("GatherObject", profile.gather.Target));
            xGather.Add(new XElement("HotSpots",
                new XElement("HotSpot", new XAttribute("XYZ", hotSpot.GetXYZ()),
                    new XAttribute("Radius", hotSpot.Radius))));
            if (!profile.gather.exGather.Enabled)
            {
                xGather.Add(new XElement("BlackSpots",
                    profile.Blackspots.Select(bs => new XElement("BlackSpot",
                        new XAttribute("XYZ", bs.GetXYZ()),
                        new XAttribute("Radius", bs.Radius)
                    ))));
            }

            xGather.Add(new XElement("ItemNames",
                profile.Items.Select(item => new XElement("ItemName", new XText(item)))));
            xGather.Add(new XElement("GatheringSkillOrder", profile.Gatherskills.Select(gSkill =>
                new XElement("GatheringSkill", new XAttribute("TimesToCast", 1),
                    new XAttribute("SpellName", gSkill)))));

            return xGather;
        }


        private static XElement GetGatherElement(Profile.Gather gather)
        {
            if (gather.exGather.Enabled)
            {
                object[] attributes =
                {
                    new XAttribute("Loops", "1"),
                    gather.exGather.UseCordial ? new XAttribute("CordialTime", gather.exGather.CordialTime) : null,
                    gather.exGather.UseCordial ? new XAttribute("CordialType", gather.exGather.CordialType) : null,
                    new XAttribute("DiscoverUnknowns", gather.exGather.DiscoverUnknowns)
                };
                return new XElement("ExGather", attributes);
            }

            return new XElement("Gather", new XAttribute("Loops", "1"));
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
            var gearSetChangeCode = new[]
            {
                "",
                $"\tif (ff14bot.Managers.GearsetManager.ActiveGearset.Index != {profile.gear.GearSet})",
                "\t{",
                $"\t\tff14bot.Managers.GearsetManager.ChangeGearset({profile.gear.GearSet});",
                "\t\tawait Buddy.Coroutines.Coroutine.Sleep(3000);",
                "\t}",
                ""
            };

            var disableMountCode = "ff14bot.Settings.CharacterSettings.Instance.UseMount = false;";
            var enableMountCode = "ff14bot.Settings.CharacterSettings.Instance.UseMount = true;";

            return new XElement("CodeChunks",
                new XElement("CodeChunk", new XAttribute("Name", "GearSetChange"),
                    new XCData(String.Join("\r\n", gearSetChangeCode))),
                new XElement("CodeChunk", new XAttribute("Name", "DisableMount"), new XCData(disableMountCode)),
                new XElement("CodeChunk", new XAttribute("Name", "EnableMount"), new XCData(enableMountCode)));
        }


        private static XElement GetWhileElement(string condition)
        {
            return new XElement("While", new XAttribute("Condition", condition));
        }
    }
}