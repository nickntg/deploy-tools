using System;
using System.Text;

namespace DeployTools.Core.Helpers
{
    public static class RandomExtensions
    {
        private static readonly Random Rnd = new();

        private static readonly string[] Adjectives =
        [
            "Bright", "Swift", "Clever", "Happy", "Mighty", "Brave", "Wise", "Quick", "Lively", "Bold"
        ];

        private static readonly string[] Nouns =
        [
            "Falcon", "Tiger", "Eagle", "Panther", "Dragon", "Lion", "Wolf", "Hawk", "Shark", "Puma"
        ];

        private const string NumericCharactersAndLetters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GenerateDatabaseName()
        {
            var adjective = Adjectives[Rnd.Next(Adjectives.Length)];

            var noun = Nouns[Rnd.Next(Nouns.Length)];

            var number = Rnd.Next(1000, 9999);

            return $"{adjective}{noun}{number}";
        }

        public static string GenerateRandomPassword(this int length)
        {
            return Generate(NumericCharactersAndLetters, length);
        }

        private static string Generate(string dictionary, int length)
        {
            var sb = new StringBuilder();
            for (var i = 1; i <= length; i++)
            {
                sb.Append(dictionary[Rnd.Next(0, dictionary.Length)]);
            }

            return sb.ToString();
        }
    }
}
