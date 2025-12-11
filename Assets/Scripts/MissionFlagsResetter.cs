using UnityEngine;

public static class MissionFlagsResetter
{
    public static void ResetAllFlags()
    {
        var flags = Resources.Load<MissionFlagsSO>("MissionFlags");
        if (flags != null) flags.ResetAll();
    }
}