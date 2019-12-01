﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows;

namespace Sonic3AIR_ModManager
{
    static class Program
    {
        public static bool AutoBootCanceled = false;

        public static Options Arguments;

        public static bool isDebug;

        private static bool isDev = true;

        [ConditionalAttribute("DEBUG")]
        public static void isDebugging()
        {
            isDebug = true;
        }

        private static string VersionString = "1.4.3";

        public static string Version { get => GetVersionString(); }

        private static string GetVersionString()
        {
            return "v." + VersionString + (isDev ? " DEV" : "");
        }

        public static Version InternalVersion { get; } = new Version(VersionString);

        public static bool CheckedForUpdateOnStartup = false;

        public enum UpdateResult : int
        {
            OutOfDate,
            UpToDate,
            Offline,
            FileNotFound,
            ValueNull,
            Null,
            Error
        }

        public enum UpdateState : int
        {
            Running,
            Finished,
            NeverStarted,
        }

        public static UpdateState AIRUpdaterState { get; set; } = UpdateState.NeverStarted;
        public static UpdateState MMUpdaterState { get; set; } = UpdateState.NeverStarted;

        public static UpdateResult AIRUpdateResults { get; set; } = UpdateResult.Null;
        public static UpdateResult MMUpdateResults { get; set; } = UpdateResult.Null;

        public static UpdateResult AIRLastUpdateResult { get; set; } = UpdateResult.Null;
        public static UpdateResult MMLastUpdateResult { get; set; } = UpdateResult.Null;

        public static ResourceManager LanguageResource { get { return UserLanguage.CurrentResource; } set { UserLanguage.CurrentResource = value; } }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            isDebugging();
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>( o => { Arguments = o; });
            ProgramPaths.CreateMissingModManagerFolders();


            var exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists) GamebannaAPIHandler(args);
            else StartApplication(args);

        }

        static void GamebannaAPIHandler(string[] args)
        {
            if (Arguments.gamebanana_api != null)
            {

                int currentFileIndex = 0;
                bool fileCreated = false;
                while (!fileCreated)
                {
                    string path = Path.Combine(ProgramPaths.Sonic3AIR_MM_GBRequestsFolder, $"gb_api{currentFileIndex}.txt");
                    if (!File.Exists(path))
                    {
                        CreateFile(path, Arguments.gamebanana_api);
                        fileCreated = true;
                    }
                    else currentFileIndex++;
                }

            }
            Environment.Exit(Environment.ExitCode);


            void CreateFile(string path, string contents)
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(contents);
                }
            }

        }

        static void GamebanannaAPIHandler_Startup()
        {
            var app = new App();
            app.GBAPI(Arguments.gamebanana_api);
        }

        static void StartApplication(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false); 
            UserLanguage.ApplyLanguageResourcePath(UserLanguage.CurrentLanguage);
            ModFileManagement.CleanUpAPIRequests();

            if (Arguments?.gamebanana_api != null)
            {
                GamebanannaAPIHandler_Startup();
            }
            else
            {
                if (Arguments?.auto_boot == true) ForcedAutoBootStartup();
                else StockStartup();

            }
            
        }

        static void ForcedAutoBootStartup()
        {
            // Save Original Values
            var autoLaunchOld = Properties.Settings.Default.AutoLaunch;
            var preStartOld = Properties.Settings.Default.KeepOpenOnLaunch;
            var postCloseOld = Properties.Settings.Default.KeepOpenOnQuit;
            var autoLaunchDelayOld = Properties.Settings.Default.AutoLaunchDelay;

            // Set Values Specific to Forced Auto Boot Startup
            Properties.Settings.Default.AutoLaunch = true;
            Properties.Settings.Default.KeepOpenOnLaunch = true;
            Properties.Settings.Default.KeepOpenOnQuit = false;
            Properties.Settings.Default.AutoLaunchDelay = 7;
            Properties.Settings.Default.Save();

            // Start Auto-Boot
            AutoBootLoader(true);

            // Revert Options to their Original Values
            Properties.Settings.Default.AutoLaunch = autoLaunchOld;
            Properties.Settings.Default.KeepOpenOnLaunch = preStartOld;
            Properties.Settings.Default.KeepOpenOnQuit = postCloseOld;
            Properties.Settings.Default.AutoLaunchDelay = autoLaunchDelayOld;
            Properties.Settings.Default.Save();

        }

        static void StockStartup()
        {
            if (Properties.Settings.Default.AutoLaunch) AutoBootLoader();
            else
            {
                var app = new App();
                app.DefaultStart();
            }
        }

        static void AutoBootLoader(bool isForced = false)
        {
            var app = new App();
            app.RunAutoBoot(isForced);
        }

        public class Options
        {
            [Option('g', "gamebanana_api", Required = false, HelpText = "Used with Gamebanna's 1 Click Install API")]
            public string gamebanana_api { get; set; }

            [Option('a', "auto_boot", Required = false, HelpText = "Launch's the Application in Auto Boot Mode (Ideal for Steam Big Picture)")]
            public bool auto_boot { get; set; } = false;
        }


    }
}
