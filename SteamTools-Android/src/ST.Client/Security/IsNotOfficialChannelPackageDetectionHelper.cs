using System.Application.Models;
using System.Application.UI.Resx;
using APIConst = System.Application.Services.CloudService.Constants;

namespace System.Application.Security
{
    public static class IsNotOfficialChannelPackageDetectionHelper
    {
        /// <summary>
        /// <see cref="AppSettings.IsOfficialChannelPackage"/>
        /// </summary>
        /// <param name="showMessageBox"></param>
        /// <returns></returns>
        public static bool Check(bool showMessageBox = true) => true;
    }
}