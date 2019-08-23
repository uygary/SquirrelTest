using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SquirrelTestApp.Launcher;

namespace SquirrelTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
            : base()
        {
            Task.Run(async () => await UpdateApplication());
        }

        /// <summary>
        /// To test this:
        /// 1. Install this version (1.0.0).
        /// 2. Bump the version to 1.1.0 and then try running the installed version (1.0.0).
        /// 3. Bump the version to 2.0.0 and then try running the installed version (1.1.0).
        /// </summary>
        private async Task UpdateApplication()
        {
            var updateSerice = new UpdateService();
            var updateResult = await updateSerice.Update();

            switch (updateResult)
            {
                case UpdateResult.None:
                case UpdateResult.Update:
                {
                    Debug.WriteLine("There is no need to restart.");
                    await Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var laundherWindow = MainWindow;
                        MainWindow = new MainWindow();
                        MainWindow.Show();
                        laundherWindow.Close();
                    }));
                        break;
                }

                case UpdateResult.UpdateAndRestart:
                {
                    Debug.WriteLine("We need to restart.");
                    await Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var dialogWindow = new Window
                        {
                            Content = new Label
                            {
                                Content = "Restarting!",
                            },
                        };
                        dialogWindow.ShowDialog();
                    }));
                    updateSerice.Restart();
                    return;
                }
                case UpdateResult.Error:
                {
                    Debug.WriteLine("I don't know what happened, but this is life for some reason!");
                    await Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var laundherWindow = MainWindow;
                        MainWindow = new ErrorWindow();
                        MainWindow.Show();
                        laundherWindow.Close();
                    }));
                    return;
                }

                default:
                {
                    throw new NotImplementedException("You forgot to handle me!");
                }
            }
        }
    }
}
