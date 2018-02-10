using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace TombRaider1Launcher
{
    class JoystickHelper
    {
        private DirectInput directInput;
        private Task pollingTask;
        private Joystick joystick;
        private JoystickDescriptor joystickDescriptor;
        private Task getDInputJoysticksTask;

        public const int A_BUTTON_OFFSET = 48;
        public const int B_BUTTON_OFFSET = 49;
        public const int BACK_BUTTON_OFFSET = 54;
        public const int START_BUTTON_OFFSET = 55;
        public const int LAP_BUTTON_OFFSET = -1;
        public const int POV_BUTTONS_OFFSET = 32;

        public const int POV_UP_BUTTON_VALUE = 0;
        public const int POV_DOWN_BUTTONS_VALUE = 18000;
        public const int POV_LEFT_BUTTONS_VALUE = 27000;
        public const int POV_RIGHT_BUTTONS_VALUE = 9000;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickHelper"/> class.
        /// </summary>
        public JoystickHelper()
        {
            this.directInput = new DirectInput();

            detectDevices();
        }

        /// <summary>
        /// Detects the devices.
        /// </summary>
        private void detectDevices()
        {
            getDInputJoysticksTask = Task.Factory.StartNew(() =>
            {
                do
                {
                    joystickDescriptor = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices)
                                  .Concat(directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                                  .Select(d => new JoystickDescriptor(d.InstanceGuid, d.InstanceName))
                                  .FirstOrDefault();

                    if (joystickDescriptor != null)
                    {
                        this.StartCapture(joystickDescriptor.InstanceGUID);
                    }

                    Task.Delay(500);
                } while (joystickDescriptor == null);
            });
        }

        /// <summary>
        /// Starts the capture.
        /// </summary>
        /// <param name="joystickGuid">The joystick unique identifier.</param>
        public void StartCapture(Guid joystickGuid)
        {
            this.joystick = new Joystick(directInput, joystickGuid);
            this.joystick.Properties.BufferSize = 128;
            this.joystick.Acquire();
            this.PollJoystick();
        }

        /// <summary>
        /// Stops the capture.
        /// </summary>
        /// <param name="stopNow">if set to <c>true</c> [stop now].</param>
        public void StopCapture(bool stopNow = false)
        {
            if (this.getDInputJoysticksTask != null)
            {
                this.getDInputJoysticksTask.Wait(250);
            }

            if (this.pollingTask != null)
            {
                this.pollingTask.Wait(250); // you can give this Wait a timeout
            }
        }

        /// <summary>
        /// Polls the joystick.
        /// </summary>
        private void PollJoystick()
        {
            this.pollingTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        joystick.Poll();
                        JoystickUpdate[] data = this.joystick.GetBufferedData();
                        foreach (JoystickUpdate state in data.Where(IsRelevantUpdate))
                        {
                            // pressed down
                            JoystickButtonPressedEventArgs args = new JoystickButtonPressedEventArgs();
                            args.ButtonOffset = state.RawOffset;
                            args.PovValue = state.Value;
                            args.NamedOffset = state.Offset.ToString().Trim();
                            args.TimeStamp = DateTime.Now;

                            this.OnJoystickButtonPressed(args);
                        }
                    }
                    catch (SharpDX.SharpDXException)
                    {
                        this.StopCapture(true);
                        return;
                    }

                    Task.Delay(250);
                }
            });
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
                (state.RawOffset == 32 && (state.Value == POV_UP_BUTTON_VALUE || state.Value == POV_RIGHT_BUTTONS_VALUE || state.Value == POV_DOWN_BUTTONS_VALUE || state.Value == POV_LEFT_BUTTONS_VALUE)) ||
                // POV DINPUT 
                ((state.Offset.ToString() == "X" || state.Offset.ToString() == "Y") && (state.Value == 0 || state.Value == 65535))
                ;
        }

        /// <summary>
        /// Raises the <see cref="E:JoystickButtonPressed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnJoystickButtonPressed(JoystickButtonPressedEventArgs e)
        {
            this.JoystickButtonPressed?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when [joystick button pressed].
        /// </summary>
        public event EventHandler<JoystickButtonPressedEventArgs> JoystickButtonPressed;

        protected virtual void OnJoystickLapButtonPressed(JoystickButtonPressedEventArgs e)
        {
            this.JoystickLapButtonPressed?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when [joystick lap button pressed].
        /// </summary>
        public event EventHandler<JoystickButtonPressedEventArgs> JoystickLapButtonPressed;

        protected virtual void OnJoystickStartButtonPressed(JoystickButtonPressedEventArgs e)
        {
            this.JoystickStartButtonPressed?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when [joystick start button pressed].
        /// </summary>
        public event EventHandler<JoystickButtonPressedEventArgs> JoystickStartButtonPressed;
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
        private Guid _instanceGUID;
        public Guid InstanceGUID
        {
            get { return _instanceGUID; }
            set { _instanceGUID = value; }
        }

        private string _instanceName;
        public string InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickDescriptor"/> class.
        /// </summary>
        /// <param name="instanceGUID">The instance unique identifier.</param>
        /// <param name="instanceName">Name of the instance.</param>
        public JoystickDescriptor(Guid instanceGUID, string instanceName)
        {
            this.InstanceGUID = instanceGUID;
            this.InstanceName = instanceName;
        }
    }
}
