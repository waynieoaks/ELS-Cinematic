using System;
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
        public static ControllerButtons CinematicButton { get; set; }
        public static ControllerButtons CinematicModifierButton { get; set; }
        public static Boolean ShowDebug { get; set; }
        public static Boolean CinematicActive { get; set; }
        public static Int32 CurrentFollowVehicleCamMode { get; set; }
        public static Int32 LastFollowVehicleCamMode { get; set; }

        //Initialization of the plugin.
        public static void Main()
        {
            Game.LogTrivial("Loading ELS-Cinematic settings...");
            LoadValuesFromIniFile();

            CinematicActive = false;

            // Disable the in game control action: 


           
            Game.LogTrivial(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialised.");

            // Cinematic control fiber
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    // Disable the in game control action:

                    Game.DisableControlAction(0, GameControl.VehicleCinCam, true);

                    //Check for key press
                    if (
                            (Game.IsKeyDown(CinematicKey) 
                                && (Game.IsKeyDownRightNow(CinematicModifierKey) 
                                || CinematicModifierKey == Keys.None)
                            ) 
                            || 
                            (Game.IsControllerButtonDown(CinematicButton) 
                                && (Game.IsControllerButtonDownRightNow(CinematicModifierButton) 
                                || CinematicModifierButton == ControllerButtons.None)
                            )
                        )
                    {
                        DoCinematic();
                        GameFiber.Sleep(1000);
                    }
                }
            });

            // Exit vehicle fiber
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    if (Game.IsControlPressed(0, GameControl.VehicleExit) == true)
                    {
                        DoViewCheck();
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
                //Keyboard ini

                if (ini.DoesKeyExist("Keyboard", "CinematicKey")) { CinematicKey = ini.ReadEnum<Keys>("Keyboard", "CinematicKey", Keys.R); }
                else
                {
                    ini.Write("Keyboard", "CinematicKey", "R");
                    CinematicKey = Keys.R;
                }

                if (ini.DoesKeyExist("Keyboard", "CinematicModifierKey")) { CinematicModifierKey = ini.ReadEnum<Keys>("Keyboard", "CinematicModifierKey", Keys.ControlKey); }
                else
                {
                    ini.Write("Keyboard", "CinematicModifierKey", "ControlKey");
                    CinematicModifierKey = Keys.ControlKey;
                }

                // Controller ini
                
                if (ini.DoesKeyExist("Controller", "CinematicButton")) { CinematicButton = ini.ReadEnum<ControllerButtons>("Controller", "CinematicButton", ControllerButtons.None); }
                else
                {
                    ini.Write("Controller", "CinematicButton", "None");
                    CinematicButton = ControllerButtons.None;
                }

                if (ini.DoesKeyExist("Controller", "CinematicModifierButton")) { CinematicModifierButton = ini.ReadEnum<ControllerButtons>("Controller", "CinematicModifierButton", ControllerButtons.None); }
                else
                {
                    ini.Write("Controller", "CinematicModifierButton", "None");
                    CinematicModifierButton = ControllerButtons.None;
                }

                // Other ini

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
                    // What cam mode are we in?
                    CurrentFollowVehicleCamMode = NativeFunction.Natives.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE<Int32>();
                    
                    // Get current state
                    CinematicActive = NativeFunction.Natives.IS_CINEMATIC_CAM_RENDERING<bool>();
                    Command_Debug("Cinematic view was " + CinematicActive.ToString());

                    //bool IS_CINEMATIC_SHOT_ACTIVE = NativeFunction.Natives.IS_CINEMATIC_SHOT_ACTIVE<bool>();
                    //int GET_FOLLOW_VEHICLE_CAM_VIEW_MODE = NativeFunction.Natives.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE<int>();

                    //Command_Debug("Cinematic Shot: " + IS_CINEMATIC_SHOT_ACTIVE.ToString());
                    //Command_Debug("Vehicle Cam Mode: " + GET_FOLLOW_VEHICLE_CAM_VIEW_MODE.ToString());

                    if (CinematicActive == true)
                        {
                            // Turn OFF the cinematic view
                            Command_Debug("Turning cinematic view OFF");

                            // FIrst turn off the cinematic view
                            NativeFunction.Natives.SET_CINEMATIC_MODE_ACTIVE(false);

                            if (LastFollowVehicleCamMode == 4)
                                {
                                // Need to go back to 1st person
                                NativeFunction.Natives.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE(4);
                                }
                        } else {
                            // Turn ON the cinematic view 
                            Command_Debug("Turning cinematic view ON");

                            if (CurrentFollowVehicleCamMode == 4)
                                {
                                    // You are in 1st person
                                    NativeFunction.Natives.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE(0);
                                    LastFollowVehicleCamMode = 4;
                                }
                            // Make sure we remember the view we were in 
                            LastFollowVehicleCamMode = CurrentFollowVehicleCamMode;

                            // Now turn on the cinematic view
                            NativeFunction.Natives.SET_CINEMATIC_MODE_ACTIVE(true);
                        }
                    }
                    else
                    {
                        Command_Debug("Not switching cinematic view as not in vehicle");
                    }

            }
            catch (Exception e)
            {
                ErrorLogger(e, "Activation", "Error during view change sequence");
            }
        }

        private static void DoViewCheck()
        {
            // This is to restore 1st person view in vehicle if exiting while cinematic view is on and last view was 1st person

            if (Game.LocalPlayer.Character.IsInAnyVehicle(true)) {
                try
                {
                    CinematicActive = NativeFunction.Natives.IS_CINEMATIC_CAM_RENDERING<bool>();

                    if (CinematicActive == true)
                    {
                        // Turn OFF the cinematic view
                        Command_Debug("Exiting vehicle while cinematic view was on");

                        if (LastFollowVehicleCamMode == 4)
                        {
                            Command_Debug("Seting vehicle view back to 1st person");
                            // Need to go back to 1st person
                            NativeFunction.Natives.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE(4);
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorLogger(e, "Activation", "Error during vehicle exit");
                }
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
