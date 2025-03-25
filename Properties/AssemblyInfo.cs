using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MelonLoader;
using OBS_Control_API; // The namespace of your mod class
// ...
[assembly: MelonInfo(typeof(OBS_Control_API.MyMod), OBS_Control_API.BuildInfo.ModName, OBS_Control_API.BuildInfo.ModVersion, OBS_Control_API.BuildInfo.Author)]
[assembly: VerifyLoaderVersion(0, 6, 5, true)]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: MelonColor(255, 255, 31, 90)]
[assembly: MelonAuthorColor(255, 255, 31, 90)]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(OBS_Control_API.BuildInfo.ModName)]
[assembly: AssemblyDescription(OBS_Control_API.BuildInfo.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(OBS_Control_API.BuildInfo.Company)]
[assembly: AssemblyProduct(OBS_Control_API.BuildInfo.ModName)]
[assembly: AssemblyCopyright("Copyright ©  2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("621d30a5-8fa1-4d87-9826-92c0149b033e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(OBS_Control_API.BuildInfo.ModVersion)]
[assembly: AssemblyFileVersion(OBS_Control_API.BuildInfo.ModVersion)]
