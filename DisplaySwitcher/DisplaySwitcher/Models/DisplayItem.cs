using System.Collections.Generic;

namespace DisplaySwitcher.Models
{
  public class DisplayItem
  {
    public string Name
    {
      get
      {
        switch (TopologyId)
        {
          case DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_INTERNAL:
            return "PC Screen Only";
          case DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_CLONE:
            return "Duplicate";
          case DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_EXTEND:
            return "Extend";
          case DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_EXTERNAL:
            return "Second Screen Only";
          default:
            throw new KeyNotFoundException($"Unhandled TopologyId {TopologyId}");
        }
      }
    }
    public DISPLAYCONFIG_TOPOLOGY_ID TopologyId { get; set; }

    public bool Selected { get; set; }

  }
}
