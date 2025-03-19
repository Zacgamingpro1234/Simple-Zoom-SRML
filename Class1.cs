using SRML;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using static SRML.Console.Console;

namespace SimpleZoom
{
    public class SimpleZoomMod : ModEntryPoint
    {
        private Camera fpsCamera; // To store the Camera reference
        private bool isZoomed = false;
        private bool isInitialized = false; // Track whether the plugin is fully initialized
        private float currentFOV = 75f; // Store the current FOV dynamically
        private float targetFOV; // Target FOV for smooth zoom animation
        private float zoomVelocity; // Velocity for SmoothDamp
        private Coroutine fovMonitorCoroutine; // Reference to the FOV monitoring coroutine
        private bool isGamePaused = false; // Track whether the game is paused

        public override void Load()
        {
            // Initialize the FOV monitoring system
            InitializeFOVMonitoring();
        }

        private void InitializeFOVMonitoring()
        {
            // Use the CoroutineHelper to start the coroutine
            fovMonitorCoroutine = CoroutineHelper.Instance.StartCoroutine(MonitorPauseState());
        }

        private IEnumerator MonitorPauseState()
        {
            while (true)
            {
                // Check if the game is paused (e.g., by checking Time.timeScale)
                bool newPauseState = Time.timeScale == 0;

                // If the pause state has changed
                if (newPauseState != isGamePaused)
                {
                    isGamePaused = newPauseState;

                    if (isGamePaused)
                    {
                        // Start the FOV monitoring coroutine when the game is paused
                        fovMonitorCoroutine = CoroutineHelper.Instance.StartCoroutine(MonitorFOVFromSettings());
                    }
                    else
                    {
                        // Stop the FOV monitoring coroutine when the game is unpaused
                        if (fovMonitorCoroutine != null)
                        {
                            CoroutineHelper.Instance.StopCoroutine(fovMonitorCoroutine);
                            fovMonitorCoroutine = null;
                        }
                    }
                }

                // Wait for the next frame
                yield return null;
            }
        }

        private IEnumerator MonitorFOVFromSettings()
        {
            string lastFOVText = ""; // Store the last retrieved FOV text
            Stopwatch stopwatch = new Stopwatch(); // Use a Stopwatch for precise timing
            stopwatch.Start();

            while (true)
            {
                // Check the FOV value every 0.5 seconds (500 milliseconds)
                if (stopwatch.ElapsedMilliseconds >= 500)
                {
                    stopwatch.Reset(); // Reset the timer
                    stopwatch.Start(); // Start it again immediately

                    GameObject fovLabelObject = GameObject.Find("FOVValueLabel");
                    if (fovLabelObject != null)
                    {
                        // Use reflection to get the TextMeshProUGUI component
                        Component textMeshProComponent = fovLabelObject.GetComponent("TextMeshProUGUI");
                        if (textMeshProComponent != null)
                        {
                            // Use reflection to get the m_text field
                            FieldInfo textField = textMeshProComponent.GetType().GetField("m_text", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (textField != null)
                            {
                                string fovText = (string)textField.GetValue(textMeshProComponent);

                                // Check if the FOV text has changed
                                if (fovText != lastFOVText && float.TryParse(fovText, out float settingsFOV))
                                {
                                    // Update the current FOV value
                                    currentFOV = settingsFOV;
                                    lastFOVText = fovText; // Update the last retrieved FOV text
                                    ConsoleInstance.Log($"FOV updated from settings: {currentFOV}");

                                    // If the camera is already found, update its FOV
                                    if (fpsCamera != null)
                                    {
                                        targetFOV = currentFOV; // Update the target FOV for smooth animation
                                        if (!Config.EnableAnimation)
                                        {
                                            fpsCamera.fieldOfView = targetFOV; // Instant update if animation is disabled
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Yield until the next frame
                yield return null;
            }
        }

        public override void Update()
        {
            // Find the camera if not already found
            if (fpsCamera == null)
            {
                isInitialized = false; // Ensure the plugin is not initialized if the camera is not found
                GameObject cameraObject = GameObject.Find("FPSCamera");
                if (cameraObject != null)
                {
                    fpsCamera = cameraObject.GetComponent<Camera>();
                    if (fpsCamera != null)
                    {
                        ConsoleInstance.Log("FPSCamera and its Camera component successfully located!");

                        // Initialize currentFOV to the camera's current FOV if it hasn't been set yet
                        if (currentFOV == 75f) // 75f is the fallback default value
                        {
                            currentFOV = fpsCamera.fieldOfView;
                            ConsoleInstance.Log($"Initialized currentFOV to camera's FOV: {currentFOV}");
                        }

                        // Set the camera's FOV to currentFOV
                        fpsCamera.fieldOfView = currentFOV;
                        targetFOV = currentFOV; // Initialize targetFOV
                        isInitialized = true; // Mark the plugin as initialized
                    }
                    else
                    {
                        ConsoleInstance.Log("FPSCamera found, but no Camera component attached.");
                    }
                }
                return;
            }

            // Ensure the plugin is initialized before proceeding
            if (!isInitialized)
                return;

            // Handle zoom logic based on toggle/hold mode
            if (Config.ToggleMode)
            {
                // Toggle mode: Press the key to zoom in, press again to zoom out
                if (Input.GetKeyDown(Config.Keybind))
                {
                    if (!isZoomed)
                    {
                        targetFOV = Config.ZoomAmount; // Set target FOV for zoom in
                        isZoomed = true;
                    }
                    else
                    {
                        targetFOV = currentFOV; // Set target FOV for zoom out
                        isZoomed = false;
                    }
                }
            }
            else
            {
                // Hold mode: Hold the key to zoom in, release to zoom out
                if (Input.GetKey(Config.Keybind))
                {
                    if (!isZoomed)
                    {
                        targetFOV = Config.ZoomAmount; // Set target FOV for zoom in
                        isZoomed = true;
                    }
                }
                else
                {
                    if (isZoomed)
                    {
                        targetFOV = currentFOV; // Set target FOV for zoom out
                        isZoomed = false;
                    }
                }
            }

            if (Input.GetKey(Config.Keybind))
            {
                // Handle scroll wheel zoom
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0)
                {
                    float fovChange = scroll * Config.ScrollSensitivity * targetFOV * 0.4f;
                    targetFOV -= fovChange;
                }
            }

            targetFOV = Mathf.Clamp(targetFOV, Config.zoomMinCap, Config.zoomMaxCap);
            // Handle FOV updates based on animation setting
            if (Config.EnableAnimation)
            {
                // Smoothly interpolate the camera's FOV towards the target FOV using SmoothDamp
                fpsCamera.fieldOfView = Mathf.SmoothDamp(fpsCamera.fieldOfView, targetFOV, ref zoomVelocity, Config.ZoomSpeed);
            }
            else
            {
                // Instant update if animation is disabled
                fpsCamera.fieldOfView = targetFOV;
            }
        }
    }
}