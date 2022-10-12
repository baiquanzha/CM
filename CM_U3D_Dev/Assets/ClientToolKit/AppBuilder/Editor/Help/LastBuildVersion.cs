using System.Collections;
using System.Collections.Generic;
using System;
using MTool.AppUpdaterLib.Runtime;

[Serializable]
public sealed class LastBuildVersion
{
    public string Version = null;
    public VersionManifest Info;
}