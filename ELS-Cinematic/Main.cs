﻿using System;
using System.Windows.Forms;
using Rage;
using Rage.Native;

namespace ELS_Cinematic
{
    public static class EntryPoint

    {
        public static string INIpath = "Plugins\\ELS-Cinematic.ini";
        public static Keys CinematicKey { get; set; }
        public static Keys CinematicModifierKey { get; set; }
        public static Boolean ShowDebug { get; set; }
        public static Boolean CinematicActive { get; set; }

        public static GameControl VehicleCinCam_Control { get; set; }

    //Initialization of the plugin.
    public static void Main()
        {
            Game.LogTrivial("Loading ELS-Cinematic settings...");
            LoadValuesFromIniFile();

            CinematicActive = false;

            // Disable the in game control action: 


           
            Game.LogTrivial(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialised.");

            // Key binding fiber
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    // Disable the in game control action: 
                   // NativeFunction.Natives.DISABLE_CONTROL_ACTION(32, 80, true); Does not work

                    //Check for key press
                    if (Game.IsKeyDown(CinematicKey) &&
                        (Game.IsKeyDownRightNow(CinematicModifierKey) ||
                            CinematicModifierKey == Keys.None))
                    {
                        DoCinematic();
                        GameFiber.Sleep(1000);
                    }
                }

            });

        }

        private static void LoadValuesFromIniFile()
        {
            InitializationFile ini = new InitializationFile(INIpath);
            ini.Create();

            try
            {
                if (ini.DoesKeyExist("Keybindings", "CinematicKey")) { CinematicKey = ini.ReadEnum<Keys>("Keybindings", "CinematicKey", Keys.R); }
                else
                {
                    ini.Write("Keybindings", "CinematicKey", "R");
                    CinematicKey = Keys.R;
                }

                if (ini.DoesKeyExist("Keybindings", "CinematicModifierKey")) { CinematicModifierKey = ini.ReadEnum<Keys>("Keybindings", "CinematicModifierKey", Keys.ControlKey); }
                else
                {
                    ini.Write("Keybindings", "CinematicModifierKey", "ControlKey");
                    CinematicModifierKey = Keys.ControlKey;
                }

                if (ini.DoesKeyExist("Other", "ShowDebug")) { ShowDebug = ini.ReadBoolean("Other", "ShowDebug", false); }
                else
                {
                    ini.Write("Other", "ShowDebug", "false");
                    ShowDebug = false;
                }

                Game.LogTrivial("Settings initialisation complete.");
            }
            catch (Exception e)
            {
                ErrorLogger(e, "Initialisation", "Unable to read INI file.");
            }

        }

        private static void DoCinematic()
        {

            try
            {

                if (Game.LocalPlayer.Character.IsInAnyVehicle(true))
                {
                // Get current state
                CinematicActive = NativeFunction.Natives.IS_CINEMATIC_CAM_RENDERING<bool>();
                Command_Debug("Cinematic view was " + CinematicActive.ToString());

                    if (CinematicActive == true)
                    {
                        // Turn OFF the cinematic view
                        Command_Debug("Turning Cinematic view OFF");
                        NativeFunction.Natives.SET_CINEMATIC_MODE_ACTIVE(false);
                    } else
                    {
                        // Turn ON the cinematic view 
                        Command_Debug("Turning Cinematic view ON");
                        NativeFunction.Natives.SET_CINEMATIC_MODE_ACTIVE(true);
                    }
                }
                else
                {
                    Command_Debug("Not switching Cinematic view as not in vehicle");
                }

            }
            catch (Exception e)
            {
                ErrorLogger(e, "Activation", "Error during view change sequence");
            }
        }

        public static void Command_Debug(string text)
        {
            if (ShowDebug == true) 
            {
                Game.DisplayNotification(text);
            }
        }

        public static void ErrorLogger(Exception Err, String ErrMod, String ErrDesc)
        {
            Game.LogTrivial("--------------------------------------");
            Game.LogTrivial("Error during " + ErrMod);
            Game.LogTrivial("Decription: " + ErrDesc);
            Game.LogTrivial(Err.ToString());
            Game.DisplayNotification("~r~~h~ELS-Cinematic:~h~~s~ Error during " + ErrMod + ". Please send logs.");
        }
    }
}