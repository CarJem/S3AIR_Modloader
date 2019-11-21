﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;

namespace Sonic3AIR_ModManager
{
    /// <summary>
    /// Interaction logic for AutoBootDialogV2.xaml
    /// </summary>
    public partial class AutoBootDialogV2 : Window
    {
        private System.Windows.Forms.Timer CountDown = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer AnimationLoop = new System.Windows.Forms.Timer();
        private int TimeLeft = (int)(Properties.Settings.Default.AutoLaunchDelay - 1);


        private double AnimationPostion = 1.0;
        private GradientStop WhitePoint = new GradientStop(Colors.White, 0);
        private GradientStop TransparentPoint = new GradientStop(Color.FromArgb(Colors.White.A, Colors.White.R, Colors.White.G, Colors.White.B), 0.5);
        private GradientStop WhitePoint2 = new GradientStop(Colors.White, 1);
        private LinearGradientBrush BrushTest = new LinearGradientBrush();

        public AutoBootDialogV2()
        {
            InitializeComponent();

            AutoBootDialogV2 Instance = this;
            UserLanguage.ApplyLanguage(ref Instance);

            buildDetails.Text = $"{Program.LanguageResource.GetString("ModManagerVersion")}: {Program.Version}" + Environment.NewLine + $"{Program.LanguageResource.GetString("AIRVersion")}: {GetAIRVersion()}";
            Random rnd = new Random();
            int knuckMode = (rnd.Next(1, 25));
            if (knuckMode == 3)
            {
                S3Logo.Visibility = Visibility.Collapsed;
                S3KLogo.Visibility = Visibility.Visible;
            }

            S3Logo.Opacity = 0;
            S3KLogo.Opacity = 0;

            label1.Text = $"  {Program.LanguageResource.GetString("AutoBoot_Initalizing")}";
            CountDown.Interval = 1000;
            AnimationLoop.Tick += AnimationLoop_Tick;
            AnimationLoop.Enabled = true;
            CountDown.Tick += CountDown_Tick;
            CountDown.Enabled = true;

        }

        bool AllowFadeIn = false;
        bool HasPlayedSFX = false;

        private void AnimationLoop_Tick(object sender, EventArgs e)
        {   
            bool allowedToTick = (Properties.Settings.Default.AutoUpdates ? Program.CheckedForUpdateOnStartup && Program.UpdaterState == Updater.UpdateState.Finished : true);
            if (allowedToTick)
            {
                if (!HasPlayedSFX)
                {
                    System.IO.Stream str = Properties.Resources.EntrySFX;
                    System.Media.SoundPlayer snd = new System.Media.SoundPlayer(str);
                    snd.Play();
                    HasPlayedSFX = true;
                }
                if (AnimationPostion >= 1.0)
                {
                    if (TransparentPoint.Color.A != (byte)255)
                    {
                        var color = TransparentPoint.Color;
                        color.A = (byte)(color.A + 0x0F);
                        TransparentPoint.Color = color;
                    }
                    else
                    {
                        AnimationPostion = -2.0;
                    }

                }
                else if (TransparentPoint.Color.A != (byte)0)
                {
                    var color = TransparentPoint.Color;
                    color.A = (byte)(color.A - 0x0F);
                    TransparentPoint.Color = color;
                }
                else
                {
                    AllowFadeIn = true;
                }



                if (!(AnimationPostion >= 1.0) && TransparentPoint.Color.A != (byte)255)
                {
                    if (AnimationPostion <= 0.0 && AllowFadeIn)
                    {
                        S3Logo.Opacity += 0.1;
                        S3KLogo.Opacity += 0.1;
                    }

                    AnimationPostion += 0.1;
                    TransparentPoint.Color = Colors.Transparent;
                }

                BrushTest.StartPoint = new Point(-1, 0);
                BrushTest.EndPoint = new Point(1, 1);

                WhitePoint.Offset = 0;
                TransparentPoint.Offset = AnimationPostion;
                WhitePoint2.Offset = 1;

                TransparentPoint.Offset = AnimationPostion;
                BrushTest.GradientStops = new GradientStopCollection(new List<GradientStop>() { WhitePoint, TransparentPoint, WhitePoint2 });

                logoAlt.OpacityMask = BrushTest;
                logoAlt.InvalidateVisual();

                logo.OpacityMask = BrushTest;
                logo.InvalidateVisual();
            }

        }

        private string GetAIRVersion()
        {
            if (File.Exists(ProgramPaths.Sonic3AIRPath))
            {
                string metaDataFile = Directory.GetFiles(Path.GetDirectoryName(ProgramPaths.Sonic3AIRPath), "metadata.json", SearchOption.AllDirectories).FirstOrDefault();
                if (metaDataFile != null)
                {
                    try
                    {
                        var CurrentAIRVersion = new AIR_API.VersionMetadata(new FileInfo(metaDataFile));
                        return CurrentAIRVersion.VersionString;
                    }
                    catch
                    {
                        return "N/A";

                    }

                }
                else
                {
                    return "N/A";
                }
            }
            else
            {
                return "N/A";
            }
        }



        private void StartUpdater()
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                Program.UpdaterState = Updater.UpdateState.Running;
                label1.Text = $"  {Program.LanguageResource.GetString("AutoBoot_CheckingForUpdates")}";
                Updater updaterTask = new Updater();

            }));

        }

        private void UpdateTimeLeftLabel(bool startUp = false)
        {
            int time = TimeLeft;
            if (startUp) time = time + 1;
            TimeSpan result = TimeSpan.FromSeconds(time);
            string fromTimeString = result.ToString("mm':'ss");
            label1.Text = $" {Program.LanguageResource.GetString("AutoBoot_LaunchingIn")}: {fromTimeString}";
        }

        private void CountDown_Tick(object sender, EventArgs evt)
        {
            bool allowedToProcced = (Properties.Settings.Default.AutoUpdates ? Program.CheckedForUpdateOnStartup && Program.UpdaterState == Updater.UpdateState.Finished : true);
            if (allowedToProcced)
            {
                if (TimeLeft >= 1)
                {
                    UpdateTimeLeftLabel();
                    TimeLeft -= 1;
                }
                else
                {
                    CountDown.Enabled = false;
                    this.DialogResult = true;
                }
            }
            else
            {
                if (Program.UpdaterState == Updater.UpdateState.NeverStarted) StartUpdater();
                else if (Program.UpdateResult != Updater.UpdateResult.Null && Program.CheckedForUpdateOnStartup && Program.UpdaterState == Updater.UpdateState.Finished)
                {
                    Program.LastUpdateResult = Program.UpdateResult;
                    Program.UpdateResult = Updater.UpdateResult.Null;
                }
            }


        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            CountDown.Enabled = false;
            Program.AutoBootCanceled = true;
            this.DialogResult = true;
        }

        private void ForceStartButton_Click(object sender, RoutedEventArgs e)
        {
            TimeLeft = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
