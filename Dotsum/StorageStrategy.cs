using System;

namespace Dotsum;

public enum StorageStrategy
{
    Default = 0,
    OneObject = 1,
    OneFieldPerType = 2,
    NoBoxing = 3,
}