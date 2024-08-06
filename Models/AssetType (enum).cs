using System.ComponentModel;

namespace AssetInventory.Models;

public enum AssetType
{
    [Description("Unknown asset type")]
    Unknown,
    [Description("A mobile phone")]
    Phone,
    [Description("A tablet")]
    Tablet,
    [Description("A laptop")]
    Laptop,
    [Description("A stationary computer")]
    Desktop,
}