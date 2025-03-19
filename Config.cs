using SRML.Config.Attributes;
using UnityEngine;

namespace SimpleZoom
{
    [ConfigFile("SimpleZoomCFG", "OPTIONS")]
    public static class Config
    {
        [ConfigName("Zoom Amount")]
        [ConfigComment("The Amount To Zoom In")]
        public static float ZoomAmount = 20f;

        [ConfigName("Keybind")]
        [ConfigComment("The Keybind To Press To Zoom (Must Be A Valid Keycode)")]
        public static KeyCode Keybind = KeyCode.C;

        [ConfigName("Toggle Mode")]
        [ConfigComment("Whether zoom should be toggled (true) or held (false)")]
        public static bool ToggleMode = false;

        [ConfigName("Zoom Speed")]
        [ConfigComment("The speed of the zoom animation (In Seconds)")]
        public static float ZoomSpeed = 0.08f;

        [ConfigName("Enable Animation")]
        [ConfigComment("Whether zoom animation should be enabled (true) or instant (false)")]
        public static bool EnableAnimation = true;

        [ConfigName("Scroll Sensitivity")]
        [ConfigComment("The sensitivity of the scroll wheel zoom (higher values mean faster zoom)")]
        public static float ScrollSensitivity = 5f;

        [ConfigName("Minimum FOV Zoom Cap")]
        [ConfigComment("The minimum FOV to zoom in")]
        public static float zoomMinCap = .2f;

        [ConfigName("Maximum FOV Zoom Cap")]
        [ConfigComment("The maximum FOV to zoom out")]
        public static float zoomMaxCap = 130f;
    }
}