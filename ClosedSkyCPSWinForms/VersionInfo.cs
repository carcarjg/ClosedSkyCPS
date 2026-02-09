// %%%%%%    @%%%%%@
//%%%%%%%%   %%%%%%%@
//@%%%%%%%@  %%%%%%%%%        @@      @@  @@@      @@@ @@@     @@@ @@@@@@@@@@   @@@@@@@@@
//%%%%%%%%@ @%%%%%%%%       @@@@@   @@@@ @@@@@   @@@@ @@@@   @@@@ @@@@@@@@@@@@@@@@@@@@@@@ @@@@
// @%%%%%%%%  %%%%%%%%%      @@@@@@  @@@@  @@@@  @@@@   @@@@@@@@@     @@@@    @@@@         @@@@
//  %%%%%%%%%  %%%%%%%%@     @@@@@@@ @@@@   @@@@@@@@     @@@@@@       @@@@    @@@@@@@@@@@  @@@@
//   %%%%%%%%@  %%%%%%%%%    @@@@@@@@@@@@     @@@@        @@@@@       @@@@    @@@@@@@@@@@  @@@@
//    %%%%%%%%@ @%%%%%%%%    @@@@ @@@@@@@     @@@@      @@@@@@@@      @@@@    @@@@         @@@@
//    @%%%%%%%%% @%%%%%%%%   @@@@   @@@@@     @@@@     @@@@@ @@@@@    @@@@    @@@@@@@@@@@@ @@@@@@@@@@
//     @%%%%%%%%  %%%%%%%%@  @@@@    @@@@     @@@@    @@@@     @@@@   @@@@    @@@@@@@@@@@@ @@@@@@@@@@@
//      %%%%%%%%@ @%%%%%%%%
//      @%%%%%%%%  @%%%%%%%%
//       %%%%%%%%   %%%%%%%@
//         %%%%%      %%%%
//
// Copyright (C) 2025-2026 NyxTel Wireless / Nyx Gallini
//
using System.Reflection;

namespace ClosedSkyCPSWinForms
{
    internal static class VersionInfo
    {
        public static string GetVersion()
        {
            // Try to get the custom version from assembly metadata
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            
            if (versionAttr != null && !string.IsNullOrEmpty(versionAttr.InformationalVersion))
            {
                // Strip any build metadata after '+' sign (e.g., "CSP 01A1+g1234567" → "CSP 01A1")
                string version = versionAttr.InformationalVersion;
                int plusIndex = version.IndexOf('+');
                if (plusIndex > 0)
                {
                    version = version.Substring(0, plusIndex);
                }
                return version.Trim();
            }
            
            // Fallback to reading from AssemblyVersion
            var numVersion = assembly.GetName().Version;
            if (numVersion != null)
            {
                // Convert numeric version back to CSP format
                // Version is stored as 1.LetterNum.Number.0
                // Where LetterNum: A=1, B=2, etc.
                int letterNum = numVersion.Minor;
                int number = numVersion.Build;
                
                if (letterNum >= 1 && letterNum <= 26 && number >= 1 && number <= 9)
                {
                    char letter = (char)('A' + letterNum - 1);
                    return $"CSP 01{letter}{number}";
                }
            }
            
            return "CSP 01A1"; // Default fallback
        }
        
        public static string GetNumericVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0.0";
        }
    }
}
