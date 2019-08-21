using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SquirrelTestApp.Launcher
{
    public class UpdateService
    {
        private static readonly string ApplicationName = "SquirrelTestApp";
        private static readonly string UpdatePath = @"C:\Temp\Update";

        public UpdateService()
        {
        }

        public async Task<UpdateResult> Update()
        {
            UpdateResult result;

            try
            {
                using (var updateManager = new UpdateManager(urlOrPath: UpdatePath, applicationName: ApplicationName))
                {
                    Log("Checking for updates...");
                    var checkForUpdateResult = await updateManager.CheckForUpdate(ignoreDeltaUpdates: false, progress: OnCheckForUpdateProgressChange);

                    var releasesToApply = checkForUpdateResult.ReleasesToApply ?? new List<ReleaseEntry>();
                    var requiresUpdate = releasesToApply.Any();
                    
                    var currentVersion = checkForUpdateResult.CurrentlyInstalledVersion?.Version?.Version
                        ?? Assembly.GetExecutingAssembly().GetName().Version;
                    Log($"{ApplicationName} v{currentVersion}");

                    if (requiresUpdate)
                    {
                        Log("Installing updates...");
                        var updateAppResult = await updateManager.UpdateApp(OnUpdateAppProgressChange); ;
                        LogUpdateResult(updateAppResult);

                        var requiresRestart = updateAppResult.Version.Version.Major > currentVersion.Major;
                        result = requiresRestart
                            ? UpdateResult.UpdateAndRestart
                            : UpdateResult.Update;
                    }
                    else
                    {
                        Log("Application is ready.");
                        result = UpdateResult.None;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Cannot connect to the update server.", ex);
                result = UpdateResult.Error;
            }

            return result;
        }

        public void Restart()
        {
            UpdateManager.RestartApp();
        }

        private void OnCheckForUpdateProgressChange(int percentage)
        {
            Log($"Checking for updates: {percentage}%");
        }

        private void OnUpdateAppProgressChange(int percentage)
        {
            Log($"Installing updates: {percentage}%");
        }

        private void LogUpdateResult(ReleaseEntry updateResult)
        {
            Log($"Downloaded {updateResult.Filesize} bytes in {updateResult.Filename} as {(updateResult.IsDelta ? "delta" : "full")} package.");
            Log($"Updated to v{updateResult.Version}.");
        }

        #region Logging helpers

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }

        private void Log(string message, Exception exception)
        {
            Debug.WriteLine($"ERROR: {message}{Environment.NewLine}EXCEPTION: {exception}");
        }

        #endregion
    }
}