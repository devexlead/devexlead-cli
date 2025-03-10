﻿using DevEx.Core.Storage;
using System.Text.RegularExpressions;

namespace DevEx.Core.Helpers
{
    public static class VariableHelper
    {
        public static string ReplacePlaceholders(string input)
        {
            return Regex.Replace(input, @"\{\{(\w+)\}\}", match =>
            {
                var key = match.Groups[1].Value;
                var placeHolder = UserStorageManager.GetDecryptedValue(key) ?? match.Value;
                placeHolder = ReplacePlaceholders(placeHolder); // Recursively replace placeholders
                return placeHolder;
            });
        }
    }
}
