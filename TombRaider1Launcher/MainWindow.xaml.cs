using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace TombRaider1Launcher
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IComponentConnector
    {
        #region Attributes
        private JoystickHelper joystickHelper = new JoystickHelper(); 
        #endregion

        #region Constructor
        public MainWindow()
        {
            // UI Stuff.
            InitializeComponent();

            // Enable Joystick support.
            joystickHelper.SetupJoystickSupport();

            // Register events.
            joystickHelper.JoystickLapButtonPressed += JoystickHelper_JoystickLapButtonPressed;
            joystickHelper.JoystickButtonPressed += JoystickHelper_JoystickButtonPressed;
        } 
        #endregion

        #region Private Methods
        #region Start Game
        /// <summary>
        /// Starts the tr1 game executable.
        /// </summary>
        private void StartTR1()
        {
            try
            {
                Environment.CurrentDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
                ProcessStartInfo startInfo = new ProcessStartInfo("tombati.exe");
                Hide();
                Process.Start(startInfo).WaitForExit();
                Close();
            }
            catch (SystemException)
            {
                Show();
                MessageBox.Show("tombati.exe not found, please check the installation directory.");
            }
        }

        /// <summary>
        /// Starts the tr1 ub game executable.
        /// </summary>
        private void StartTR1_UB()
        {
            try
            {
                Environment.CurrentDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
                ProcessStartInfo startInfo = new ProcessStartInfo("tombub.exe");
                Hide();
                Process.Start(startInfo).WaitForExit();
                Close();
            }
            catch (SystemException)
            {
                Show();
                MessageBox.Show("tombub.exe not found, please check the installation directory.");
            }
        } 
        #endregion

        /// <summary>
        /// Moves the focused element.
        /// </summary>
        /// <param name="direction">The direction.</param>
        private void MoveFocus(FocusNavigationDirection direction)
        {
            var request = new TraversalRequest(direction);
            MoveFocus(request);
        }
        #endregion

        #region UI Events
        /// <summary>
        /// Handles the Click event of the TR1_Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TR1_Button_Click(object sender, RoutedEventArgs e)
        {
            // Disable joystick support before starting the game.
            joystickHelper.StopCapture(true);
            joystickHelper.Dispose();

            StartTR1();
        }

        /// <summary>
        /// Handles the Click event of the TUB_Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TUB_Button_Click(object sender, RoutedEventArgs e)
        {
            // Disable joystick support before starting the game.
            joystickHelper.StopCapture(true);
            joystickHelper.Dispose();

            StartTR1_UB();
        }

        /// <summary>
        /// Handles the Click event of the exitButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable joystick support before stopping the application.
            joystickHelper.StopCapture(true);
            joystickHelper.Dispose();

            Close();
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    MoveFocus(FocusNavigationDirection.Previous);
                    break;
                case Key.Right:
                    MoveFocus(FocusNavigationDirection.Next);
                    break;
                case Key.Escape:
                    Close();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        } 
        #endregion

        #region UI Joystick Events
        /// <summary>
        /// Handles the JoystickStartButtonPressed event of the joystickHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        private void JoystickHelper_JoystickStartButtonPressed(object sender, JoystickButtonPressedEventArgs e)
        {
            MessageBox.Show("joystickHelper_JoystickStartButtonPressed");
        }

        /// <summary>
        /// Handles the JoystickLapButtonPressed event of the joystickHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        private void JoystickHelper_JoystickLapButtonPressed(object sender, JoystickButtonPressedEventArgs e)
        {
            MessageBox.Show("joystickHelper_JoystickLapButtonPressed");
        }

        /// <summary>
        /// Handles the JoystickButtonPressed event of the joystickHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        private void JoystickHelper_JoystickButtonPressed(object sender, JoystickButtonPressedEventArgs e)
        {
            // Translate joystick buttons presses to keyboard keys presses, because WPF does not handle joysticks well for UI navigation.

            // Buttons and POV support.
            switch (e.ButtonOffset)
            {
                case JoystickConstants.A_BUTTON_OFFSET:
                case JoystickConstants.START_BUTTON_OFFSET:
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        SendKeys.Send(Key.Enter);
                    });
                    break;
                case JoystickConstants.BACK_BUTTON_OFFSET:
                case JoystickConstants.B_BUTTON_OFFSET:
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Close();
                    });
                    break;
                case JoystickConstants.POV_BUTTONS_OFFSET:
                    switch (e.PovValue)
                    {
                        case JoystickConstants.POV_UP_BUTTON_VALUE:
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SendKeys.Send(Key.Up);
                            });
                            break;
                        case JoystickConstants.POV_DOWN_BUTTONS_VALUE:
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SendKeys.Send(Key.Down);
                            });
                            break;
                        case JoystickConstants.POV_LEFT_BUTTONS_VALUE:
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SendKeys.Send(Key.Left);
                            });
                            break;
                        case JoystickConstants.POV_RIGHT_BUTTONS_VALUE:
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SendKeys.Send(Key.Right);
                            });
                            break;
                    }
                    break;
                default:
                    break;
            }
            // X and Y axis support (left stick/joystick)
            switch (e.NamedOffset)
            {
                case "X":
                    if (e.PovValue == 0)
                    {
                        // Left
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            SendKeys.Send(Key.Left);
                        });
                    }
                    else if (e.PovValue == UInt16.MaxValue)
                    {
                        // Right
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            SendKeys.Send(Key.Right);
                        });
                    }
                    break;
                case "Y":
                    if (e.PovValue == 0)
                    {
                        // Up
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            SendKeys.Send(Key.Up);
                        });
                    }
                    else if (e.PovValue == UInt16.MaxValue)
                    {
                        // Down
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            SendKeys.Send(Key.Down);
                        });
                    }
                    break;
            }
        } 
        #endregion
    }
}
