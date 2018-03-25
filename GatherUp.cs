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
        public static readonly Version version = new Version(1, 5, 2);
        public override string Author => "Parrot";
        public override string Description => "Tool for making orderbot gathering profiles";
        public override Version Version => version;

        public override string Name => "GatherUp";

        public override bool WantButton => true;

        public override string ButtonText => "Open";

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
