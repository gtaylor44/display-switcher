using System;
using System.Windows.Forms;

namespace DisplaySwitcher
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      // Instead of running a form, we run an ApplicationContext.
      Application.Run(new DisplaySwitcherContext());
    }
  }
}