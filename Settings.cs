using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;
using ff14bot.Managers;

namespace GatherUp
{
    public class Settings
    {
        public static Profile Current = new Profile();

        public static bool Save()
        {
            return Save(Current);
        }

        public static bool Save(Profile profile)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Profile));
                TextWriter textWriter = new StreamWriter(PluginManager.PluginDirectory + @"\GatherUp\Settings.xml");
                serializer.Serialize(textWriter, profile);
                textWriter.Close();
            }
            catch (Exception e)
            {
                Log.Bot.print("Error while saving :\n" + e.Message);
                return false;
            }
            Log.Bot.print("Saved settings.", Colors.White);
            return true;
        }

        public static bool Load()
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Profile));
                TextReader textReader = new StreamReader(PluginManager.PluginDirectory + @"\GatherUp\Settings.xml");
                Current = (Profile)deserializer.Deserialize(textReader);
                textReader.Close();
            }
            catch (Exception)
            {
                Log.Bot.print("Failed to load settings");
                Current = new Profile();                
                return false;
            }

            Log.Bot.print("Settings loaded", Colors.White);
            return true;

        }

        public static void CreateSettingsFile()
        {
            if (!File.Exists(PluginManager.PluginDirectory + @"\GatherUp\Settings.xml"))
            {
                Log.Bot.print("Settings.xml is missing, creating a new file.", Colors.White);
                Save(new Profile());
            }
        }

        public class Profile
        {
            public bool DisableBotbaseWarning;
            public string SavePath;

            public Profile()
            {
                DisableBotbaseWarning = false;
                SavePath = PluginManager.PluginDirectory + @"\GatherUp\Profiles";
            }
        }
    }
}
