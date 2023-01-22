using Microsoft.Win32;

namespace ShareX
{
    public static class RegistryHelpers
    {
        public static object GetValue(string path, string name = null, RegistryHive root = RegistryHive.CurrentUser, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(root, view))
                using (RegistryKey rk = baseKey.OpenSubKey(path))
                {
                    if (rk != null)
                    {
                        return rk.GetValue(name);
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public static int? GetValueDWord(string path, string name = null, RegistryHive root = RegistryHive.CurrentUser, RegistryView view = RegistryView.Default)
        {
            return (int?)GetValue(path, name, root, view);
        }

    }
}