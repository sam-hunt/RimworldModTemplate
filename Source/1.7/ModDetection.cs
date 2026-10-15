using Verse;

namespace MyRimWorldMod;

// Single place to ask "is mod X active?" from C#.
//
// Do NOT use ModsConfig.IsActive(string) for this: it is an exact match against the ids
// stored in ModsConfig.xml, and Steam Workshop installs are stored with a "_steam" postfix
// (somebody.somemod_steam), so it returns false for every Workshop build of the mod and true
// only for local copies in Mods/. Dev testing almost always uses local copies, so the bug
// hides until a player reports it (BetterTradersGuild, UniqueWeaponsUnbound and
// PersonaWeaponsUnbound all shipped it). The engine's own MayRequire, IfModActive and
// PatchOperation gates go through ModLister's NoSuffix helpers, which is what this wraps,
// so C# detection agrees with the XML gates. Verified on 1.7.4979.
internal static class ModDetection
{
    public static bool IsActive(string packageId)
    {
        return ModLister.GetActiveModWithIdentifier(packageId, ignorePostfix: true) != null;
    }
}
