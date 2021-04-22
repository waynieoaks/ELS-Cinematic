using System;
using GTA;
using GTA.Native;
using System.Windows.Forms;

namespace ELS_Cinematic_SVnet
{
    public class Main : Script
    {
        public static string INIpath = "scripts\\ELS-Cinematic-SVnet.ini";
        public static ScriptSettings IniSettings;
        public static Keys CinematicKey { get; set; }
        public static Keys CinematicModifierKey { get; set; }
        public static ControllerKeybinds CinematicButton { get; set; }
        public static ControllerKeybinds CinematicModifierButton { get; set; }
        public static Boolean ShowDebug { get; set; }
        public static Boolean CinematicActive { get; set; }
        public static Int32 CurrentFollowVehicleCamMode { get; set; }
        public static Int32 LastFollowVehicleCamMode { get; set; }

        public Main()
        {
            LoadValuesFromIniFile();

            CinematicActive = false;

            KeyDown += OnKeyDown;
            Tick += OnControllerDown;
            Tick += ChkVehicleExitTick;

            Interval = 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == CinematicKey && e.Modifiers == CinematicModifierKey)
            {
                Command_Debug("Debug: Cinematic key just pressed");
                DoCinematic();
            }
        }

        private void OnControllerDown(object sender, EventArgs e)
        {
            if (CinematicButton != ControllerKeybinds.None)
            {
                if (CinematicModifierButton != ControllerKeybinds.None)
                {
                    if (Game.IsControlPressed((GTA.Control)CinematicModifierButton) && Game.IsControlJustReleased((GTA.Control)CinematicButton))
                    {
                        DoCinematic();
                    }
                }
                else if (CinematicModifierButton == ControllerKeybinds.None && Game.IsControlJustReleased((GTA.Control)CinematicButton))
                {
                    DoCinematic();
                }
            }
        }

        private void ChkVehicleExitTick(object sender, EventArgs e)
        {
            if (Game.IsControlPressed((GTA.Control)GTA.Control.VehicleExit))
            {
                Command_Debug("Debug: Vehicle Exit just pressed");
                DoViewCheck();
                Wait(1000);
              }
        }

            private void DoCinematic()
        {
            try
            {

                if (Game.Player.Character.IsInVehicle())
                {
                    // What cam mode are we in?
                    CurrentFollowVehicleCamMode = Function.Call<Int32>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);

                    // Get current state
                    CinematicActive = Function.Call<bool>(Hash.IS_CINEMATIC_CAM_RENDERING);
                    Command_Debug("Debug: Cinematic view was " + CinematicActive.ToString());

                    //bool IS_CINEMATIC_SHOT_ACTIVE = NativeFunction.Natives.IS_CINEMATIC_SHOT_ACTIVE<bool>();
                    //int GET_FOLLOW_VEHICLE_CAM_VIEW_MODE = NativeFunction.Natives.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE<int>();

                    //Command_Debug("Cinematic Shot: " + IS_CINEMATIC_SHOT_ACTIVE.ToString());
                    //Command_Debug("Vehicle Cam Mode: " + GET_FOLLOW_VEHICLE_CAM_VIEW_MODE.ToString());

                    if (CinematicActive == true)
                    {
                        // Turn OFF the cinematic view
                        Command_Debug("Debug: Turning cinematic view OFF");

                        // FIrst turn off the cinematic view
                        Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);

                        if (LastFollowVehicleCamMode == 4)
                        {
                            // Need to go back to 1st person
                            Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, 4);
                        }
                    }
                    else
                    {
                        // Turn ON the cinematic view 
                        Command_Debug("Debug: Turning cinematic view ON");

                        if (CurrentFollowVehicleCamMode == 4)
                        {
                            // You are in 1st person
                            Command_Debug("Debug: In first person");
                            Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, 0);
                            LastFollowVehicleCamMode = 4;
                        }
                        // Make sure we remember the view we were in 
                        LastFollowVehicleCamMode = CurrentFollowVehicleCamMode;

                        // Now turn on the cinematic view
                        Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);
                    }
                }
                else
                {
                    Command_Debug("Debug: Not switching cinematic view as not in vehicle");
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

            if (Game.Player.Character.IsInVehicle())
            {
                try
                {
                    CinematicActive = Function.Call<bool>(Hash.IS_CINEMATIC_CAM_RENDERING);

                    if (CinematicActive == true)
                    {
                        // Turn OFF the cinematic view
                        Command_Debug("Debug: Exiting vehicle while cinematic view was on");

                        if (LastFollowVehicleCamMode == 4)
                        {
                            Command_Debug("Debug: Setting vehicle view back to 1st person");
                            // Need to go back to 1st person
                            Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, 4);
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorLogger(e, "Activation", "Error during vehicle exit");
                }
            }
        }

        private void LoadValuesFromIniFile()
        {
            ScriptSettings scriptSettings = ScriptSettings.Load(INIpath);
            CinematicKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "CinematicKey", Keys.R);
            CinematicModifierKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "CinematicModifierKey", Keys.ControlKey);

            CinematicButton = (ControllerKeybinds)scriptSettings.GetValue<ControllerKeybinds>("Controller", "CinematicButton", ControllerKeybinds.None);
            CinematicModifierButton = (ControllerKeybinds)scriptSettings.GetValue<ControllerKeybinds>("Controller", "CinematicModifierButton", ControllerKeybinds.None);

            ShowDebug = (bool)scriptSettings.GetValue<bool>("Other", "ShowDebug", false);
            Command_Debug("Debug: ELS Cinematic INI loaded");
        }

        public enum ControllerKeybinds
        {
            None = -1, // 0xFFFFFFFF
            A = 201, // 0x000000C9
            B = 202, // 0x000000CA
            X = 203, // 0x000000CB
            Y = 204, // 0x000000CC
            LB = 226, // 0x000000E2
            RB = 227, // 0x000000E3
            LT = 228, // 0x000000E4
            RT = 229, // 0x000000E5
            LS = 230, // 0x000000E6
            RS = 231, // 0x000000E7
            DPadUp = 232, // 0x000000E8
            DPadDown = 233, // 0x000000E9
            DPadLeft = 234, // 0x000000EA
            DPadRight = 235, // 0x000000EB
        }

        public static void Command_Debug(string text)
        {
            if (ShowDebug == true)
            {
                GTA.UI.Notification.Show(text); 
            }
        }

        public static void ErrorLogger(Exception Err, String ErrMod, String ErrDesc)
        {
        //    Game.LogTrivial("--------------------------------------");
        //    Game.LogTrivial("Error during " + ErrMod);
        //    Game.LogTrivial("Decription: " + ErrDesc);
        //    Game.LogTrivial(Err.ToString());
            
            GTA.UI.Notification.Show("~r~~h~ELS-Cinematic:~h~~s~ Error during " + ErrMod + ". Please send logs.");
        }
    }
}