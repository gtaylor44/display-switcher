using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DisplaySwitcher
{
  public class NativeMethods
  {
    public const int ERROR_SUCCESS = 0;

    [DllImport("user32.dll")]
    private static extern int GetDisplayConfigBufferSizes(
        QUERY_DEVICE_CONFIG_FLAGS Flags,
        out uint NumPathArrayElements,
        out uint NumModeInfoArrayElements
    );

    [DllImport("user32.dll")]
    private static extern int QueryDisplayConfig(
        QUERY_DEVICE_CONFIG_FLAGS Flags,
        ref uint NumPathArrayElements,
        [Out] DISPLAYCONFIG_PATH_INFO[] PathInfoArray,
        ref uint NumModeInfoArrayElements,
        [Out] DISPLAYCONFIG_MODE_INFO[] ModeInfoArray,
        out DISPLAYCONFIG_TOPOLOGY_ID topologyId
    );

    [DllImport("user32.dll")]
    private static extern int DisplayConfigGetDeviceInfo(
        ref DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName
    );

    [DllImport("user32.dll")]
    private static extern int SetDisplayConfig(uint numPathArrayElements,
      IntPtr pathArray, uint numModeArrayElements, IntPtr modeArray, DISPLAYCONFIG_TOPOLOGY_ID flags);

    public static DISPLAYCONFIG_TOPOLOGY_ID GetCurrentTopologyId()
    {
      uint PathCount, ModeCount;
      int error = GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_DATABASE_CURRENT,
          out PathCount, out ModeCount);
      if (error != ERROR_SUCCESS)
        throw new Win32Exception(error);

      DISPLAYCONFIG_TOPOLOGY_ID mode;

      DISPLAYCONFIG_PATH_INFO[] DisplayPaths = new DISPLAYCONFIG_PATH_INFO[PathCount];
      DISPLAYCONFIG_MODE_INFO[] DisplayModes = new DISPLAYCONFIG_MODE_INFO[ModeCount];
      error = QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_DATABASE_CURRENT,
          ref PathCount, DisplayPaths, ref ModeCount, DisplayModes, out mode);
      if (error != ERROR_SUCCESS)
        throw new Win32Exception(error);

      return mode;
    }

    public static void SetTopologyId(DISPLAYCONFIG_TOPOLOGY_ID mode)
    {
      int error = SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, (DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_SDC_APPLY | mode));

      if (error != ERROR_SUCCESS)
        throw new Win32Exception(error);
    }

  }
}
