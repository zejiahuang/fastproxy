using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Titanium.Web.Proxy.Helpers
{
    /// <summary>
    ///     Run time helpers
    /// </summary>
    public static class RunTime
    {
        /// <summary>
        ///     Is running on Mono?
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsRunningOnMono() => OperatingSystem2.IsRunningOnMono();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLinux() => OperatingSystem2.IsLinux();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows() => OperatingSystem2.IsWindows();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUwpOnWindows() => OperatingSystem2.IsWindows() && OperatingSystem2.IsRunningAsUwp();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMac() => OperatingSystem2.IsMacOS();

        /// <summary>
        /// Is socket reuse available to use?
        /// </summary>
        public static bool IsSocketReuseAvailable => isSocketReuseAvailable();

        private static bool? _isSocketReuseAvailable;

        private static bool isSocketReuseAvailable()
        {
            // use the cached value if we have one
            if (_isSocketReuseAvailable != null)
                return _isSocketReuseAvailable.Value;

            try
            {
                if (IsWindows())
                {
                    // since we are on windows just return true
                    // store the result in our static object so we don't have to be bothered going through all this more than once
                    _isSocketReuseAvailable = true;
                    return true;
                }

                // get the currently running framework name and version (EX: .NETFramework,Version=v4.5.1) (Ex: .NETCoreApp,Version=v2.0)
                string? ver = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

                if (ver == null)
                    return false; // play it safe if we can not figure out what the framework is

                // make sure we are on .NETCoreApp
                ver = ver.ToLower(); // make everything lowercase to simplify comparison
                if (ver.Contains(".netcoreapp"))
                {
                    var versionString = ver.Replace(".netcoreapp,version=v", "");
                    var versionArr = versionString.Split('.');
                    var majorVersion = Convert.ToInt32(versionArr[0]);

                    var result = majorVersion >= 3; // version 3 and up supports socket reuse

                    // store the result in our static object so we don't have to be bothered going through all this more than once
                    _isSocketReuseAvailable = result;
                    return result;
                }

                // store the result in our static object so we don't have to be bothered going through all this more than once
                _isSocketReuseAvailable = false;
                return false;
            }
            catch
            {
                // store the result in our static object so we don't have to be bothered going through all this more than once
                _isSocketReuseAvailable = false;
                return false;
            }
        }
    }
}
