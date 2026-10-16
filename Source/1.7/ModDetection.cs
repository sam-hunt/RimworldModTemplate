using Verse;

namespace MyRimWorldMod;

// Single place to ask "is mod X active?" from C#.
//
// Do NOT use ModsConfig.IsActive(string) for this: it is an exact match against the ids
// stored in ModsConfig.xml, and when the same packageId is installed both locally and from
// the Steam Workshop, ModLister.TryAddMod keeps both by giving the Workshop copy a "_steam"
// postfix (somebody.somemod_steam). With that Workshop copy active, the exact match misses
// it. A Workshop-only install keeps the plain id, so this mostly bites a developer whose
// local dev copies sit beside the subscribed builds (BetterTradersGuild hit it that way).
// The engine's own MayRequire, IfModActive and PatchOperation gates go through ModLister's
// NoSuffix helpers, which is what this wraps, so C# detection agrees with the XML gates.
// Verified on 1.7.4979.
internal static class ModDetection
{
    public static bool IsActive(string packageId)
    {
        return ModLister.GetActiveModWithIdentifier(packageId, ignorePostfix: true) != null;
    }
}
