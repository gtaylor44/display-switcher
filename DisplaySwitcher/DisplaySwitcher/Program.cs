using System;
using System.Threading;
using System.Windows.Forms;

namespace DisplaySwitcher
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// 
    static Mutex myMutex;

    [STAThread]
    static void Main()
    {
      bool aIsNewInstance = false;
      myMutex = new Mutex(true, "DisplaySwitcher", out aIsNewInstance);
      if (!aIsNewInstance)
      {
        MessageBox.Show("Already an instance running. Please check your task tray.", "Display Switcher");
        Environment.Exit(0);
      }

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      // Instead of running a form, we run an ApplicationContext.
      Application.Run(new DisplaySwitcherContext());
    }
  }
}