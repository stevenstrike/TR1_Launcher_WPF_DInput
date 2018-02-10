using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TombRaider1Launcher
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IComponentConnector
    {
        private JoystickHelper joystickHelper = new JoystickHelper();

        public MainWindow()
        {
            this.InitializeComponent();

            joystickHelper.JoystickLapButtonPressed += joystickHelper_JoystickLapButtonPressed;
            joystickHelper.JoystickButtonPressed += joystickHelper_JoystickButtonPressed;
        }

        /// <summary>
        /// Handles the Click event of the TR1_Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TR1_Button_Click(object sender, RoutedEventArgs e)
        {
            joystickHelper.StopCapture();

            startTR1();
        }

        /// <summary>
        /// Handles the Click event of the TUB_Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TUB_Button_Click(object sender, RoutedEventArgs e)
        {
            joystickHelper.StopCapture();

            startTR1_UB();
        }

        /// <summary>
        /// Handles the Click event of the exitButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            joystickHelper.StopCapture();

            this.Close();
        }

        /// <summary>
        /// Starts the tr1.
        /// </summary>
        private void startTR1()
        {
            try
            {
                Environment.CurrentDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
                ProcessStartInfo startInfo = new ProcessStartInfo("tombati.exe");
                this.Hide();
                Process.Start(startInfo).WaitForExit();
                this.Close();
            }
            catch (SystemException)
            {
                this.Show();
                int num = (int)MessageBox.Show("tombati.exe not found, please check the installation directory.");
            }
        }

        /// <summary>
        /// Starts the tr1 ub.
        /// </summary>
        private void startTR1_UB()
        {
            try
            {
                Environment.CurrentDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
                ProcessStartInfo startInfo = new ProcessStartInfo("tombub.exe");
                this.Hide();
                Process.Start(startInfo).WaitForExit();
                this.Close();
            }
            catch (SystemException)
            {
                this.Show();
                int num = (int)MessageBox.Show("tombub.exe not found, please check the installation directory.");
            }
        }

        /// <summary>
        /// Moves the focus.
        /// </summary>
        /// <param name="direction">The direction.</param>
        private void MoveFocus(FocusNavigationDirection direction)
        {
            var request = new TraversalRequest(direction);
            this.MoveFocus(request);
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
                    this.MoveFocus(FocusNavigationDirection.Previous);
                    break;
                case Key.Right:
                    this.MoveFocus(FocusNavigationDirection.Next);
                    break;
                case Key.Escape:
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the JoystickStartButtonPressed event of the joystickHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        private void joystickHelper_JoystickStartButtonPressed(object sender, JoystickButtonPressedEventArgs e)
        {
            MessageBox.Show("joystickHelper_JoystickStartButtonPressed");
        }

        /// <summary>
        /// Handles the JoystickLapButtonPressed event of the joystickHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        private void joystickHelper_JoystickLapButtonPressed(object sender, JoystickButtonPressedEventArgs e)
        {
            MessageBox.Show("joystickHelper_JoystickLapButtonPressed");
        }

        /// <summary>
        /// Handles the JoystickButtonPressed event of the joystickHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JoystickButtonPressedEventArgs"/> instance containing the event data.</param>
        private void joystickHelper_JoystickButtonPressed(object sender, JoystickButtonPressedEventArgs e)
        {
            switch (e.ButtonOffset)
            {
                case JoystickHelper.A_BUTTON_OFFSET:
                case JoystickHelper.START_BUTTON_OFFSET:
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        SendKeys.Send(Key.Enter);
                    });
                    break;
                case JoystickHelper.BACK_BUTTON_OFFSET:
                case JoystickHelper.B_BUTTON_OFFSET:
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.Close();
                    });
                    break;
                case JoystickHelper.POV_BUTTONS_OFFSET:
                    switch (e.PovValue)
                    {
                        case JoystickHelper.POV_UP_BUTTON_VALUE:
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SendKeys.Send(Key.Up);
                            });
                            break;
                        case JoystickHelper.POV_DOWN_BUTTONS_VALUE:
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SendKeys.Send(Key.Down);
                            });
                            break;
                        case JoystickHelper.POV_LEFT_BUTTONS_VALUE:
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SendKeys.Send(Key.Left);
                            });
                            break;
                        case JoystickHelper.POV_RIGHT_BUTTONS_VALUE:
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

            switch(e.NamedOffset)
            {
                case "X":
                    if(e.PovValue == 0)
                    {
                        // Left
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            SendKeys.Send(Key.Left);
                        });
                    }
                    else if(e.PovValue == 65535)
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
                    else if (e.PovValue == 65535)
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
    }
}
