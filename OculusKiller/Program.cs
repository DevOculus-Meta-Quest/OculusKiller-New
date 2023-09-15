using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                string oculusPath = GetOculusPath();
                var steamPaths = GetSteamPaths();

                if (steamPaths == null || string.IsNullOrEmpty(oculusPath))
                {
                    return;
                }

                string startupPath = steamPaths.Item1;
                string vrServerPath = steamPaths.Item2;

                Process.Start(startupPath).WaitForExit();

                // Use Task.Delay for better performance and less CPU usage
                int attempts = 0;
                while (attempts < 100) // 10 seconds (100 * 100ms)
                {
                    if (IsVrServerRunning(vrServerPath))
                    {
                        KillOculusServer(oculusPath);
                        break;
                    }
                    Task.Delay(100).Wait(); // wait for 100ms
                    attempts++;
                }

                if (attempts == 100)
                {
                    LogError("SteamVR vrserver not found... (Did SteamVR crash?)");
                }
            }
            catch (Exception e)
            {
                LogError($"An exception occurred: {e.Message}");
            }
        }

        static string GetOculusPath()
        {
            try
            {
                string oculusPath = Environment.GetEnvironmentVariable("OculusBase");
                if (string.IsNullOrEmpty(oculusPath))
                {
                    throw new Exception("Oculus installation environment not found...");
                }

                oculusPath = Path.Combine(oculusPath, @"Support\oculus-runtime\OVRServer_x64.exe");
                if (!File.Exists(oculusPath))
                {
                    throw new Exception("Oculus server executable not found...");
                }

                return oculusPath;
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return null;
            }
        }

        public static Tuple<string, string> GetSteamPaths()
        {
            try
            {
                string openVrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");
                if (!File.Exists(openVrPath))
                {
                    throw new Exception("OpenVR Paths file not found... (Has SteamVR been run once?)");
                }

                JavaScriptSerializer jss = new JavaScriptSerializer();
                string openvrJsonString = File.ReadAllText(openVrPath);
                var openvrPaths = jss.Deserialize<dynamic>(openvrJsonString);

                string location = openvrPaths["runtime"][0].ToString();
                string startupPath = Path.Combine(location, @"bin\win64\vrstartup.exe");
                string serverPath = Path.Combine(location, @"bin\win64\vrserver.exe");

                if (!File.Exists(startupPath) || !File.Exists(serverPath))
                {
                    throw new Exception("SteamVR executables not found... (Has SteamVR been run once?)");
                }

                return new Tuple<string, string>(startupPath, serverPath);
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return null;
            }
        }

        static bool IsVrServerRunning(string vrServerPath)
        {
            return Process.GetProcessesByName("vrserver").Any(process => process.MainModule.FileName == vrServerPath);
        }

        static void KillOculusServer(string oculusPath)
        {
            var ovrServerProcess = Process.GetProcessesByName("OVRServer_x64").FirstOrDefault(process => process.MainModule.FileName == oculusPath);
            if (ovrServerProcess != null)
            {
                ovrServerProcess.Kill();
                ovrServerProcess.WaitForExit();
            }
            else
            {
                LogError("Oculus runtime not found...");
            }
        }

        static void LogError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
