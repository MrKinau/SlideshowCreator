﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator
{
    /*
     * File association parts stolen from: https://stackoverflow.com/a/44816953
     */
    public class Settings
    {
        public string SavingPath;

        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public void EnsureAssociationsSet(string extension, string progid, string fileTypeDescription, string applicationFilePath)
        {
            bool madeChanges = SetAssociation(extension, progid, fileTypeDescription, applicationFilePath);

            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public bool SetAssociation(string extension, string progId, string fileTypeDescription, string applicationFilePath)
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", "\"" + applicationFilePath + "\" \"%1\"");
            return madeChanges;
        }

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
