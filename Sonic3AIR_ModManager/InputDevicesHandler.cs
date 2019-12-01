﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIR_API;
using Device = AIR_API.InputMappings.Device;

namespace Sonic3AIR_ModManager
{
    public static class InputDevicesHandler
    {
        public static InputDevices InputDevices { get; set; }
        public static Dictionary<string, Device> Devices { get => InputDevices.KeyPairs; set => InputDevices.KeyPairs = value; }



        public static void SaveInputs()
        {
            ModManager.S3AIRSettings.InputDevices = InputDevices;
            ModManager.GameConfig.InputDevices = InputDevices;

            ModManager.S3AIRSettings.SaveSettings();
            ModManager.GameConfig.Save();
        }
    }
}