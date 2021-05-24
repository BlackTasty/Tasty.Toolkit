using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tasty.Logging;

namespace Tasty.Samples.Controls
{
    /// <summary>
    /// Interaktionslogik für LoggingExample.xaml
    /// </summary>
    public partial class LoggingExample : DockPanel, IConsole
    {
        private bool verboseLogging = false;
        private Logger secondaryLogger;

        public LoggingExample()
        {
            InitializeComponent();

            // Determine via precompiler check if current build is set to Debug or Release. Set "isDebug" flag in Logger.Initialize(...) accordingly
            // Try it out for yourself! Start the example once in Debug mode and once in Release mode and see what this example prints in the log!
#if DEBUG
            // Initialize default debugger with "isDebug" set to true
            Logger.Initialize(true);
#else
            // Initialize default debugger with "isDebug" set to false
            Logger.Initialize(false);
#endif
            // Tell the primary logger that this control should serve as a console output.
            Logger.Default.AttachedConsole = this;

            // Tell the primary logger that output to the attached console should be returned exactly as logged
            Logger.Default.AddIdentifierToConsole = true;

            // Initialize an additional logger with file name "log_secondary.log". Also generate a session id for this logger and regenerate the file when initialized
            secondaryLogger = new Logger("log_secondary.log", true, true);

            // Tell the additional logger that this control should serve as a console output.
            secondaryLogger.AttachedConsole = this;

            Logger.Default.WriteLog("You should see this DEBUG message only in Debug mode!", LogType.DEBUG);
            Logger.Default.WriteLog("You should see this VERBOSE message only in Debug mode!", LogType.VERBOSE);

            try
            {
                throw new System.IO.IOException("Foobar");
            }
            catch (Exception ex)
            {
                Logger.Default.WriteLog("Test", ex);
            }
        }

        public bool VerboseLogging()
        {
            // If true is returned, the Logger class will also log lines with the "VERBOSE" flag
            return verboseLogging;
        }

        public void WriteString(string str, LogType type)
        {
            // This method is called every time the Logger class logs a new line

            LogSimple(str);
            LogColored(str, type);
        }

        private void LogSimple(string str)
        {
            // Append new line to log_simple (TextBox)
            log_simple.Dispatcher.Invoke(() =>
            {
                log_simple.Text += str;
            });
        }

        private void LogColored(string str, LogType type)
        {
            // Determine type of logged message and add new line to log_colored (RichTextBox) with given color
            switch (type)
            {
                case LogType.INFO:
                    SetColoredText(str, Brushes.White);
                    break;

                case LogType.WARNING:
                    SetColoredText(str, Brushes.DarkOrange);
                    break;

                case LogType.ERROR:
                case LogType.FATAL:
                    SetColoredText(str, Brushes.Red);
                    break;

                case LogType.CONSOLE:
                    SetColoredText(str, Brushes.MediumVioletRed);
                    break;

                case LogType.DEBUG:
                    SetColoredText(str, Brushes.SlateBlue);
                    break;

                case LogType.VERBOSE:
                    SetColoredText(str, Brushes.Pink);
                    break;

                default:
                    SetColoredText(str, Brushes.LightSlateGray);
                    break;
            }
        }

        private void SetColoredText(string msg, SolidColorBrush brush)
        {
            try
            {
                // Invoke dispatcher (UI thread) and add new TextRange with given color and text to log_colored (RichTextBox)
                Dispatcher.Invoke(() =>
                {
                    TextRange tr = new TextRange(log_colored.Document.ContentEnd, log_colored.Document.ContentEnd)
                    {
                        Text = msg
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                });
            }
            catch (Exception)
            {

            }
        }

        private void LogText_Click(object sender, RoutedEventArgs e)
        {
            switch (combo_logger.SelectedIndex)
            {
                case 0:
                    // Write something in the primary Logger with the given log type
                    Logger.Default.WriteLog(txt_input.Text, (LogType)combo_type.SelectedIndex);
                    break;
                case 1:
                    // Write something in the additional Logger with the given log type
                    secondaryLogger.WriteLog(txt_input.Text, (LogType)combo_type.SelectedIndex);
                    break;
                default:
                    // Write something in both Logger with the given log type
                    Logger.Default.WriteLog(txt_input.Text, (LogType)combo_type.SelectedIndex);
                    secondaryLogger.WriteLog(txt_input.Text, (LogType)combo_type.SelectedIndex);
                    break;
            }
        }
    }
}
