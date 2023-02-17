using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tarkov_settings.Setting
{
    class AppSetting : Settings<AppSetting>
    {
        public double brightness = 0.5;
        public double contrast = 0.5;
        public double gamma = 1.0;
        public int saturation = 0;
        public HashSet<string> pTargets = new HashSet<string>{
            "EscapeFromTarkov"
        };
        public string display = @"\\.\DISPLAY1";
        public bool minimizeOnStart = false;
        public int prof1Brightness = 0;
        public int prof1Contrast = 0;
        public int prof1Gamma = 0;
        public int prof2Brightness = 0;
        public int prof2Contrast = 0;
        public int prof2Gamma = 0;
    }
}
