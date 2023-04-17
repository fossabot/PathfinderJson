﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SolidShineUi;
using System.Runtime;
using System.IO;
using System.Windows.Media.Imaging;
using System.Text;
using System.Windows.Shell;
using System.Runtime.InteropServices;

namespace PathfinderJson
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }

        public static ColorScheme ColorScheme { get; set; } = new ColorScheme(Colors.Peru);

        public static Settings Settings { get; set; } = new Settings();

        #region Constants
        public static Version AppVersion { get; } = new Version("1.2.3");
        public const string VersionString = "102030";
        public const int VersionInt = 1_02_03_0;

        public const string NO_HIGH_CONTRAST = "0";

        public const string TABS_VIEW = "tabs";
        public const string CONTINUOUS_VIEW = "continuous";
        public const string RAWJSON_VIEW = "rawjson";

        public const int ABILITY_TABLE_VIEW = 0;
        public const int ABILITY_ICON_VIEW = 1;
        #endregion

        /// <summary>
        /// Get a particular image from the internal Images folder.
        /// </summary>
        /// <exception cref="IOException">Thrown if there is no image with this name.</exception>
        /// <param name="name">The name for the resource.</param>
        public static BitmapImage GetResourcesImage(string name)
        {
            if (!name.EndsWith(".png"))
            {
                name += ".png";
            }

            return new BitmapImage(new Uri("pack://application:,,,/Images/" + name, UriKind.RelativeOrAbsolute));
        }

        public static BitmapImage GetResourcesImage(string name, ColorScheme cs)
        {
            if (!name.EndsWith(".png"))
            {
                name += ".png";
            }

            ImageColor _theme = ImageColor.Color;

            if (cs.IsHighContrast)
            {
                if (cs.BackgroundColor == Colors.Black)
                {
                    _theme = ImageColor.White;
                }
                else
                {
                    _theme = ImageColor.Black;
                }
            }
            else
            {
                if (cs.BackgroundColor == Colors.Black)
                {
                    _theme = ImageColor.White;
                }
                else if (cs.BackgroundColor == Colors.White)
                {
                    _theme = ImageColor.Black;
                }
                else
                {
                    _theme = ImageColor.Color;
                }
            }

            return new BitmapImage(new Uri("pack://application:,,,/Images/" + _theme.ToString("g") + "/" + name, UriKind.RelativeOrAbsolute));
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SettingsIo.SetupBaseDirectories(); // this does not create the actual settings directory, that is done a few lines later

            (string? settingsDir, bool canUpdate) = SettingsIo.FindLatestSettings();
            if (settingsDir == null || (SettingsIo.IsPortable && Directory.Exists(SettingsIo.SettingsDirectory)))
            {
                // settingsDir is null if no old settings directories could be found, so we'll just default to the standard directory
                // otherwise, if this is a Portable app and the portable settings app already exists, just use the standard directory
                settingsDir = SettingsIo.SettingsDirectory;
            }

            // now the actual settings directory is created (could not be created prior to FindLatestSettings to prevent a false positive)
            Directory.CreateDirectory(SettingsIo.SettingsDirectory);

            if (canUpdate)
            {
                // settings stored in old directory, ask user to update
                string pStr = SettingsIo.IsPortable ? "(or non-portable) " : "";
                var result = MessageBox.Show($"Settings for a previous {pStr}version of PathfinderJson was located. Do you want to transfer these settings to this version?",
                                 "Old Settings Found", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    // transfer from old to new
                    foreach (string sff in Directory.GetFiles(settingsDir))
                    {
                        // TODO: add try-catch statements
                        File.Copy(sff, Path.Combine(SettingsIo.SettingsDirectory, Path.GetFileName(sff)), true);
                    }
                }
            } // otherwise, settings is in the standard location

            string settingsFileName = Path.Combine(SettingsIo.SettingsDirectory, "settings.json");

            Settings = SettingsIo.LoadSettingsJson<Settings>(settingsFileName);

            if (Settings.UseStartupOptimization)
            {
                // more info: https://docs.microsoft.com/en-us/dotnet/api/system.runtime.profileoptimization?view=netcore-3.1
                ProfileOptimization.SetProfileRoot(Path.Combine(SettingsIo.AppDataDirectory, "Optimization"));
                ProfileOptimization.StartProfile(VersionInt + "_Startup.profile");
            }

            Newtonsoft.Json.JsonConvert.DefaultSettings = () => new Newtonsoft.Json.JsonSerializerSettings
            {
                DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };

            string file = "";

            if (e.Args.Length > 0)
            {
                if (File.Exists(e.Args[0]))
                {
                    file = e.Args[0];
                }
            }

            if (SystemParameters.HighContrast)
            {
                if (Settings.HighContrastTheme == NO_HIGH_CONTRAST)
                {
                    ColorScheme ncs;
                    MessageDialog md = new MessageDialog
                    {
                        Message = "It appears that you have Windows High Contrast mode activated. Did you want to activate High Contrast mode in PathfinderJSON as well?",
                        OkButtonText = "Yes",
                        CancelButtonText = "No",
                        Image = MessageDialogImage.Question,
                        Title = "PathfinderJSON - High Contrast Mode"
                    };

                    // check the control color
                    if (SystemColors.ControlColor == Colors.Black)
                    {
                        // black color scheme?
                        if (SystemColors.WindowTextColor == Colors.White)
                        {
                            ncs = ColorScheme.GetHighContrastScheme(HighContrastOption.WhiteOnBlack);
                            md.ColorScheme = ncs;

                            if (md.ShowDialog() == MessageDialogResult.OK)
                            {
                                // white on black
                                Settings.HighContrastTheme = "1";
                            }
                        }
                        else
                        {
                            ncs = ColorScheme.GetHighContrastScheme(HighContrastOption.GreenOnBlack);
                            md.ColorScheme = ncs;

                            if (md.ShowDialog() == MessageDialogResult.OK)
                            {
                                // green on black
                                Settings.HighContrastTheme = "2";
                            }
                        }
                    }
                    else
                    {
                        ncs = ColorScheme.GetHighContrastScheme(HighContrastOption.BlackOnWhite);
                        md.ColorScheme = ncs;

                        if (md.ShowDialog() == MessageDialogResult.OK)
                        {
                            // black on white
                            Settings.HighContrastTheme = "3";
                        }
                    }

                    try
                    {
                        Settings.Save(Path.Combine(SettingsIo.SettingsDirectory, "settings.json"));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("The settings file for PathfinderJson could not be saved. Please check the permissions for your AppData folder.",
                            "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (System.Security.SecurityException)
                    {
                        MessageBox.Show("The settings file for PathfinderJson could not be saved. Please check the permissions for your AppData folder.",
                            "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("The settings file for PathfinderJson could not be saved. Please check the permissions for your AppData folder.",
                            "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            MainWindow mw = new MainWindow();
            MainWindow = mw;
            if (file != "") mw.OpenFile(file);
            mw.Show();
        }

        private async void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // save this to the crash logs
            //string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PathfinderJson");
            //string errorLogPath = Path.Combine(appDataPath, "ErrorLogs");

            if (Directory.Exists(SettingsIo.ErrorLogDirectory))
            {
                Exception ex = e.Exception;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(DateTime.UtcNow.ToString("yyyyMMddTHH:mm:ssZ"));
                sb.AppendLine("VERSION " + AppVersion.ToString());
                sb.AppendLine("--------------------------------------");
                sb.AppendLine(ex.GetType().FullName);
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    sb.AppendLine("INNER EXCEPTION:");
                    sb.AppendLine(ex.InnerException.GetType().FullName);
                    sb.AppendLine(ex.InnerException.Message);
                    sb.AppendLine(ex.InnerException.ToString());
                }

                sb.AppendLine("END FILE");

                await File.WriteAllTextAsync(Path.Combine(SettingsIo.ErrorLogDirectory, DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ") + ".txt"), sb.ToString(), Encoding.UTF8);
            }

            MessageBox.Show("An error has occurred and PathfinderJSON may not be able to continue.\n\n" +
                "An error log file was created.\n\n" +
                "Please go to PathfinderJson GitHub website to report the error.",
                "PathfinderJSON Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Jump List Functions
        private void JumpList_JumpItemsRemovedByUser(object sender, JumpItemsRemovedEventArgs e)
        {

        }

        private async void JumpList_JumpItemsRejected(object sender, JumpItemsRejectedEventArgs e)
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PathfinderJson");

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} Jump Items Rejected:\n", e.RejectionReasons.Count);
            for (int i = 0; i < e.RejectionReasons.Count; ++i)
            {
                if (e.RejectedItems[i].GetType() == typeof(JumpPath))
                    sb.AppendFormat("Reason: {0}\tItem: {1}\n", e.RejectionReasons[i], ((JumpPath)e.RejectedItems[i]).Path);
                else
                    sb.AppendFormat("Reason: {0}\tItem: {1}\n", e.RejectionReasons[i], ((JumpTask)e.RejectedItems[i]).ApplicationPath);
            }

            await File.WriteAllTextAsync(Path.Combine(appDataPath, "JumpItemsRejected.txt"), sb.ToString(), Encoding.UTF8);
        }
        #endregion
    }
}
