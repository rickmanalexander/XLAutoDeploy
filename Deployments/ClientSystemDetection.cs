using XLAutoDeploy.Manifests;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace XLAutoDeploy.Deployments
{
    internal class ClientSystemDetection
    {
        public const string V1To4SubKey32Bit = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\";
        public const string V1To4SubKey64Bit = @"SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\";
        public const string V45PlusSubKey32Bit = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
        public const string V45PlusSubKey64Bit = @"SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\v4\Full\";

        public enum NetFrameworkVersionSet
        {
            V1To4 = 1,
            V45Plus = 2
        }

        private static readonly IDictionary<string, KeyValuePair<NetFrameworkVersionSet, OperatingSystemBitness>> NetFrameworkRegistryKeys = 
            new Dictionary<string, KeyValuePair<NetFrameworkVersionSet, OperatingSystemBitness>>(4)
        {
            [V1To4SubKey32Bit] = new KeyValuePair<NetFrameworkVersionSet, OperatingSystemBitness>(NetFrameworkVersionSet.V1To4, OperatingSystemBitness.X86),
            [V1To4SubKey64Bit] = new KeyValuePair<NetFrameworkVersionSet, OperatingSystemBitness>(NetFrameworkVersionSet.V1To4, OperatingSystemBitness.X64),
            [V45PlusSubKey32Bit] = new KeyValuePair<NetFrameworkVersionSet, OperatingSystemBitness>(NetFrameworkVersionSet.V45Plus, OperatingSystemBitness.X86),
            [V45PlusSubKey64Bit] = new KeyValuePair<NetFrameworkVersionSet, OperatingSystemBitness>(NetFrameworkVersionSet.V45Plus, OperatingSystemBitness.X64)
        };

        public const string MsOfficeVersionBaseKey = @"SOFTWARE\Microsoft\Office\";

        private static readonly IDictionary<string, double> MsOfficeVersionRegistryKeys = new Dictionary<string, double>(5)
        {
            [MsOfficeVersionBaseKey + @"11.0\Outlook"] = 11.0,  // 2003
            [MsOfficeVersionBaseKey + @"12.0\Outlook"] = 12.0,  // 2007
            [MsOfficeVersionBaseKey + @"14.0\Outlook"] = 14.0,  // 2010; 64 bit office was introduced here
            [MsOfficeVersionBaseKey + @"15.0\Outlook"] = 15.0,  // 2013
            [MsOfficeVersionBaseKey + @"16.0\Outlook"] = 16.0   // 2016, 2019; Note that office 2019 uses the same version key
        };

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern NTSTATUS RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OSVERSIONINFOEX
        {
            // The OSVersionInfoSize field must be set to Marshal.SizeOf(typeof(OSVERSIONINFOEX))
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string CSDVersion;
            internal ushort ServicePackMajor;
            internal ushort ServicePackMinor;
            internal short SuiteMask;
            internal byte ProductType;
            internal byte Reserved;
        }

        private enum NTSTATUS : uint
        {
            /// <summary>
            /// The operation completed successfully. 
            /// </summary>
            STATUS_SUCCESS = 0x00000000
        }

        private enum BinaryType : uint
        {
            SCS_32BIT_BINARY = 0,   // A 32-bit Windows-based application
            SCS_DOS_BINARY = 1,     // An MS-DOS – based application
            SCS_WOW_BINARY = 2,     // A 16-bit Windows-based application 
            SCS_PIF_BINARY = 3,     // A PIF file that executes an MS-DOS – based application
            SCS_POSIX_BINARY = 4,   // A POSIX – based application
            SCS_OS216_BINARY = 5,   // A 16-bit OS/2-based application
            SCS_64BIT_BINARY = 6    // A 64-bit Windows-based application.
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetBinaryType(string lpApplicationName, out BinaryType lpBinaryType);

        public static bool IsWindowsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();

            if (identity is not null)
            {
                var sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                var principal = new WindowsPrincipal(identity);

                return principal?.UserClaims?.Any(x => x?.Value?.Contains(sid?.Value) == true) ?? false;
            }

            return false;
        }

        public static System.Version GetOsVersion()
        {
            var osVersionInfo = new OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)) };

            if (RtlGetVersion(ref osVersionInfo) != NTSTATUS.STATUS_SUCCESS)
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Retrieving System Version Info.",
                            $"Could not retrive system version info.",
                            $"N/A."));

            return new System.Version(osVersionInfo.MajorVersion, osVersionInfo.MinorVersion, osVersionInfo.BuildNumber); 
        }

        public static OperatingSystemBitness GetOsBitness()
        {
            return Environment.Is64BitOperatingSystem ? OperatingSystemBitness.X64 : OperatingSystemBitness.X86;
        }

        // Only one version of office can be installed at a time. That version can be either 32 or 64 bit, but both cannot be installed side-by-side.
        public static MicrosoftOfficeBitness GetMicrosoftOfficeBitness()
        {
            const string excelExePath = @"Software\Microsoft\Windows\CurrentVersion\App Paths\excel.exe";

            BinaryType binaryType;
            using (var key = Registry.LocalMachine.OpenSubKey(excelExePath))
            {
                var excel = key.GetValue("");
                GetBinaryType(excel.ToString(), out binaryType);
            }

            switch (binaryType)
            {
                case BinaryType.SCS_32BIT_BINARY:
                    return MicrosoftOfficeBitness.Bit32;

                case BinaryType.SCS_64BIT_BINARY:
                    return MicrosoftOfficeBitness.Bit64;

                default:
                    return MicrosoftOfficeBitness.Unknown; 

            }
        }
        /*
        public static MicrosoftOfficeBitness GetMicrosoftOfficeBitness()
        {
            var versionKeys = MsOfficeVersionRegistryKeys; 

            foreach (var kvp in versionKeys)
            {
                string versionKey = kvp.Key;
                double version = kvp.Value;

                // 64 bit office was introduced here
                if (version >= 14)
                {
                    using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
                    {
                        using (RegistryKey subKey = baseKey.OpenSubKey(versionKey, false))
                        {
                            if (subKey is not null)
                            {
                                var value = subKey.GetValue("Bitness").ToString();
                                if (value is not null)
                                {
                                    if (value == "x64")
                                    {
                                        return MicrosoftOfficeBitness.Bit64;
                                    }
                                    else
                                    {
                                        return MicrosoftOfficeBitness.Bit32;
                                    }
                                }
                                else
                                {
                                    return MicrosoftOfficeBitness.Bit32;
                                }
                            }
                            else
                            {
                                if (Environment.Is64BitOperatingSystem)
                                {
                                    using (RegistryKey baseKey2 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                                    {
                                        using (RegistryKey subKey2 = baseKey2.OpenSubKey(versionKey, false))
                                        {
                                            if (subKey2 is not null)
                                            {
                                                var value = subKey2.GetValue("Bitness").ToString();
                                                if (value is not null)
                                                {
                                                    if (value == "x64")
                                                    {
                                                        return MicrosoftOfficeBitness.Bit64;
                                                    }
                                                    else
                                                    {
                                                        return MicrosoftOfficeBitness.Bit32;
                                                    }
                                                }
                                                else
                                                {
                                                    return MicrosoftOfficeBitness.Bit32;
                                                }
                                            }
                                            else
                                            {
                                                return MicrosoftOfficeBitness.Bit32;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return MicrosoftOfficeBitness.Bit32;
                                }
                            }
                        }
                    }
                }
            }

            return MicrosoftOfficeBitness.Bit32;
        }
        */

        public static IDictionary<NetClrVersion, HashSet<System.Version>> GetAllInstalledClrAndNetFrameworkVersions()
        {
            var V1To4Installations = Get1To4NetFrameworkVersionsFromRegistry();
            var v45PlusInstallations = Get45PlusNetFrameworkVersionsFromRegistry();

            foreach (var clrAndVersionHashSet in V1To4Installations)
            {
                if (!v45PlusInstallations.ContainsKey(clrAndVersionHashSet.Key))
                {
                    v45PlusInstallations.Add(clrAndVersionHashSet.Key, clrAndVersionHashSet.Value);
                }
                else
                {
                    var hashset = v45PlusInstallations[clrAndVersionHashSet.Key];

                    foreach(var version in clrAndVersionHashSet.Value)
                    {
                        if (!hashset.Contains(version))
                        {
                            hashset.Add(version);
                        }
                    }

                    v45PlusInstallations[clrAndVersionHashSet.Key] = hashset;
                }
            }

            return v45PlusInstallations;
        }

        private static IDictionary<NetClrVersion, HashSet<System.Version>> Get1To4NetFrameworkVersionsFromRegistry()
        {
            var result = new Dictionary<NetClrVersion, HashSet<System.Version>>();

            //1.0 to 4.0
            var keys = NetFrameworkRegistryKeys.Where(x => x.Value.Key == NetFrameworkVersionSet.V1To4);
            foreach (var key in keys)
            {
                var registryView = key.Value.Value == OperatingSystemBitness.X86 ? RegistryView.Registry32 : RegistryView.Registry64;

                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView)
                        .OpenSubKey(key.Key))
                {
                    foreach (var versionKeyName in ndpKey.GetSubKeyNames())
                    {
                        // Skip .NET Framework 4.5 versionValue information.
                        if (versionKeyName == "v4")
                            continue;

                        if (versionKeyName.StartsWith("v"))
                        {
                            var versionKey = ndpKey.OpenSubKey(versionKeyName);
                            var install = versionKey.GetValue("Install", "").ToString(); // installation flag.

                            var versionValue = (string)versionKey.GetValue("Version", "");

                            if (String.IsNullOrEmpty(install))
                            {
                                // No install info; it must be in a child subkey.
                            }
                            else if (install == "1") // Install = 1 means the versionValue is installed.
                            {
                                var clrAndVersion = Get1To4NetFrameworkVersion(versionKeyName, versionValue);

                                if (!result.ContainsKey(clrAndVersion.Key))
                                {
                                    result.Add(clrAndVersion.Key, new HashSet<System.Version>() { clrAndVersion.Value });
                                }
                                else
                                {
                                    var hashset = result[clrAndVersion.Key];

                                    if (!hashset.Contains(clrAndVersion.Value))
                                        hashset.Add(clrAndVersion.Value);

                                    result[clrAndVersion.Key] = hashset;
                                }
                            }

                            if (!String.IsNullOrEmpty(versionValue))
                                continue;

                            // else print out info from subkeys...

                            // Iterate through the subkeys of the versionValue subkey.
                            foreach (var subKeyName in versionKey.GetSubKeyNames())
                            {
                                var subKey = versionKey.OpenSubKey(subKeyName);
                                versionValue = (string)subKey.GetValue("Version", "");

                                install = subKey.GetValue("Install", "").ToString();

                                if (String.IsNullOrEmpty(install))
                                {
                                    // No install info; it must be later.
                                }
                                else if (install == "1")
                                {
                                    var clrAndVersion = Get1To4NetFrameworkVersion(versionKeyName, versionValue);

                                    if (!result.ContainsKey(clrAndVersion.Key))
                                    {
                                        result.Add(clrAndVersion.Key, new HashSet<System.Version>() { clrAndVersion.Value });
                                    }
                                    else
                                    {
                                        var hashset = result[clrAndVersion.Key];

                                        if (!hashset.Contains(clrAndVersion.Value))
                                            hashset.Add(clrAndVersion.Value);

                                        result[clrAndVersion.Key] = hashset;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static KeyValuePair<NetClrVersion, System.Version> Get1To4NetFrameworkVersion(string versionKeyName, string versionValue)
        {
            string tempString;
            tempString = versionKeyName.Contains(".") ? versionKeyName.Substring(1, 3) : versionKeyName.Substring(1);

            // See https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/versions-and-dependencies
            if (tempString == "1" || tempString == "1.0")
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V1, new System.Version(versionValue));
            
            if (tempString == "1.1")
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V11, new System.Version(versionValue));

            if (tempString == "2" || tempString == "2.0")
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V2, new System.Version(versionValue));

            if (tempString == "3" || tempString == "3.0" || tempString == "3.5")
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V2, new System.Version(versionValue));

            if (tempString == "4" || tempString == "4.0")
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version(versionValue));

            // This code should never execute.
            return new KeyValuePair<NetClrVersion, System.Version>();
        }

        private static IDictionary<NetClrVersion, HashSet<System.Version>> Get45PlusNetFrameworkVersionsFromRegistry()
        {
            var result = new Dictionary<NetClrVersion, HashSet<System.Version>>();

            //4.5 to 4.8
            var keys = NetFrameworkRegistryKeys.Where(x => x.Value.Key == NetFrameworkVersionSet.V45Plus);
            foreach (var key in keys)
            {
                var registryView = key.Value.Value == OperatingSystemBitness.X86 ? RegistryView.Registry32 : RegistryView.Registry64;

                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView)
                        .OpenSubKey(key.Key))
                {
                    if (ndpKey is not null && ndpKey.GetValue("Release") is not null)
                    {
                        KeyValuePair<NetClrVersion, System.Version> clrAndVersion;

                        //First check if there's a specific version indicated
                        if (ndpKey.GetValue("Version") is not null)
                        {
                            clrAndVersion = new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version(ndpKey.GetValue("Version").ToString()));
                        }
                        else
                        {
                            clrAndVersion = Get45PlusNetFrameworkVersion((int)ndpKey.GetValue("Release"));
                        }

                        if (!result.ContainsKey(clrAndVersion.Key))
                        {
                            result.Add(clrAndVersion.Key, new HashSet<System.Version>() { clrAndVersion.Value });
                        }
                        else
                        {
                            var hashset = result[clrAndVersion.Key];

                            if (!hashset.Contains(clrAndVersion.Value))
                                hashset.Add(clrAndVersion.Value);

                            result[clrAndVersion.Key] = hashset;
                        }
                    }
                }
            }

            return result;
        }


        private static KeyValuePair<NetClrVersion, System.Version> Get45PlusNetFrameworkVersion(int releaseKey)
        {
            // Checking the versionString using >= enables forward compatibility.
            if (releaseKey >= 528040)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.8"));  //Could be later 

            if (releaseKey >= 461808)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.7.2"));

            if (releaseKey >= 461308)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.7.1"));

            if (releaseKey >= 460798)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.7"));

            if (releaseKey >= 394802)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.6.2"));

            if (releaseKey >= 394254)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.6.1"));

            if (releaseKey >= 393295)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.6"));

            if (releaseKey >= 379893)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.5.2"));

            if (releaseKey >= 378675)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.5.1"));

            if (releaseKey >= 378389)
                return new KeyValuePair<NetClrVersion, System.Version>(NetClrVersion.V4, new System.Version("4.5"));

            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return new KeyValuePair<NetClrVersion, System.Version>();
        }
    }
}
