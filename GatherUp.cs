using ff14bot.AClasses;
using System;

namespace GatherUp
{
    public class GatherUp : BotPlugin
    {
        public static readonly Version version = new Version(1, 6, 1);
        public override string Author => "Parrot";
        public override string Description => "Tool for making orderbot gathering profiles";
        public override Version Version => version;

        public override string Name => "GatherUp";

        public override bool WantButton => true;

        public override string ButtonText => "Open";

        public override void OnButtonPress()
        {              
            if (_gatherUpForm == null || _gatherUpForm.IsDisposed || _gatherUpForm.Disposing)
            {
                _gatherUpForm = new GatherUpForm();
            }
            _gatherUpForm.Show();
           
        }

        private GatherUpForm _gatherUpForm;
    }
}
