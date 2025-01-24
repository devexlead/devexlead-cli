namespace DevEx.Plugins.Vault
{
    public static class StringExtension
    {
        public static string Encrypt(this string str)
        {
            return VaultHelper.Encrypt(str);
        }

        public static string Decrypt(this string str)
        {
            return VaultHelper.Decrypt(str);
        }
    }
}
