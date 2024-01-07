using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Utils;

namespace Archipelago
{
    public class AP_Settings
    {
        public Dictionary<string, string> settings = new Dictionary<string, string>();
        public bool settingsPresent = false;
        public void readAPSettings()
        {
            const string APSettingsFileName = "APSettings.txt";
            if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(APSettingsFileName))
            {

                using (TextReader file = MyAPIGateway.Utilities.ReadFileInGlobalStorage(APSettingsFileName))
                {
                    string wholeFile = file.ReadToEnd();
                    wholeFile = wholeFile.Replace("\r", "");
                    string[] lines = wholeFile.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line == null || line.Equals(""))
                        {
                            continue;
                        }
                        string[] segments = line.Split(':');
                        settings[segments[0]] = segments[1];
                        //MyLog.Default.WriteLineAndConsole($"[ Setting: {segments[0]}: {segments[1]} ]");
                    }
                    settingsPresent = true;
                }
            }
            else
            {
                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"Archipelago settings file not found! Please start and connect your Space Engineers Client to the Archipelago server!", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLineAndConsole($"Archipelago settings file not found! Please start and connect your Space Engineers Client to the Archipelago server!");
                settingsPresent = false;
            }
        }

        public int getNumber(string key, int initialValue = 0)
        {
            int ret;
            if(settings.ContainsKey(key) && int.TryParse(settings[key], out ret))
            {
                return ret;
            }
            return initialValue;
        }

        public string getString(string key, string initialValue = "")
        {
            if (settings.ContainsKey(key))
            {
                return settings[key];
            }
            return initialValue;
        }
    }
}
