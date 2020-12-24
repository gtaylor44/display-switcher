using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DisplaySwitcher
{
  public class DisplaySwitcherContext : ApplicationContext
  {
    private readonly NotifyIcon _notifyIcon = new NotifyIcon();

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

      var thumb = (Bitmap)Properties.Resources.monitor_icon.GetThumbnailImage(64, 64, null, IntPtr.Zero);
      thumb.MakeTransparent();
      var icon = Icon.FromHandle(thumb.GetHicon());

      _notifyIcon.Icon = icon;
      _notifyIcon.DoubleClick += new EventHandler(ShowMessage);

      _notifyIcon.Text = "Display Switcher";

      _notifyIcon.ContextMenu = new ContextMenu();

      var startWithWindowsMenuItem = new MenuItem()
      {
        Checked = StartsWithWindows(),
        Text = "Automatically start with Windows"
      };

      startWithWindowsMenuItem.Click += StartWithWindowsOnClick;

      var selectedTopologyId = NativeMethods.GetCurrentTopologyId();

      AddMenuItem("PC Screen Only", PCScreenOnly, selectedTopologyId == DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_INTERNAL);
      AddMenuItem("Duplicate", Duplicate, selectedTopologyId == DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_CLONE);
      AddMenuItem("Extend", Extend, selectedTopologyId == DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_EXTEND);
      AddMenuItem("Second Screen Only", SecondScreenOnly, selectedTopologyId == DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_EXTERNAL);

      _notifyIcon.ContextMenu.MenuItems.Add("-");


      // other
      _notifyIcon.ContextMenu.MenuItems.Add(startWithWindowsMenuItem);
      _notifyIcon.ContextMenu.MenuItems.Add(exitMenuItem);

      _notifyIcon.Visible = true;
    }

    private void AddMenuItem(string text, EventHandler onClick, bool defaultItem)
    {
      // second screen only
      var menuItem = new MenuItem
      {
        DefaultItem = defaultItem,
        Text = text
      };

      menuItem.Click += onClick;
      _notifyIcon.ContextMenu.MenuItems.Add(menuItem);
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
      var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
      var version = assemblyVersion.Major + "." + assemblyVersion.Minor + "." + assemblyVersion.Revision;

      if (Properties.Settings.Default.ShowMessage)
        MessageBox.Show($@"Display Switcher v{version}{Environment.NewLine}{Environment.NewLine}© Greg Taylor 2020{Environment.NewLine}greg@gregnz.com", "Display Switcher");
    }

    void Exit(object sender, EventArgs e)
    {
      // We must manually tidy up and remove the icon before we exit.
      // Otherwise it will be left behind until the user mouses over.
      _notifyIcon.Visible = false;

      Application.Exit();
    }

    private void PCScreenOnly(object sender, EventArgs e)
    {
      SwitchScreen(sender, DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_INTERNAL);
    }

    private void Duplicate(object sender, EventArgs e)
    {
      SwitchScreen(sender, DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_INTERNAL);
    }

    private void Extend(object sender, EventArgs e)
    {
      SwitchScreen(sender, DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_EXTEND);
    }

    private void SecondScreenOnly(object sender, EventArgs e)
    {
      SwitchScreen(sender, DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_EXTERNAL);
    }

    private void SwitchScreen(object sender, DISPLAYCONFIG_TOPOLOGY_ID topologyId)
    {
      NativeMethods.SetTopologyId(topologyId);

      foreach (var menuItem in _notifyIcon.ContextMenu.MenuItems)
      {
        ((MenuItem)menuItem).DefaultItem = false;
      }

      ((MenuItem)sender).DefaultItem = true;
    }

  }
}
