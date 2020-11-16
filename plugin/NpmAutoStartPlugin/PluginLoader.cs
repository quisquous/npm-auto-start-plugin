using System.Windows.Forms;
using RainbowMage.OverlayPlugin;
using Advanced_Combat_Tracker;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System;

namespace NpmAutoStart
{
    public class PluginLoader : IActPluginV1, IOverlayAddonV2
    {
        string pluginDirectory;
        ILogger logger;
        Process npm = null;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginStatusText.Text = "Ready.";

            // We don't need a tab here.
            ((TabControl)pluginScreenSpace.Parent).TabPages.Remove(pluginScreenSpace);
        }

        public void DeInitPlugin()
        {
            KillProcess();
        }

        // TODO: there's no way to get the plugin directory that this plugin was loaded from.
        // Maybe InitPlugin should get this information??
        private ActPluginData GetPluginData(string pluginName)
        {
            ActPluginData found = null;
            foreach (var plugin in Advanced_Combat_Tracker.ActGlobals.oFormActMain.ActPlugins)
            {
                if (!plugin.cbEnabled.Checked)
                {
                    continue;
                }
                if (plugin.pluginFile.Name == pluginName)
                {
                    if (found != null)
                    {
                        logger.Log(LogLevel.Error, "Found duplicate {0}", pluginName);
                    }
                    found = plugin;
                }
            }
            return found;
        }

        public void Init()
        {
            var pluginName = "NpmAutoStart.dll";
            var plugin = GetPluginData(pluginName);
            if (plugin == null) {
                logger.Log(LogLevel.Error, "Could not find plugin: {0}", pluginName);
                return;
            }
            pluginDirectory = plugin.pluginFile.DirectoryName;

            logger = Registry.GetContainer().Resolve<ILogger>();

            CreateProcess();
        }

        public void CreateProcess()
        {
            KillProcess();

            ProcessStartInfo info = new ProcessStartInfo("npm.cmd", "start");
            // npm.cmd passes directories to node.exe as an arg, so just use a shell here for now?
            info.UseShellExecute = true;
            info.WorkingDirectory = pluginDirectory;
            info.ErrorDialog = true;

            // Hide in release builds.
            #if !DEBUG
            // CreateNoWindow is ignored when UseShellExecute is true, use this instead.
            info.WindowStyle = ProcessWindowStyle.Hidden;
            #endif

            // TODO: redirect standard out/error to overlayplugin log (only in debug?)

            Process p;
            try
            {
                p = new Process();
                p.EnableRaisingEvents = true;
                p.StartInfo = info;
                p.Start();
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, "Failed to start npm: {0}: {1}", e.Message, e.ToString());
                return;
            }

            npm = p;
            npm.Exited += (o, e) => OnExit();
            logger.Log(LogLevel.Info, "npm start ({0})", pluginDirectory);
        }

        public void OnExit()
        {
            if (npm == null)
            {
                return;
            }
            logger.Log(LogLevel.Info, "npm exited ({0}, error {1})", pluginDirectory, npm.ExitCode);
            npm = null;
        }

        public void KillProcess()
        {
            if (npm == null)
            {
                return;
            }
            // We need to close the main window, because UseShellExecute spawns a shell and a child process.
            npm.CloseMainWindow();
            // For good measure.
            npm.Kill();

            npm = null;
            logger.Log(LogLevel.Info, "npm kill ({0})", pluginDirectory);
        }
    }
}
