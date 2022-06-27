using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Serialization;

namespace Devcoons.Misc
{
    public static class Extensions
    {
        public static string NullToEmpty(this string arg)
        {
            return string.IsNullOrEmpty(arg) ? "" : arg;
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghigklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool IsNullOrEmptyOrShort(this string arg, int minLenght)
        {
            if (string.IsNullOrEmpty(arg))
                return true;
            if (arg.Length < minLenght)
                return true;
            return false;
        }

        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

    
        public static string StringReverse(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
        {
            var sortableList = new List<T>(collection);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortableList[i]), i);
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            var col = new ObservableCollection<T>();
            foreach (var cur in enumerable)
            {
                col.Add(cur);
            }
            return col;
        }

        public static string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static bool RetryIfFailed(Func<bool> lamda, int times, int delay)
        {
            while (lamda() == false)
            {
                Thread.Sleep(delay);
                if (times == 0)
                    return false;
                times--;
            }
            return true;
        }

        public static bool RetryIfFailed(Func<bool> lamda, int times, int delay, CancellationToken token)
        {
            while (lamda() == false)
            {
                Thread.Sleep(delay);
                if (times == 0)
                    return false;
                times--;
                if (token.IsCancellationRequested == true)
                    return false;
            }
            return true;
        }

        public static string SubstringExt(this string arg, int start, int length)
        {
            try
            {
                int full_length = arg.Length;

                if (full_length - (start + length) > 0)
                    return arg.Substring(start, length);

                return arg.Substring(start, length + (full_length - (start + length)));
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ExceptionOccured = "";

        public static T SafeExecutor<T>(Func<T> action)
        {
            ExceptionOccured = "";

            try
            {
                return action();
            }
            catch (Exception ex)
            {
                ExceptionOccured = ex.Message.ToString();
            }

            return default(T);
        }

        public static string SafeExecutorResults()
        {
            return ExceptionOccured;
        }


        public static Stream ToStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static long? GetMinFromDatatype(string datatype, string signess)
        {
            if (signess.ToLower().Contains("u"))
            {
                return 0;
            }

            if (datatype.ToLower() == "byte" || datatype.ToLower() == "bool")
            {
                return -127;
            }

            if (datatype.ToLower() == "word")
            {
                return -32768;
            }

            if (datatype.ToLower() == "long")
            {
                return -2147483648;
            }
            return null;
        }

        public static long? GetMaxFromDatatype(string datatype, string signess)
        {
            if (datatype.ToLower() == "byte" || datatype.ToLower() == "bool")
            {
                if (signess.ToLower().Contains("u"))
                {
                    return 255;
                }
                return 127;
            }

            if (datatype.ToLower() == "word")
            {
                if (signess.ToLower().Contains("u"))
                {
                    return 65535;
                }
                return 32767;
            }

            if (datatype.ToLower() == "long")
            {
                if (signess.ToLower().Contains("u"))
                {
                    return 4294967295;
                }
                return 2147483647;
            }
            return null;
        }

        public static long DateToLong(DateTime arg, bool rebaseTo2000)
        {
            DateTime d2000 = new DateTime(2000, 1, 1).ToUniversalTime();
            DateTime utcnow = arg.ToUniversalTime();
            return rebaseTo2000 == false
                ? ((DateTimeOffset)utcnow).ToUnixTimeSeconds()
                : ((DateTimeOffset)utcnow).ToUnixTimeSeconds() - ((DateTimeOffset)d2000).ToUnixTimeSeconds();
        }

        public static DateTime LongToDate(long arg)
        {
            DateTime d2000 = new DateTime(2000, 1, 1).ToUniversalTime();

            return (arg < ((DateTimeOffset)d2000).ToUnixTimeSeconds())
                ? d2000.AddSeconds(arg).ToLocalTime()
                : (DateTimeOffset.FromUnixTimeSeconds(arg)).DateTime.ToLocalTime();
        }

        public static long? ConvertToActualMaxParameterValue(string datatype, string signess, Int64 value)
        {
            if (signess.ToLower().Contains("u"))
                return value;


            string binary = Convert.ToString(value, 2).PadLeft(64, '0');


            if (datatype.ToLower() == "byte" || datatype.ToLower() == "bool")
            {
                return (long)System.Convert.ToSByte(binary.Substring(binary.Length - 8), 2);

            }

            if (datatype.ToLower() == "word")
            {
                return (long)System.Convert.ToUInt16(binary.Substring(binary.Length - 16), 2);
            }

            if (datatype.ToLower() == "long")
            {
                return (long)System.Convert.ToUInt32(binary.Substring(binary.Length - 32), 2);
            }

            return null;
        }


        public static byte[] HexToByteArray(this string hex)
        {
            if (hex.Length % 2 == 1)
                return null;

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            return (int)hex - ((int)hex < 58 ? 48 : ((int)hex < 97 ? 55 : 87));
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] CombineWith(this byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static T[] SubArrayDeepClone<T>(this T[] data, int index, int length)
        {
            try
            {
                T[] arrCopy = new T[(data.Length - index) > length ? length : data.Length - index];
                Array.Copy(data, index, arrCopy, 0, (data.Length - index) > length ? length : data.Length - index);
                using (MemoryStream ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, arrCopy);
                    ms.Position = 0;
                    return (T[])bf.Deserialize(ms);
                }
            }
            catch (Exception)
            {
                return default(T[]);
            }
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            try
            {
                T[] arrCopy = new T[(data.Length - index) > length ? length : data.Length - index];
                Array.Copy(data, index, arrCopy, 0, (data.Length - index) > length ? length : data.Length - index);
                return arrCopy;
            }
            catch (Exception)
            {
                return default(T[]);
            }
        }
        public static string GetAsString<T>(this T arg)
        {
            if (arg == null)
                return "";
            return (string)arg.ToString();
        }
        public static bool HasMethod(this object obj, string methodName)
        {
            var type = obj.GetType();
            return type.GetMethod(methodName) != null;
        }

  
        public static int SumUpBytes(this byte[] t)
        {
            int res = 0;

            for (int i = 0; i < t.Length; i++)
                res += t[i];

            return res;
        }

        public static T FindBasedOn<T>(this List<T> list, string propertyName, object value)
        {
            if (list.Count == 0)
                return default(T);

            if (list[0].HasProperty(propertyName) == false)
                return default(T);

            for (int i = 0; i < list.Count; i++)
                if (list[i].GetType().GetProperty(propertyName).GetValue(list[i]).Equals(value) == true)
                    return list[i];

            return default(T);
        }

 
        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static IEnumerable<byte[]> Split(this byte[] value, int bufferLength)
        {
            int countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
                countOfArray++;
            for (int i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();

            }
        }

        public static string ConvertToStringFromType(this string type, byte[] array, bool isReversed)
        {
            type = type.ToLowerInvariant().Trim();

            try
            {

                if (type == "uint8_t" || type == "u8")
                    return Convert.ToSByte(array[0]).ToString();

                if (type == "int8_t" || type == "i8")
                    return Convert.ToByte(array[0]).ToString();

                if (type == "uint16_t" || type == "u16")
                    return isReversed == false ? BitConverter.ToUInt16(array, 0).ToString() : BitConverter.ToUInt16(array.SubArray(0, 2).Reverse().ToArray(), 0).ToString();

                if (type == "int16_t" || type == "i16")
                    return isReversed == false ? BitConverter.ToInt16(array, 0).ToString() : BitConverter.ToInt16(array.SubArray(0, 2).Reverse().ToArray(), 0).ToString();

                if (type == "uint32_t" || type == "u32")
                    return isReversed == false ? BitConverter.ToUInt32(array, 0).ToString() : BitConverter.ToUInt32(array.SubArray(0, 4).Reverse().ToArray(), 0).ToString();

                if (type == "int32_t" || type == "i32")
                    return isReversed == false ? BitConverter.ToInt32(array, 0).ToString() : BitConverter.ToInt32(array.SubArray(0, 4).Reverse().ToArray(), 0).ToString();

                if (type == "uint64_t" || type == "u64")
                    return isReversed == false ? BitConverter.ToUInt64(array, 0).ToString() : BitConverter.ToUInt64(array.SubArray(0, 8).Reverse().ToArray(), 0).ToString();

                if (type == "int64_t" || type == "i64")
                    return isReversed == false ? BitConverter.ToInt64(array, 0).ToString() : BitConverter.ToInt64(array.SubArray(0, 8).Reverse().ToArray(), 0).ToString();

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int GetSizeOfType(this string type)
        {
            if (type.Contains("int8_t"))
                return 1;

            if (type.Contains("int16_t"))
                return 2;

            if (type.Contains("int32_t"))
                return 4;

            if (type.Contains("int64_t"))
                return 8;

            return 0;
        }

        public static byte[] ToBytesArray(this Int32 arg)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((arg & 0xFF000000) >> 24);
            result[1] = (byte)((arg & 0x00FF0000) >> 16);
            result[2] = (byte)((arg & 0x0000FF00) >> 8);
            result[3] = (byte)((arg & 0x000000FF) >> 0);
            return result;
        }

        public static byte[] ToBytesArray(this UInt32 arg)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((arg & 0xFF000000) >> 24);
            result[1] = (byte)((arg & 0x00FF0000) >> 16);
            result[2] = (byte)((arg & 0x0000FF00) >> 8);
            result[3] = (byte)((arg & 0x000000FF) >> 0);
            return result;
        }

        public static byte[] ToBytesArray(this UInt16 arg)
        {
            byte[] result = new byte[2];
            result[0] = (byte)((arg & 0x0000FF00) >> 8);
            result[1] = (byte)((arg & 0x000000FF) >> 0);
            return result;
        }

        public static byte[] ToBytesArray(this Int16 arg)
        {
            byte[] result = new byte[2];
            result[0] = (byte)((arg & 0x0000FF00) >> 8);
            result[1] = (byte)((arg & 0x000000FF) >> 0);
            return result;
        }

        public static string EncodeUrl(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return null;
            try
            {
                return HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(CompressString(arg))));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DecodeUrl(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return null;
            try
            {
                return DecompressString(Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.UrlDecode(arg))));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string EncodeUrlNoCompression(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return null;
            try
            {
                return HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(arg)));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DecodeUrlNoCompression(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return null;
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.UrlDecode(arg)));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Encode(byte[] arg)
        {
            try
            {
                return System.Convert.ToBase64String(arg).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }



        public static byte[] Decode(string arg)
        {
            arg = arg.Replace('_', '/').Replace('-', '+');
            switch (arg.Length % 4)
            {
                case 2:
                    arg += "==";
                    break;
                case 3:
                    arg += "=";
                    break;
            }

            return Convert.FromBase64String(arg);
        }



        public static object GetClone(this object cloneThis)
        {
            object clone = null;
            Type t = cloneThis.GetType();
            try
            {
                XmlSerializer ser = XmlSerializer.FromTypes(new[] { cloneThis.GetType() })[0];
                MemoryStream ms = new MemoryStream();
                ser.Serialize(ms, cloneThis);
                ms.Flush();
                ms.Position = 0;
                clone = ser.Deserialize(ms);
            }
            catch (Exception)
            {
            }
            return clone;
        }


        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description.ToUpper() == description.ToUpper())
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name.ToUpper() == description.ToUpper())
                        return (T)field.GetValue(null);
                }
            }
            return default(T);
        }

        public static string GetDescriptionFromValue<T>(T description)
        {
            try
            {
                System.Reflection.MemberInfo[] memInfo = description.GetType().GetMember(description.ToString());
                DescriptionAttribute attribute = CustomAttributeExtensions.GetCustomAttribute<DescriptionAttribute>(memInfo[0]);
                return attribute.Description;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void SaveAsFile(this byte[] resource, string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
                File.WriteAllBytes(file, resource);
            }
            catch (Exception)
            {
            }
        }

    }
}
