using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ff14bot.Managers;
using static GatherUp.Helpers.CodeIndentor;

namespace GatherUp.Order
{
    class ProfileTransformer
    {
        private readonly Profile _profile;
        private readonly Version _version;
        private Profile.Gear Gear => _profile.gear;
        private Profile.Gather Gather => _profile.gather;

        public ProfileTransformer(Profile profile)
        {
            _profile = profile;
            _version = GatherUp.version;
        }

        public XDocument ToXDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                GetProfileElement()
            );
        }

        private XElement GetProfileElement()
        {
            var xProfile = new XElement("Profile");
            xProfile.Add(new XComment($"Generated with GatherUp {_version}"));
            xProfile.Add(new XElement("Name", new XText(_profile.Name)));
            xProfile.Add(new XElement("KillRadius", new XText(_profile.Killradius)));
            xProfile.Add(GetOrderPart());
            xProfile.Add(GetCodeChunks());
            return xProfile;
        }


        private XElement GetOrderPart()
        {
            var order = new XElement("Order", GetGearChangeElement());
            var outerLoop = GetWhileElement(GetGatherCondition());
            outerLoop.Add(GetTeleportElement(_profile.TeleportOnStart));
            outerLoop.Add(new XElement("RunCode", new XAttribute("Name", "ApplySneak")));
            foreach (var hotspot in _profile.Hotspots)
            {
                var elements = new List<XElement>
                {
                    GetFlyToElement(hotspot.FlyTo),
                    hotspot.DisableMount ? GetLogMessageElement("Disabling mount") : null,
                    hotspot.DisableMount ? GetDisableMount() : null,
                    GetGatherPart(hotspot),
                    hotspot.DisableMount ? GetLogMessageElement("Enabling mount") : null,
                    hotspot.DisableMount ? GetEnableMount() : null
                };

                if (!Gather.Infinite)
                {
                    var ifNotDone = GetIfElement(GetGatherCondition());
                    ifNotDone.Add(elements);
                    outerLoop.Add(ifNotDone);
                }
                else
                {
                    outerLoop.Add(elements);
                }
            }

            order.Add(outerLoop);
            order.Add(GetTeleportElement(_profile.TeleportOnComplete));

            return order;
        }

        private XElement GetEnableMount()
        {
            return new XElement("RunCode", new XAttribute("Name", "EnableMount"));
        }

        private XElement GetDisableMount()
        {
            return new XElement("RunCode", new XAttribute("Name", "DisableMount"));
        }

        private XElement GetFlyToElement(Profile.FlyTo flyTo)
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

        private XElement GetGatherPart(Profile.HotSpot hotSpot)
        {
            var xGather = GetGatherElement();

            xGather.Add(new XElement("GatherObject", Gather.Target));
            xGather.Add(new XElement("HotSpots",
                new XElement("HotSpot", new XAttribute("XYZ", hotSpot.GetXYZ()),
                    new XAttribute("Radius", hotSpot.Radius))));
            if (!Gather.exGather.Enabled)
            {
                xGather.Add(new XElement("BlackSpots",
                    _profile.Blackspots.Select(bs => new XElement("BlackSpot",
                        new XAttribute("XYZ", bs.GetXYZ()),
                        new XAttribute("Radius", bs.Radius)
                    ))));
            }

            xGather.Add(new XElement("ItemNames",
                _profile.Items.Select(item => new XElement("ItemName", new XText(item)))));

            if (_profile.Gatherskills.Any())
            {
                xGather.Add(new XElement("GatheringSkillOrder", _profile.Gatherskills.Select(gSkill =>
                    new XElement("GatheringSkill", new XAttribute("TimesToCast", 1),
                        new XAttribute("SpellName", gSkill)))));
            }

            return xGather;
        }


        private XElement GetGatherElement()
        {
            if (Gather.exGather.Enabled)
            {
                object[] attributes =
                {
                    new XAttribute("Loops", "1"),
                    Gather.exGather.UseCordial ? new XAttribute("CordialTime", Gather.exGather.CordialTime) : null,
                    Gather.exGather.UseCordial ? new XAttribute("CordialType", Gather.exGather.CordialType) : null,
                    new XAttribute("DiscoverUnknowns", Gather.exGather.DiscoverUnknowns)
                };
                return new XElement("ExGather", attributes);
            }

            return new XElement("Gather", new XAttribute("Loops", "1"));
        }

        private object[] GetGearChangeElement()
        {
            if (!Gear.Enabled) return null;
            return new object[]
            {
                GetLogMessageElement($"Using gearset: {Gear.GearSet}"),
                new XElement("RunCode", new XAttribute("Name", "GearSetChange"))
            };
        }

        private XElement GetTeleportElement(Profile.Teleport teleport)
        {
            if (!teleport.Enabled) return null;
            var xIfElement = GetIfElement($"not IsOnMap({teleport.ZoneId})");
            xIfElement.Add(GetLogMessageElement($"Teleporting to {teleport.Name}"));
            xIfElement.Add(new XElement("TeleportTo", new XAttribute("Name", teleport.Name),
                new XAttribute("AetheryteId", teleport.AetheryteId)));
            return xIfElement;
        }


        private string GetGatherCondition()
        {
            if (Gather.Infinite) return "True";
            var func = Gather.Hq ? "HqItemCount" : "ItemCount";
            return $"{func}({Gather.ItemId}) < {Gather.Quantity}";
        }

        private XElement GetIfElement(string condition)
        {
            return new XElement("If", new XAttribute("Condition", condition));
        }

        private XElement GetLogMessageElement(string msg)
        {
            return new XElement("LogMessage", new XAttribute("Message", msg));
        }


        private XElement GetCodeChunks()
        {
            var gearSetChangeCode = new[]
            {
                "",
                $"var index = {Gear.GearSet};",
                "if (index > GearsetManager.GearsetLimit || !GearsetManager.GearSets.ElementAt(index-1).InUse) {",
                "Logging.Write(\"Invalid gearset\");",
                "} else {",
                "var gear = GearsetManager.GearSets.ElementAt(index-1);",
                "if (GearsetManager.ActiveGearset.Index != gear.Index) {",
                "do {",
                "await Buddy.Coroutines.Coroutine.Sleep(1900);",
                "} while (Core.Player.IsCasting);", //if bot starts to summon chocobo.
                "await Buddy.Coroutines.Coroutine.Sleep(1000);",
                "gear.Activate();",
                "await Buddy.Coroutines.Coroutine.Sleep(5000);", //More human like, avoid immediate teleport.
                "}",
                "}"
            };

            var sneakCode = new[]
            {
                "",
                "var localPlayer = GameObjectManager.LocalPlayer;",
                "if (!localPlayer.HasAura(\"Sneak\")) {",
                "SpellData spell;",
                "if (ActionManager.CurrentActions.TryGetValue(\"Sneak\", out spell)) {",
                "if (ActionManager.CanCast(spell, localPlayer)) {",
                "Logging.Write(\"Applying Sneak\");",
                "await Buddy.Coroutines.Coroutine.Sleep(3000);", //teleport load issue.
                "ActionManager.DoAction(spell, localPlayer);",
                "}",
                "}",
                "}"
            };

            var disableMountCode = "ff14bot.Settings.CharacterSettings.Instance.UseMount = false;";
            var enableMountCode = "ff14bot.Settings.CharacterSettings.Instance.UseMount = true;";

            return new XElement("CodeChunks",
                Gear.Enabled
                    ? new XElement("CodeChunk", new XAttribute("Name", "GearSetChange"),
                        new XCData(IndentCode(gearSetChangeCode, 2)))
                    : null,
                ProfileDisablesMount()
                    ? new XElement("CodeChunk", new XAttribute("Name", "DisableMount"), new XCData(disableMountCode))
                    : null,
                ProfileDisablesMount()
                    ? new XElement("CodeChunk", new XAttribute("Name", "EnableMount"), new XCData(enableMountCode))
                    : null,
                new XElement("CodeChunk", new XAttribute("Name", "ApplySneak"),
                        new XCData(IndentCode(sneakCode, 2)))
                    );
        }

        private bool ProfileDisablesMount()
        {
            return _profile.Hotspots.Any(hs => hs.DisableMount);
        }

        private XElement GetWhileElement(string condition)
        {
            return new XElement("While", new XAttribute("Condition", condition));
        }
    }
}