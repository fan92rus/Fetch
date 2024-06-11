namespace UniversalProductScraper.Extensions
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public static class StringEx
    {

        /// <summary>
        /// Хэширование строки в MD5
        /// </summary>
        /// <param name="input">входная строка для получения хэша</param>
        /// <returns>Строка с хэшем</returns>
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private static readonly Random random = new Random();

        /// <summary>
        /// Получение случайной строки с использованием Random
        /// </summary>
        /// <param name="length">Длинна целевой строки</param>
        /// <returns>строка состоящая из случайного набора символов</returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Удаление спецсимволов unicode из текста
        /// </summary>
        /// <param name="input">Строка для обработки</param>
        /// <returns>Строка очищенная от спецсимволов</returns>
        public static string RemoveNotValidChars(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            var remover = new Regex("(?![\u0000-\u00FF]|[\u20A0-\u20CF])(.)");
            return remover.Replace(input, "");
        }

        /// <summary>
        /// Удаление html тегов из строки
        /// </summary>
        /// <param name="str">строка для обработки</param>
        /// <returns>строка очищенная от html тегов</returns>
        public static string RemoveTags(this string str) => HttpUtility.HtmlDecode(Regex.Replace(str, "<[^>]+>\\s+(?=<)|<[^>]+>", ""));
        public static string RemoveSpaces(this string str) => Regex.Replace(str.Replace("\n", "").Replace("\r", ""), "\\s+", " ");
    }
}