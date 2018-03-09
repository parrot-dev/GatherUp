using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;

namespace GatherUp
{
    public class Settings
    {
        public static Profile current = new Profile();
        public static bool save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Profile));
                TextWriter textWriter = new StreamWriter(Application.StartupPath + @"\Plugins\GatherUp\Settings.xml");
                serializer.Serialize(textWriter, current);
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

        public static bool load()
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Profile));
                TextReader textReader = new StreamReader(System.Windows.Forms.Application.StartupPath + @"\Plugins\GatherUp\Settings.xml");
                current = (Profile)deserializer.Deserialize(textReader);
                textReader.Close();
            }
            catch (Exception)
            {
                Log.Bot.print("Failed to load settings");
                current = new Profile();                
                return false;
            }

            Log.Bot.print("Settings loaded", Colors.White);
            return true;

        }

        public class Profile
        {
            public bool DisableBotbaseWarning;
            public string SavePath;

            public Profile()
            {
                DisableBotbaseWarning = false;
                SavePath = Application.StartupPath +@"\Plugins\GatherUp\Profiles";
            }
        }
    }
}
