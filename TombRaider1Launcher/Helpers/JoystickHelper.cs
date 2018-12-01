using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace TombRaider1Launcher
{
    class JoystickHelper : IDisposable
    {
        #region Attributes
        private Task pollingTask = null;
        private Task scanningDevicesTask = null;

        private DirectInput directInput = null;
        private Joystick joystick = null;

        private bool IsQuitPolling = false;
        private bool IsQuitScanning = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickHelper"/> class.
        /// </summary>
        public JoystickHelper()
        {
            directInput = new DirectInput();
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Detects the devices.
        /// </summary>
        public IList<JoystickDescriptor> DetectDevices()
        {
            return directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices)
                                  .Concat(directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                                  .Select(d => new JoystickDescriptor(d.InstanceGuid, d.InstanceName))
                                  .ToList();
        }

        /// <summary>
        /// Setups the joystick support. (Scan devices loop)
        /// </summary>
        public void SetupJoystickSupport()
        {
            scanningDevicesTask = Task.Factory.StartNew(() =>
            {
                while (!IsQuitScanning)
                {
                    if (!IsCurrentJoystickAttached())
                    {
                        //Get list of detected joystick devices.
                        var joysticks = DetectDevices();

                        if (joysticks != null && joysticks.Any())
                        {
                            // Capture the device.
                            StartCapture(joysticks.FirstOrDefault(x => x.InstanceGUID != null && x.InstanceGUID != Guid.Empty).InstanceGUID);
                        }
                    }
                    Task.Delay(500);
                }
            });
        }

        #region Joystick Capture
        /// <summary>
        /// Starts the capture.
        /// </summary>
        /// <param name="joystickGuid">The joystick unique identifier.</param>
        public void StartCapture(Guid joystickGuid)
        {
            joystick = new Joystick(directInput, joystickGuid);
            joystick.Properties.BufferSize = 128;

            // Get access to the joystick.
            joystick.Acquire();

            // Resets the value for the polling loop.
            IsQuitPolling = false;

            // Polling loop to read joystick data.
            PollJoystick();
        }

        /// <summary>
        /// Stops the capture.
        /// </summary>
        public void StopCapture(bool stopBackgroundScan = false)
        {
            //We want to stop the polling of the joystick (used for while loop).
            IsQuitPolling = true;
            // Wait for the while loop to stop in the polling thread.
            if (pollingTask != null)
            {
                pollingTask.Wait(250);
            }

            // Release access to the joystick.
            if (joystick != null)
            {
                joystick.Unacquire();
            }

            // If we want to stop the background scan loop (to add a new peripheral when unplugged -> plugged)
            if (stopBackgroundScan)
            {
                IsQuitScanning = true;

                if (scanningDevicesTask != null)
                {
                    scanningDevicesTask.Wait(250);
                }
            }
        }
        #endregion

        #endregion

        #region Private Methods
        /// <summary>
        /// Polls the joystick.
        /// </summary>
        private void PollJoystick()
        {
            pollingTask = Task.Factory.StartNew(() =>
            {
                while (!IsQuitPolling)
                {
                    try
                    {
                        joystick.Poll();
                        JoystickUpdate[] data = joystick.GetBufferedData();
                        foreach (JoystickUpdate joystickState in data.Where(IsRelevantUpdate))
                        {
                            // button pressed down
                            JoystickButtonPressedEventArgs args = new JoystickButtonPressedEventArgs
                            {
                                ButtonOffset = joystickState.RawOffset,
                                PovValue = joystickState.Value,
                                NamedOffset = joystickState.Offset.ToString().Trim(),
                                TimeStamp = DateTime.Now
                            };

                            OnJoystickButtonPressed(args);
                        }
                    }
                    catch (SharpDX.SharpDXException)
                    {
                        StopCapture();
                        return;
                    }

                    Task.Delay(250);
                }
            });
        }

        #region States
        /// <summary>
        /// Determines whether [is current joystick attached].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is current joystick attached]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCurrentJoystickAttached()
        {
            if (joystick != null)
            {
                try
                {
                    return joystick.GetCurrentState() != null;
                }
                catch
                {
                    return false;
                }

            }
            else return false;
        }

        /// <summary>
        /// Determines whether [is relevant update] [the specified state].
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>
        ///   <c>true</c> if [is relevant update] [the specified state]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRelevantUpdate(JoystickUpdate state)
        {
            return
                state.Offset >= JoystickOffset.Buttons0 &&
                state.Offset <= JoystickOffset.Buttons127 &&
                state.Value == 128 ||
                // POV XBOX ONE.
                (state.RawOffset == 32 && (state.Value == JoystickConstants.POV_UP_BUTTON_VALUE || state.Value == JoystickConstants.POV_RIGHT_BUTTONS_VALUE || state.Value == JoystickConstants.POV_DOWN_BUTTONS_VALUE || state.Value == JoystickConstants.POV_LEFT_BUTTONS_VALUE)) ||
                // POV DINPUT.
                ((state.Offset.ToString() == "X" || state.Offset.ToString() == "Y") && (state.Value == 0 || state.Value == UInt16.MaxValue));
        }
        #endregion 
        #endregion

        #region Joystick Events
        /// <summary>
        /// Raises the <see cref="E:JoystickButtonPressed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnJoystickButtonPressed(JoystickButtonPressedEventArgs e)
        {
            JoystickButtonPressed?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when [joystick button pressed].
        /// </summary>
        public event EventHandler<JoystickButtonPressedEventArgs> JoystickButtonPressed;

        protected virtual void OnJoystickLapButtonPressed(JoystickButtonPressedEventArgs e)
        {
            JoystickLapButtonPressed?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when [joystick lap button pressed].
        /// </summary>
        public event EventHandler<JoystickButtonPressedEventArgs> JoystickLapButtonPressed;

        protected virtual void OnJoystickStartButtonPressed(JoystickButtonPressedEventArgs e)
        {
            JoystickStartButtonPressed?.Invoke(this, e);
        } 

        /// <summary>
        /// Occurs when [joystick start button pressed].
        /// </summary>
        public event EventHandler<JoystickButtonPressedEventArgs> JoystickStartButtonPressed;
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // For Redundant Calls.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (joystick != null)
                    {
                        StopCapture();
                        joystick.Dispose();
                    }

                    if (directInput != null)
                    {
                        directInput.Dispose();
                    }
                }

                joystick = null;
                directInput = null;
                pollingTask = null;
                scanningDevicesTask = null;

                disposedValue = true;
            }
        }

        // Implements Disposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class JoystickButtonPressedEventArgs : EventArgs
    {
        public int ButtonOffset { get; set; }
        public int PovValue { get; set; }
        public string NamedOffset { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class JoystickDescriptor
    {
        public Guid InstanceGUID { get; set; }
        public string InstanceName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickDescriptor"/> class.
        /// </summary>
        /// <param name="instanceGUID">The instance unique identifier.</param>
        /// <param name="instanceName">Name of the instance.</param>
        public JoystickDescriptor(Guid instanceGUID, string instanceName)
        {
            InstanceGUID = instanceGUID;
            InstanceName = instanceName;
        }
    }

    /// <summary>
    /// Constants values for Joystick.
    /// </summary>
    public class JoystickConstants
    {
        #region Buttons Offsets
        public const int A_BUTTON_OFFSET = 48;
        public const int B_BUTTON_OFFSET = 49;
        public const int BACK_BUTTON_OFFSET = 54;
        public const int START_BUTTON_OFFSET = 55;
        public const int LAP_BUTTON_OFFSET = -1;
        public const int POV_BUTTONS_OFFSET = 32;
        #endregion

        #region Buttons Values
        public const int POV_UP_BUTTON_VALUE = 0;
        public const int POV_DOWN_BUTTONS_VALUE = 18000;
        public const int POV_LEFT_BUTTONS_VALUE = 27000;
        public const int POV_RIGHT_BUTTONS_VALUE = 9000; 
        #endregion
    }
}
