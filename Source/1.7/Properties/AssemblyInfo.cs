using System.Reflection;
using System.Runtime.InteropServices;

// Kept explicit (GenerateAssemblyInfo=false in the csproj) so the release flow can bump
// AssemblyVersion/AssemblyFileVersion in lockstep with About.xml's <modVersion> and
// CHANGELOG.md. Four-part X.Y.Z.0, where X.Y.Z is the mod's semantic version.
[assembly: AssemblyTitle("MyRimWorldMod")]
[assembly: AssemblyDescription("A RimWorld mod")]
[assembly: AssemblyProduct("MyRimWorldMod")]
[assembly: AssemblyCopyright("Copyright © 2026")]
[assembly: ComVisible(false)]
[assembly: Guid("00000000-0000-0000-0000-000000000000")]
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]
// Mirrors About.xml <modVersion> verbatim, including any SemVer prerelease suffix
// (1.4.0-rc.1); the two numeric attributes above can't hold one and stay X.Y.Z.0.
[assembly: AssemblyInformationalVersion("0.1.0")]
