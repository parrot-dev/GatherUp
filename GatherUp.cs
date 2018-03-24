using ff14bot.AClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatherUp
{
    public class GatherUp : BotPlugin
    {
        public static readonly Version version = new Version(1, 5, 1);
        public override string Author { get { return "Parrot"; } }
        public override string Description { get { return "Tool for making orderbot gathering profiles"; } }
        public override Version Version { get { return GatherUp.version; } }
        public override string Name
        {
            get { return "GatherUp"; }
        }
        public override bool WantButton
        {
            get { return true; }
        }
        public override string ButtonText
        {
            get { return "Open"; }
        }
        public override void OnButtonPress()
        {
            Settings.CreateSettingsFile();
            Settings.Load();
            if (_gatherUpForm == null || _gatherUpForm.IsDisposed || _gatherUpForm.Disposing)
            {
                _gatherUpForm = new GatherUpForm();
            }
            _gatherUpForm.Show();

        }


        public override void OnEnabled()
        {

        }

        private GatherUpForm _gatherUpForm;
    }
}
