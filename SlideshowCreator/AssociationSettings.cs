using Microsoft.Win32;
using System;

namespace SlideshowCreator
{

    /*
     * File association parts stolen from: https://stackoverflow.com/a/44816953
     */

    /// <summary>
    /// A utils class to create file associations
    /// </summary>
    public class AssociationSettings
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        /// <summary>
        /// Ensures the association is set
        /// </summary>
        /// <param name="extension">The extension of the association</param>
        /// <param name="progid">A program id</param>
        /// <param name="fileTypeDescription">A file type description</param>
        /// <param name="applicationFilePath">The path to the application executeable</param>
        public void EnsureAssociationsSet(string extension, string progid, string fileTypeDescription, string applicationFilePath)
        {
            bool madeChanges = SetAssociation(extension, progid, fileTypeDescription, applicationFilePath);

            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Sets the associations and returns if it made changes
        /// </summary>
        /// <param name="extension">The extension of the association</param>
        /// <param name="progid">A program id</param>
        /// <param name="fileTypeDescription">A file type description</param>
        /// <param name="applicationFilePath">The path to the application executeable</param>
        /// <returns>true if changes were made</returns>
        public bool SetAssociation(string extension, string progId, string fileTypeDescription, string applicationFilePath)
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", "\"" + applicationFilePath + "\" \"%1\"");
            return madeChanges;
        }

        /// <summary>
        /// Utility function to set registry values
        /// </summary>
        /// <param name="keyPath">path to the key</param>
        /// <param name="value">value to set</param>
        /// <returns></returns>
        private bool SetKeyDefaultValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key.GetValue(null) as string != value)
                {
                    key.SetValue(null, value);
                    return true;
                }
            }

            return false;
        }
    }
}
