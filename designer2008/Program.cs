using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace Reportman.Designer
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Creates an instance of the methods that will handle the exception.
            MyExceptionHandler eh = new MyExceptionHandler();

            // Adds the event handler  to the event.
            Application.ThreadException += new ThreadExceptionEventHandler(eh.OnThreadException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        // Creates a class to handle the exception event.
        internal class MyExceptionHandler
        {
            // Handles the exception event.
            public void OnThreadException(object sender, ThreadExceptionEventArgs t)
            {
                DialogResult result = DialogResult.Cancel;
                try
                {
                    result = this.ShowThreadExceptionDialog(t.Exception);
                }
                catch
                {
                    try
                    {
                        MessageBox.Show("Fatal Error", "Fatal Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                    }
                    finally
                    {
                        Application.Exit();
                    }
                }

                // Exits the program when the user clicks Abort.
                if (result == DialogResult.Abort)
                    Application.Exit();
            }

            // Creates the error message and displays it.
            private DialogResult ShowThreadExceptionDialog(Exception e)
            {
                string errorMsg = "Exception raised:\n\n";
                errorMsg = errorMsg + e.Message + "\n\nStack call:\n" + e.StackTrace;
                DialogResult aresult = MessageBox.Show(errorMsg, "Application Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                return aresult;
            }
        }
     }
}