using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using static Utility.EnumHelper;

namespace Utility
{
    public static class ProjectHelper
    {
        #region Serialization
        public static JsonSerializerSettings GetJsonSettings()
        {
            var _jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ObjectCreationHandling = ObjectCreationHandling.Auto
            };
            return _jsonSettings;
        }
        #endregion
        #region Cryptography
        private const string initVector = "pemgail9uzpgzl88";
        private const int keysize = 256;
        public static string EncryptString(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            Aes symmetricKey = Aes.Create();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
        public static string DecryptString(string cipherText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            Aes symmetricKey = Aes.Create();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
        
        private const string key = "b14ca5898a4e4133bbce2ea2315a1916";
        public static string EncryptWithRandomIV(string plaintext)
        {
            string randomString = RandomStringGenerator(16);
            string encryptedText = string.Empty;
            if (!string.IsNullOrEmpty(plaintext))
            {
                using Aes aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = Encoding.UTF8.GetBytes(randomString);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream ms = new();
                using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
                using StreamWriter writer = new(cs);

                writer.Write(plaintext);
                writer.Flush();
                cs.FlushFinalBlock();

                byte[] encryptedBytes = ms.ToArray();
                string encryptedString = randomString + "||" + Convert.ToBase64String(encryptedBytes);
                encryptedText = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedString));
            }
            return encryptedText;
        }
        public static string DecryptWithRandomIV(string encryptedText)
        {
            string plaintext = string.Empty;
            if (!string.IsNullOrEmpty(encryptedText))
            {
                byte[] byteArray = Convert.FromBase64String(encryptedText);
                string decodedString = Encoding.UTF8.GetString(byteArray);
                string[] parts = decodedString.Split("||");
                var data = Convert.FromBase64String(parts[1]);
                using Aes aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.CBC;
                aes.IV = Encoding.UTF8.GetBytes(parts[0]);
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream ms = new(data);
                using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader reader = new(cs);
                plaintext = reader.ReadToEnd();
            }
            return plaintext;
        }
        private static string RandomStringGenerator(int length)
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(characters.Length);
                result.Append(characters[index]);
            }
            string iv = result.ToString();
            return iv;
        }
        
        public static string AesEncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new())
                {
                    using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }
        public static string AesDecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new(buffer))
                {
                    using (CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static string TripleDesEncrypt(string textToEncrypt, string key1, string key2)
        {
            TripleDES tdes = TripleDES.Create();
            tdes.Key = Encoding.ASCII.GetBytes(key1);
            tdes.IV = Encoding.ASCII.GetBytes(key2);
            byte[] buffer = Encoding.ASCII.GetBytes(textToEncrypt);
            return Convert.ToBase64String(tdes.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }
        public static string TripleDesDecrypt(string textToDecrypt, string key1, string key2)
        {
            byte[] buffer = Convert.FromBase64String(textToDecrypt);
            TripleDES des = TripleDES.Create();
            des.Key = Encoding.ASCII.GetBytes(key1);
            des.IV = Encoding.ASCII.GetBytes(key2);
            return Encoding.ASCII.GetString(des.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }
        #endregion
        #region Math
        public static double RoundUp(double input, int places)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            return Math.Ceiling(input * multiplier) / multiplier;
        }
        public static decimal Truncate(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }
        public static decimal PercentageAmount(decimal amount, decimal perchant)
        {
            decimal result = (amount * perchant) / 100;
            return result;
        }
        #endregion
        #region Transaction
        public static string RRN()
        {
            return DateTime.UtcNow.ToString("yyMMddHHmmssfffff");
        }
        public static string GenerateTrxId(string rrn)
        {
            var otpRandom = new Random();
            char padLeft = Convert.ToChar(otpRandom.Next(1, 2).ToString());
            var num = Convert.ToInt64(rrn + otpRandom.Next(1, 99).ToString().PadLeft(2, padLeft));
            return DecimalToArbitrarySystem(num, 36);
        }
        public static string DecimalToArbitrarySystem(long decimalNumber, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());

            if (decimalNumber == 0)
                return "0";

            int index = BitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }
        #endregion
        #region Imaging
        public static ImageFormat GetImageFormat(byte[] bytes)
        {
            var bmp = Encoding.ASCII.GetBytes("BM");
            var gif = Encoding.ASCII.GetBytes("GIF");
            var png = new byte[] { 137, 80, 78, 71 };
            var tiff = new byte[] { 73, 73, 42 };
            var tiff2 = new byte[] { 77, 77, 42 };
            var jpeg = new byte[] { 255, 216, 255, 224 };
            var jpeg2 = new byte[] { 255, 216, 255, 225 };
            var pdf = new byte[] { 37, 80, 68, 70 };

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;
            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.jpeg;
            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpeg;
            if (pdf.SequenceEqual(bytes.Take(pdf.Length)))
                return ImageFormat.PDF;
            return ImageFormat.unknown;
        }
        #endregion
        #region Enum
        public static string GetEnumDescription(this Enum enumValue)
        {
            return enumValue.GetType()
                       .GetMember(enumValue.ToString())
                       .First()
                       .GetCustomAttribute<DescriptionAttribute>()?
                       .Description ?? string.Empty;
        }
        #endregion
        #region Password
        public static string GenerateHashPassword(string password, byte[]? salt = null, bool needsOnlyHash = false)
        {
            if (salt == null || salt.Length != 16)
            {
                salt = new byte[128 / 8];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(salt);
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            if (needsOnlyHash) return hashed;
            return $"{hashed}:{Convert.ToBase64String(salt)}";
        }
        public static bool VerifyPassword(string hashedPasswordWithSalt, string passwordToCheck)
        {
            var passwordAndHash = hashedPasswordWithSalt.Split(':');
            if (passwordAndHash == null || passwordAndHash.Length != 2)
                return false;
            var salt = Convert.FromBase64String(passwordAndHash[1]);
            if (salt == null)
                return false;
            var hashOfpasswordToCheck = GenerateHashPassword(passwordToCheck, salt, true);
            return String.Compare(passwordAndHash[0], hashOfpasswordToCheck) == 0;
        }
        #endregion
        #region DateTime
        public static int GetNumberOfDaysInMonth(int month)
        {
            int[] daysList = { 31, 25, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            return daysList[(month - 1)];
        }
        public static IEnumerable<DateTime> DaysBetween(DateTime start, DateTime end)
        {
            var current = start;
            if (current != current.Date)
                current = current.AddDays(1).Date;
            while (current < end)
            {
                yield return current;
                current = current.AddDays(1);
            }
        }
        public static DateTime GetLastDayOfPreviousMonth(this DateTime date)
        {
            return date.AddDays(-date.Day);
        }
        #endregion
        #region Mobile
        public static KeyValuePair<string, bool> FormatMobileNumber(String MobileNumber)
        {
            var mobile_no = MobileNumber.Replace("-", "").Replace(" ", "");
            var mobileNumberFormat = mobile_no.Substring(0, 3);
            if (mobileNumberFormat == "+88")
            {
                mobileNumberFormat = mobile_no.Replace(" ", String.Empty);
            }
            else if (mobileNumberFormat == "880")
            {
                var imobileNumberFormat = "+" + mobile_no;
                mobileNumberFormat = imobileNumberFormat.Replace(" ", String.Empty);
            }
            else
            {
                var imobileNumberFormat = "+88" + mobile_no;
                mobileNumberFormat = imobileNumberFormat.Replace(" ", String.Empty);
            }

            var isMobileNumber = IsMobileNumber(mobileNumberFormat);
            if (isMobileNumber == false)
            {
                return new KeyValuePair<string, bool>("Invalid Phone Number", false);
            }
            return new KeyValuePair<string, bool>(mobileNumberFormat, true);
        }
        public static bool IsMobileNumber(string number)
        {
            return Regex.Match(number, @"^\+8801([13-9]\d{1})[\d]{7}$").Success;
        }
        #endregion
        #region PromoCode
        public static string GeneratePromoCode(long num)
        {
            return DecimalToArbitrarySystemBase34(num, 34);
        }
        public static string DecimalToArbitrarySystemBase34(long decimalNumber, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "23456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());

            if (decimalNumber == 0)
                return "0";

            int index = BitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }
        #endregion
        #region Currency
        public static bool TryParseCurrency(string input, out Currency currency)
        {
            currency = default;
            if (Enum.TryParse<Currency>(input, ignoreCase: true, out currency))
            {
                return true;
            }
            string trimmedInput = input.TrimStart('0');
            foreach (var kvp in CurrencyCode.NumericCodes)
            {
                if (kvp.Value.TrimStart('0') == trimmedInput)
                {
                    currency = kvp.Key;
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region Ping
        public static async Task<bool> PingAsync(string url)
        {
            Ping ping = new();
            PingReply result = await ping.SendPingAsync(url);
            return result.Status == IPStatus.Success;
        }
        #endregion
        #region String
        public static string ToTitleCase(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ' || c == ',')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static string AddDoubleQuotes(this string value)
        {
            return "\"" + value + "\"";
        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion
        #region Imoji
        public static bool CheckIfTextContainsEmoji(string text_value)
        {
            bool status = false;
            var EmojiPattern = "\"[#*0-9]\\uFE0F?\\u20E3|\\u00A9\\uFE0F?|[\\u00AE\\u203C\\u2049\\u2122\\u2139\\u2194-\\u2199\\u21A9\\u21AA]\\uFE0F?|[\\u231A\\u231B]|[\\u2328\\u23CF]\\uFE0F?|[\\u23E9-\\u23EC]|[\\u23ED-\\u23EF]\\uFE0F?|\\u23F0|[\\u23F1\\u23F2]\\uFE0F?|\\u23F3|[\\u23F8-\\u23FA\\u24C2\\u25AA\\u25AB\\u25B6\\u25C0\\u25FB\\u25FC]\\uFE0F?|[\\u25FD\\u25FE]|[\\u2600-\\u2604\\u260E\\u2611]\\uFE0F?|[\\u2614\\u2615]|\\u2618\\uFE0F?|\\u261D(?:\\uD83C[\\uDFFB-\\uDFFF]|\\uFE0F)?|[\\u2620\\u2622\\u2623\\u2626\\u262A\\u262E\\u262F\\u2638-\\u263A\\u2640\\u2642]\\uFE0F?|[\\u2648-\\u2653]|[\\u265F\\u2660\\u2663\\u2665\\u2666\\u2668\\u267B\\u267E]\\uFE0F?|\\u267F|\\u2692\\uFE0F?|\\u2693|[\\u2694-\\u2697\\u2699\\u269B\\u269C\\u26A0]\\uFE0F?|\\u26A1|\\u26A7\\uFE0F?|[\\u26AA\\u26AB]|[\\u26B0\\u26B1]\\uFE0F?|[\\u26BD\\u26BE\\u26C4\\u26C5]|\\u26C8\\uFE0F?|\\u26CE|[\\u26CF\\u26D1\\u26D3]\\uFE0F?|\\u26D4|\\u26E9\\uFE0F?|\\u26EA|[\\u26F0\\u26F1]\\uFE0F?|[\\u26F2\\u26F3]|\\u26F4\\uFE0F?|\\u26F5|[\\u26F7\\u26F8]\\uFE0F?|\\u26F9(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?|\\uFE0F(?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\u26FA\\u26FD]|\\u2702\\uFE0F?|\\u2705|[\\u2708\\u2709]\\uFE0F?|[\\u270A\\u270B](?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\u270C\\u270D](?:\\uD83C[\\uDFFB-\\uDFFF]|\\uFE0F)?|\\u270F\\uFE0F?|[\\u2712\\u2714\\u2716\\u271D\\u2721]\\uFE0F?|\\u2728|[\\u2733\\u2734\\u2744\\u2747]\\uFE0F?|[\\u274C\\u274E\\u2753-\\u2755\\u2757]|\\u2763\\uFE0F?|\\u2764(?:\\u200D(?:\\uD83D\\uDD25|\\uD83E\\uDE79)|\\uFE0F(?:\\u200D(?:\\uD83D\\uDD25|\\uD83E\\uDE79))?)?|[\\u2795-\\u2797]|\\u27A1\\uFE0F?|[\\u27B0\\u27BF]|[\\u2934\\u2935\\u2B05-\\u2B07]\\uFE0F?|[\\u2B1B\\u2B1C\\u2B50\\u2B55]|[\\u3030\\u303D\\u3297\\u3299]\\uFE0F?|\\uD83C(?:[\\uDC04\\uDCCF]|[\\uDD70\\uDD71\\uDD7E\\uDD7F]\\uFE0F?|[\\uDD8E\\uDD91-\\uDD9A]|\\uDDE6\\uD83C[\\uDDE8-\\uDDEC\\uDDEE\\uDDF1\\uDDF2\\uDDF4\\uDDF6-\\uDDFA\\uDDFC\\uDDFD\\uDDFF]|\\uDDE7\\uD83C[\\uDDE6\\uDDE7\\uDDE9-\\uDDEF\\uDDF1-\\uDDF4\\uDDF6-\\uDDF9\\uDDFB\\uDDFC\\uDDFE\\uDDFF]|\\uDDE8\\uD83C[\\uDDE6\\uDDE8\\uDDE9\\uDDEB-\\uDDEE\\uDDF0-\\uDDF5\\uDDF7\\uDDFA-\\uDDFF]|\\uDDE9\\uD83C[\\uDDEA\\uDDEC\\uDDEF\\uDDF0\\uDDF2\\uDDF4\\uDDFF]|\\uDDEA\\uD83C[\\uDDE6\\uDDE8\\uDDEA\\uDDEC\\uDDED\\uDDF7-\\uDDFA]|\\uDDEB\\uD83C[\\uDDEE-\\uDDF0\\uDDF2\\uDDF4\\uDDF7]|\\uDDEC\\uD83C[\\uDDE6\\uDDE7\\uDDE9-\\uDDEE\\uDDF1-\\uDDF3\\uDDF5-\\uDDFA\\uDDFC\\uDDFE]|\\uDDED\\uD83C[\\uDDF0\\uDDF2\\uDDF3\\uDDF7\\uDDF9\\uDDFA]|\\uDDEE\\uD83C[\\uDDE8-\\uDDEA\\uDDF1-\\uDDF4\\uDDF6-\\uDDF9]|\\uDDEF\\uD83C[\\uDDEA\\uDDF2\\uDDF4\\uDDF5]|\\uDDF0\\uD83C[\\uDDEA\\uDDEC-\\uDDEE\\uDDF2\\uDDF3\\uDDF5\\uDDF7\\uDDFC\\uDDFE\\uDDFF]|\\uDDF1\\uD83C[\\uDDE6-\\uDDE8\\uDDEE\\uDDF0\\uDDF7-\\uDDFB\\uDDFE]|\\uDDF2\\uD83C[\\uDDE6\\uDDE8-\\uDDED\\uDDF0-\\uDDFF]|\\uDDF3\\uD83C[\\uDDE6\\uDDE8\\uDDEA-\\uDDEC\\uDDEE\\uDDF1\\uDDF4\\uDDF5\\uDDF7\\uDDFA\\uDDFF]|\\uDDF4\\uD83C\\uDDF2|\\uDDF5\\uD83C[\\uDDE6\\uDDEA-\\uDDED\\uDDF0-\\uDDF3\\uDDF7-\\uDDF9\\uDDFC\\uDDFE]|\\uDDF6\\uD83C\\uDDE6|\\uDDF7\\uD83C[\\uDDEA\\uDDF4\\uDDF8\\uDDFA\\uDDFC]|\\uDDF8\\uD83C[\\uDDE6-\\uDDEA\\uDDEC-\\uDDF4\\uDDF7-\\uDDF9\\uDDFB\\uDDFD-\\uDDFF]|\\uDDF9\\uD83C[\\uDDE6\\uDDE8\\uDDE9\\uDDEB-\\uDDED\\uDDEF-\\uDDF4\\uDDF7\\uDDF9\\uDDFB\\uDDFC\\uDDFF]|\\uDDFA\\uD83C[\\uDDE6\\uDDEC\\uDDF2\\uDDF3\\uDDF8\\uDDFE\\uDDFF]|\\uDDFB\\uD83C[\\uDDE6\\uDDE8\\uDDEA\\uDDEC\\uDDEE\\uDDF3\\uDDFA]|\\uDDFC\\uD83C[\\uDDEB\\uDDF8]|\\uDDFD\\uD83C\\uDDF0|\\uDDFE\\uD83C[\\uDDEA\\uDDF9]|\\uDDFF\\uD83C[\\uDDE6\\uDDF2\\uDDFC]|\\uDE01|\\uDE02\\uFE0F?|[\\uDE1A\\uDE2F\\uDE32-\\uDE36]|\\uDE37\\uFE0F?|[\\uDE38-\\uDE3A\\uDE50\\uDE51\\uDF00-\\uDF20]|[\\uDF21\\uDF24-\\uDF2C]\\uFE0F?|[\\uDF2D-\\uDF35]|\\uDF36\\uFE0F?|[\\uDF37-\\uDF7C]|\\uDF7D\\uFE0F?|[\\uDF7E-\\uDF84]|\\uDF85(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDF86-\\uDF93]|[\\uDF96\\uDF97\\uDF99-\\uDF9B\\uDF9E\\uDF9F]\\uFE0F?|[\\uDFA0-\\uDFC1]|\\uDFC2(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDFC3\\uDFC4](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDFC5\\uDFC6]|\\uDFC7(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDFC8\\uDFC9]|\\uDFCA(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDFCB\\uDFCC](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?|\\uFE0F(?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDFCD\\uDFCE]\\uFE0F?|[\\uDFCF-\\uDFD3]|[\\uDFD4-\\uDFDF]\\uFE0F?|[\\uDFE0-\\uDFF0]|\\uDFF3(?:\\u200D(?:\\u26A7\\uFE0F?|\\uD83C\\uDF08)|\\uFE0F(?:\\u200D(?:\\u26A7\\uFE0F?|\\uD83C\\uDF08))?)?|\\uDFF4(?:\\u200D\\u2620\\uFE0F?|\\uDB40\\uDC67\\uDB40\\uDC62\\uDB40(?:\\uDC65\\uDB40\\uDC6E\\uDB40\\uDC67|\\uDC73\\uDB40\\uDC63\\uDB40\\uDC74|\\uDC77\\uDB40\\uDC6C\\uDB40\\uDC73)\\uDB40\\uDC7F)?|[\\uDFF5\\uDFF7]\\uFE0F?|[\\uDFF8-\\uDFFF])|\\uD83D(?:[\\uDC00-\\uDC07]|\\uDC08(?:\\u200D\\u2B1B)?|[\\uDC09-\\uDC14]|\\uDC15(?:\\u200D\\uD83E\\uDDBA)?|[\\uDC16-\\uDC3A]|\\uDC3B(?:\\u200D\\u2744\\uFE0F?)?|[\\uDC3C-\\uDC3E]|\\uDC3F\\uFE0F?|\\uDC40|\\uDC41(?:\\u200D\\uD83D\\uDDE8\\uFE0F?|\\uFE0F(?:\\u200D\\uD83D\\uDDE8\\uFE0F?)?)?|[\\uDC42\\uDC43](?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDC44\\uDC45]|[\\uDC46-\\uDC50](?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDC51-\\uDC65]|[\\uDC66\\uDC67](?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDC68(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:\\uDC8B\\u200D\\uD83D)?\\uDC68|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D(?:\\uDC66(?:\\u200D\\uD83D\\uDC66)?|\\uDC67(?:\\u200D\\uD83D[\\uDC66\\uDC67])?|[\\uDC68\\uDC69]\\u200D\\uD83D(?:\\uDC66(?:\\u200D\\uD83D\\uDC66)?|\\uDC67(?:\\u200D\\uD83D[\\uDC66\\uDC67])?)|[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92])|\\uD83E[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])|\\uD83C(?:\\uDFFB(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:\\uDC8B\\u200D\\uD83D)?\\uDC68\\uD83C[\\uDFFB-\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D\\uDC68\\uD83C[\\uDFFC-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFC(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:\\uDC8B\\u200D\\uD83D)?\\uDC68\\uD83C[\\uDFFB-\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D\\uDC68\\uD83C[\\uDFFB\\uDFFD-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFD(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:\\uDC8B\\u200D\\uD83D)?\\uDC68\\uD83C[\\uDFFB-\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D\\uDC68\\uD83C[\\uDFFB\\uDFFC\\uDFFE\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFE(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:\\uDC8B\\u200D\\uD83D)?\\uDC68\\uD83C[\\uDFFB-\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D\\uDC68\\uD83C[\\uDFFB-\\uDFFD\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFF(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:\\uDC8B\\u200D\\uD83D)?\\uDC68\\uD83C[\\uDFFB-\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D\\uDC68\\uD83C[\\uDFFB-\\uDFFE]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?))?|\\uDC69(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:\\uDC8B\\u200D\\uD83D)?[\\uDC68\\uDC69]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D(?:\\uDC66(?:\\u200D\\uD83D\\uDC66)?|\\uDC67(?:\\u200D\\uD83D[\\uDC66\\uDC67])?|\\uDC69\\u200D\\uD83D(?:\\uDC66(?:\\u200D\\uD83D\\uDC66)?|\\uDC67(?:\\u200D\\uD83D[\\uDC66\\uDC67])?)|[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92])|\\uD83E[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])|\\uD83C(?:\\uDFFB(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF]|\\uDC8B\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF])|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFC-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFC(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF]|\\uDC8B\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF])|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB\\uDFFD-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFD(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF]|\\uDC8B\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF])|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB\\uDFFC\\uDFFE\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFE(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF]|\\uDC8B\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF])|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFD\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFF(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D\\uD83D(?:[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF]|\\uDC8B\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFF])|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83D[\\uDC68\\uDC69]\\uD83C[\\uDFFB-\\uDFFE]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?))?|\\uDC6A|[\\uDC6B-\\uDC6D](?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDC6E(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDC6F(?:\\u200D[\\u2640\\u2642]\\uFE0F?)?|[\\uDC70\\uDC71](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDC72(?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDC73(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDC74-\\uDC76](?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDC77(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDC78(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDC79-\\uDC7B]|\\uDC7C(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDC7D-\\uDC80]|[\\uDC81\\uDC82](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDC83(?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDC84|\\uDC85(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDC86\\uDC87](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDC88-\\uDC8E]|\\uDC8F(?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDC90|\\uDC91(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDC92-\\uDCA9]|\\uDCAA(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDCAB-\\uDCFC]|\\uDCFD\\uFE0F?|[\\uDCFF-\\uDD3D]|[\\uDD49\\uDD4A]\\uFE0F?|[\\uDD4B-\\uDD4E\\uDD50-\\uDD67]|[\\uDD6F\\uDD70\\uDD73]\\uFE0F?|\\uDD74(?:\\uD83C[\\uDFFB-\\uDFFF]|\\uFE0F)?|\\uDD75(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?|\\uFE0F(?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDD76-\\uDD79]\\uFE0F?|\\uDD7A(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDD87\\uDD8A-\\uDD8D]\\uFE0F?|\\uDD90(?:\\uD83C[\\uDFFB-\\uDFFF]|\\uFE0F)?|[\\uDD95\\uDD96](?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDDA4|[\\uDDA5\\uDDA8\\uDDB1\\uDDB2\\uDDBC\\uDDC2-\\uDDC4\\uDDD1-\\uDDD3\\uDDDC-\\uDDDE\\uDDE1\\uDDE3\\uDDE8\\uDDEF\\uDDF3\\uDDFA]\\uFE0F?|[\\uDDFB-\\uDE2D]|\\uDE2E(?:\\u200D\\uD83D\\uDCA8)?|[\\uDE2F-\\uDE34]|\\uDE35(?:\\u200D\\uD83D\\uDCAB)?|\\uDE36(?:\\u200D\\uD83C\\uDF2B\\uFE0F?)?|[\\uDE37-\\uDE44]|[\\uDE45-\\uDE47](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDE48-\\uDE4A]|\\uDE4B(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDE4C(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDE4D\\uDE4E](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDE4F(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDE80-\\uDEA2]|\\uDEA3(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDEA4-\\uDEB3]|[\\uDEB4-\\uDEB6](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDEB7-\\uDEBF]|\\uDEC0(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDEC1-\\uDEC5]|\\uDECB\\uFE0F?|\\uDECC(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDECD-\\uDECF]\\uFE0F?|[\\uDED0-\\uDED2\\uDED5-\\uDED7\\uDEDD-\\uDEDF]|[\\uDEE0-\\uDEE5\\uDEE9]\\uFE0F?|[\\uDEEB\\uDEEC]|[\\uDEF0\\uDEF3]\\uFE0F?|[\\uDEF4-\\uDEFC\\uDFE0-\\uDFEB\\uDFF0])|\\uD83E(?:\\uDD0C(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDD0D\\uDD0E]|\\uDD0F(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDD10-\\uDD17]|[\\uDD18-\\uDD1F](?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDD20-\\uDD25]|\\uDD26(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDD27-\\uDD2F]|[\\uDD30-\\uDD34](?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDD35(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDD36(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDD37-\\uDD39](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDD3A|\\uDD3C(?:\\u200D[\\u2640\\u2642]\\uFE0F?)?|[\\uDD3D\\uDD3E](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDD3F-\\uDD45\\uDD47-\\uDD76]|\\uDD77(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDD78-\\uDDB4]|[\\uDDB5\\uDDB6](?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDDB7|[\\uDDB8\\uDDB9](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDDBA|\\uDDBB(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDDBC-\\uDDCC]|[\\uDDCD-\\uDDCF](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDDD0|\\uDDD1(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF84\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83E\\uDDD1|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD]))|\\uD83C(?:\\uDFFB(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D(?:\\uD83D\\uDC8B\\u200D)?\\uD83E\\uDDD1\\uD83C[\\uDFFC-\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF84\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83E\\uDDD1\\uD83C[\\uDFFB-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFC(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D(?:\\uD83D\\uDC8B\\u200D)?\\uD83E\\uDDD1\\uD83C[\\uDFFB\\uDFFD-\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF84\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83E\\uDDD1\\uD83C[\\uDFFB-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFD(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D(?:\\uD83D\\uDC8B\\u200D)?\\uD83E\\uDDD1\\uD83C[\\uDFFB\\uDFFC\\uDFFE\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF84\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83E\\uDDD1\\uD83C[\\uDFFB-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFE(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D(?:\\uD83D\\uDC8B\\u200D)?\\uD83E\\uDDD1\\uD83C[\\uDFFB-\\uDFFD\\uDFFF]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF84\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83E\\uDDD1\\uD83C[\\uDFFB-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?|\\uDFFF(?:\\u200D(?:[\\u2695\\u2696\\u2708]\\uFE0F?|\\u2764\\uFE0F?\\u200D(?:\\uD83D\\uDC8B\\u200D)?\\uD83E\\uDDD1\\uD83C[\\uDFFB-\\uDFFE]|\\uD83C[\\uDF3E\\uDF73\\uDF7C\\uDF84\\uDF93\\uDFA4\\uDFA8\\uDFEB\\uDFED]|\\uD83D[\\uDCBB\\uDCBC\\uDD27\\uDD2C\\uDE80\\uDE92]|\\uD83E(?:\\uDD1D\\u200D\\uD83E\\uDDD1\\uD83C[\\uDFFB-\\uDFFF]|[\\uDDAF-\\uDDB3\\uDDBC\\uDDBD])))?))?|[\\uDDD2\\uDDD3](?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDDD4(?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|\\uDDD5(?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDDD6-\\uDDDD](?:\\u200D[\\u2640\\u2642]\\uFE0F?|\\uD83C[\\uDFFB-\\uDFFF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?)?|[\\uDDDE\\uDDDF](?:\\u200D[\\u2640\\u2642]\\uFE0F?)?|[\\uDDE0-\\uDDFF\\uDE70-\\uDE74\\uDE78-\\uDE7C\\uDE80-\\uDE86\\uDE90-\\uDEAC\\uDEB0-\\uDEBA\\uDEC0-\\uDEC2]|[\\uDEC3-\\uDEC5](?:\\uD83C[\\uDFFB-\\uDFFF])?|[\\uDED0-\\uDED9\\uDEE0-\\uDEE7]|\\uDEF0(?:\\uD83C[\\uDFFB-\\uDFFF])?|\\uDEF1(?:\\uD83C(?:\\uDFFB(?:\\u200D\\uD83E\\uDEF2\\uD83C[\\uDFFC-\\uDFFF])?|\\uDFFC(?:\\u200D\\uD83E\\uDEF2\\uD83C[\\uDFFB\\uDFFD-\\uDFFF])?|\\uDFFD(?:\\u200D\\uD83E\\uDEF2\\uD83C[\\uDFFB\\uDFFC\\uDFFE\\uDFFF])?|\\uDFFE(?:\\u200D\\uD83E\\uDEF2\\uD83C[\\uDFFB-\\uDFFD\\uDFFF])?|\\uDFFF(?:\\u200D\\uD83E\\uDEF2\\uD83C[\\uDFFB-\\uDFFE])?))?|[\\uDEF2-\\uDEF6](?:\\uD83C[\\uDFFB-\\uDFFF])?)\"gm";
            Regex regex = new(EmojiPattern);
            status = regex.IsMatch(text_value);
            return status;
        }
        #endregion
        #region Otp
        public static string GenerateOTP()
        {
            return (new Random()).Next(100000, 999999).ToString();
        }
        public static DateTime GetOtpExpiryDateTime(int minutes)
        {
            return DateTime.UtcNow.AddMinutes(minutes);
        }
        #endregion
        #region Xml
        public static string XMLStringToJson(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            return jsonText;
        }
        public static string XmlSerialize<T>(T dataObject)
        {
            if (dataObject == null)
            {
                return string.Empty;
            }
            try
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stringWriter, dataObject);
                    return stringWriter.ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static T? XmlDeserialize<T>(string xml)
             where T : new()
        {
            if (string.IsNullOrEmpty(xml))
            {
                return new T();
            }
            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T?)serializer.Deserialize(stringReader);
                }
            }
            catch (Exception)
            {
                return new T();
            }
        }
        #endregion
        #region Random
        static string GenerateRandomDigits(int length)
        {
            StringBuilder randomDigits = new();
            for (int i = 0; i < length; i++)
            {
                randomDigits.Append(RandomNumberGenerator.GetInt32(0, 10));
            }
            return randomDigits.ToString();
        }
        #endregion
        #region Others
        public static string ConvertDataTableToHTML(DataTable dt, bool is_header_required = false, int table_width_percentage = 100, int font_size = 12, string font_family = "helvetica")
        {
            string html = "<table style='border:1px solid #b3adad;border-collapse:collapse;padding:5px;width:" + table_width_percentage + "%; font-family:" + font_family + ";font-size:" + font_size + "px'>";
            if (is_header_required == true)
            {
                html += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                    html += "<td style='border:1px solid #b3adad;text-align:center;padding:5px;background: #ffffff;color: #313030;'>" + dt.Columns[i].ColumnName + "</td>";
                html += "</tr>";
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td style='border:1px solid #b3adad;text-align:center;padding:5px;background: #ffffff;color: #313030;'>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        public static string ConvertDataTableToHTMLWithClass(DataTable dt, bool is_header_required = false, string table_class = "")
        {
            string html = "<table class='" + table_class + "'>";
            if (is_header_required == true)
            {
                html += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                    html += "<td'>" + dt.Columns[i].ColumnName + "</td>";
                html += "</tr>";
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new(typeof(T).Name);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    if (Props[i] != null && item != null)
                    {
                        var propertyValue = Props[i].GetValue(item, null);
                        values[i] = propertyValue != null ? propertyValue : new object();
                    }
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
        public static DataTable ToDataTable(List<Dictionary<string, string>> list)
        {
            DataTable result = new();
            if (list.Count == 0)
                return result;

            var columnNames = list.SelectMany(dict => dict.Keys).Distinct();
            result.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
            foreach (Dictionary<string, string> item in list)
            {
                var row = result.NewRow();
                foreach (var key in item.Keys)
                {
                    row[key] = item[key];
                }
                result.Rows.Add(row);
            }
            return result;
        }
        public static object? GetPropertyValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName)?.GetValue(src, null);
        }
        public static string GenerateParamsFromObject(dynamic entity, Type type)
        {
            string result = string.Empty;
            Dictionary<string, string?> result_dictionary = new();
            Dictionary<string, object> keyValuePairs_entity = ToDictionary(entity);
            var transaction_properties = type.GetProperties();
            foreach (var item in keyValuePairs_entity)
            {
                if (!transaction_properties.Any(x => x.Name.ToLower() == item.Key.Replace("_", string.Empty).ToLower()))
                {
                    result_dictionary.Add(item.Key, Convert.ToString(item.Value) ?? string.Empty);
                }
            }
            result = JsonConvert.SerializeObject(result_dictionary);
            return result;
        }
        public static string GenerateParamsFromDictionary(Dictionary<string, string>? payment_params)
        {
            string result = string.Empty;
            Dictionary<string, string?> result_dictionary = new();
            if (payment_params != null)
            {
                foreach (var item in payment_params)
                {
                    result_dictionary.Add(item.Key, Convert.ToString(item.Value) ?? string.Empty);
                }
            }
            result = JsonConvert.SerializeObject(result_dictionary);
            return result;
        }
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }
        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
                ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, T>();
            if (source != null)
            {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
                    AddPropertyToDictionary<T>(property, source, dictionary);
            }
            return dictionary;
        }
        private static void AddPropertyToDictionary<T>(PropertyDescriptor? property, object? source, Dictionary<string, T> dictionary)
        {
            object? value = property?.GetValue(source);
            if (value != null && property != null)
            {
                if (IsOfType<T>(value))
                    dictionary.Add(property.Name, (T)value);
            }
        }
        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }
        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
        }
        #endregion
        private static string GetCallerFilePath([CallerFilePath] string file = "") => file;
        private static int GetCallerLineNumber([CallerLineNumber] int line = 0) => line;

    }
}
