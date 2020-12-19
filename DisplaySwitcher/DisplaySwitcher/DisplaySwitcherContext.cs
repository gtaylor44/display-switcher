using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DisplaySwitcher
{
  public class DisplaySwitcherContext : ApplicationContext
  {
    NotifyIcon notifyIcon = new NotifyIcon();

    IntPtr wow64Value = IntPtr.Zero;
    

    public DisplaySwitcherContext()
    {
      // Set reg value 'FirstRun' to true so we know not to automatically preset 'Automatically start with Windows' setting.
      try
      {
        var displaySwitcherReg = Registry.CurrentUser.OpenSubKey("DisplaySwitcher", true);

        if (displaySwitcherReg == null)
        {
          var newKey = Registry.CurrentUser.CreateSubKey("DisplaySwitcher");

          newKey.SetValue("FirstRun", "false");

          displaySwitcherReg = Registry.CurrentUser.OpenSubKey("DisplaySwitcher", true);
        }

        if ((displaySwitcherReg.GetValue("FirstRun") as string) != "true")
        {
          displaySwitcherReg.SetValue("FirstRun", "true");

          // enables 'Automatically start with Windows' on first run.
          ToggleStartWithWindows(forceStartWithWindows: true);
        }
      }
      catch { }

      MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

      var thumb = (Bitmap)DisplaySwitcher.Properties.Resources.monitor_icon.GetThumbnailImage(64, 64, null, IntPtr.Zero);
      thumb.MakeTransparent();
      var icon = Icon.FromHandle(thumb.GetHicon());

      notifyIcon.Icon = icon;
      notifyIcon.DoubleClick += new EventHandler(ShowMessage);
      notifyIcon.ContextMenu = new ContextMenu();

      var startWithWindowsMenuItem = new MenuItem()
      {
        Checked = StartsWithWindows(),
        Text = "Automatically start with Windows",
      };

      startWithWindowsMenuItem.Click += StartWithWindowsOnClick;

      notifyIcon.ContextMenu.MenuItems.Add("PC Screen Only", PCScreenOnly);
      notifyIcon.ContextMenu.MenuItems.Add("Duplicate", Duplicate);
      notifyIcon.ContextMenu.MenuItems.Add("Extend", Extend);
      notifyIcon.ContextMenu.MenuItems.Add("Second Screen Only", SecondScreenOnly);
      notifyIcon.ContextMenu.MenuItems.Add(startWithWindowsMenuItem);
      notifyIcon.ContextMenu.MenuItems.Add(exitMenuItem);

      notifyIcon.Visible = true;
    }

    RegistryKey GetStartupRegistryKey()
    {
      RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
      return key;
    }

    bool StartsWithWindows()
    {
      var key = GetStartupRegistryKey();
      Assembly curAssembly = Assembly.GetExecutingAssembly();

      var val = key.GetValue(curAssembly.GetName().Name) as string;

      return string.Equals(val, curAssembly.Location, StringComparison.OrdinalIgnoreCase);
    }

    bool ToggleStartWithWindows(bool forceStartWithWindows = false)
    {
      var key = GetStartupRegistryKey();
      Assembly curAssembly = Assembly.GetExecutingAssembly();

      if (forceStartWithWindows)
      {
        key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
        return true;
      }

      if (StartsWithWindows())
      {
        key.DeleteValue(curAssembly.GetName().Name);
        return false;
      }
      else
      {
        key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
        return true;
      }
    }

    void StartWithWindowsOnClick(object sender, EventArgs e)
    {
      var startsWithWindows = ToggleStartWithWindows();

      var menuItem = sender as MenuItem;

      if (menuItem != null)
      {
        menuItem.Checked = startsWithWindows;
      }
    }

    void ShowMessage(object sender, EventArgs e)
    {
      if (DisplaySwitcher.Properties.Settings.Default.ShowMessage)
        MessageBox.Show($@"Display Switcher v1.0.0{Environment.NewLine}{Environment.NewLine}© Greg Taylor 2020{Environment.NewLine}greg@gregnz.com", "Display Switcher");
    }

    void Exit(object sender, EventArgs e)
    {
      // We must manually tidy up and remove the icon before we exit.
      // Otherwise it will be left behind until the user mouses over.
      notifyIcon.Visible = false;

      Application.Exit();
    }

    private void PCScreenOnly(object sender, EventArgs e)
    {
      SwitchScreen("/internal");
    }

    private void Duplicate(object sender, EventArgs e)
    {
      SwitchScreen("/clone");
    }

    private void Extend(object sender, EventArgs e)
    {
      SwitchScreen("/extend");
    }

    private void SecondScreenOnly(object sender, EventArgs e)
    {
      SwitchScreen("/external");
    }

    private void SwitchScreen(string arguments)
    {
      try
      {
        Wow64Interop.DisableWow64FSRedirection(ref wow64Value);

        Process processToStart = new Process
        {
          StartInfo = {
            FileName = @"DisplaySwitch.exe",
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System),
            Arguments = arguments
          }
        };

        // Start the application
        processToStart.Start();
      }
      catch (Exception exc)
      {
        MessageBox.Show($"Unable to disable/enable WOW64 File System Redirection {exc.Message}", "Error");
      }
      finally
      {
        Wow64Interop.Wow64RevertWow64FsRedirection(wow64Value);
      }
    }
  }
}
