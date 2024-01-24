using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XLAutoDeploy.FileSystem
{
    internal static class Comparison
    {
        public static bool BytesEqual(string filePath1, string filePath2)
        {
            if (filePath1.Equals(filePath2, System.StringComparison.OrdinalIgnoreCase))
                return true;

            using (var fs1 = File.OpenRead(filePath1))
            {
                using (var fs2 = File.OpenRead(filePath2))
                {
                    if (fs1.Length != fs2.Length)
                        return false;

                    return BytesEqual(fs1, fs2);
                }
            }
        }

        public static bool BytesEqual(string filePath, Stream stream)
        {
            return BytesEqual(stream, filePath);
        }

        public static bool BytesEqual(Stream stream, string filePath)
        {
            int b1, b2;
            using (var fs = File.OpenRead(filePath))
            {
                if (stream is null || stream.Length != fs.Length)
                    return false;

                do
                {
                    b1 = stream.ReadByte();
                    b2 = fs.ReadByte();
                }
                while ((b1 == b2) && (b1 != -1));
            }

            return ((b1 - b2) == 0);
        }

        public static bool BytesEqual(Stream stream1, Stream stream2)
        {
            if (stream1 is null || stream2 is null || stream1.Length != stream2.Length)
                return false;

            int b1, b2;
            do
            {
                b1 = stream1.ReadByte();
                b2 = stream2.ReadByte();
            }
            while ((b1 == b2) && (b1 != -1));

            return ((b1 - b2) == 0);
        }

        public static bool ValidateHash(HashAlgorithm algorithm, string filePath, string expectedHash)
        {
            using (var fs = File.OpenRead(filePath))
            {
                return ValidateHash(algorithm, fs, expectedHash);
            }
        }

        public static bool ValidateHash(HashAlgorithm algorithm, Stream stream, string expectedHash)
        {
            return ValidateHash(algorithm, stream, Encoding.Unicode.GetBytes(expectedHash));
        }

        public static bool ValidateHash(HashAlgorithm algorithm, Stream stream, byte[] expectedHash)
        {
            var actual = algorithm.ComputeHash(stream);

            return ByteArraysEqualUnsafe(actual, expectedHash);
        }

        public static bool HashEqual(HashAlgorithm algorithm, string filePath1, string filePath2)
        {
            if (filePath1.Equals(filePath2, System.StringComparison.OrdinalIgnoreCase))
                return true;

            using (var fs1 = File.OpenRead(filePath1))
            {
                using (var fs2 = File.OpenRead(filePath2))
                {
                    if (fs1.Length != fs2.Length)
                        return false;

                    var h1 = algorithm.ComputeHash(fs1);
                    var h2 = algorithm.ComputeHash(fs2);

                    return ByteArraysEqualUnsafe(h1, h2);
                }
            }
        }

        public static bool HashEqual(HashAlgorithm algorithm, string filePath, Stream stream)
        {
            return HashEqual(algorithm, stream, filePath);
        }

        public static bool HashEqual(HashAlgorithm algorithm, Stream stream, string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                if (stream is null || stream.Length != fs.Length)
                    return false;

                var h1 = algorithm.ComputeHash(stream);
                var h2 = algorithm.ComputeHash(fs);

                return ByteArraysEqualUnsafe(h1, h2);
            }
        }

        public static bool HashEqual(HashAlgorithm algorithm, Stream stream1, Stream stream2)
        {
            if (stream1 is null || stream2 is null || stream1.Length != stream2.Length)
                return false;

            var h1 = algorithm.ComputeHash(stream1);
            var h2 = algorithm.ComputeHash(stream2);

            return ByteArraysEqualUnsafe(h1, h2);
        }

        //See https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net/8808245#8808245
        internal static unsafe bool ByteArraysEqualUnsafe(byte[] b1, byte[] b2)
        {
            if (b1 == b2)
                return true;

            if (b1 is null || b2 is null || b1.Length != b2.Length)
                return false;

            fixed (byte* p1 = b1, p2 = b2)
            {
                byte* x1 = p1, x2 = p2;
                int l = b1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                {
                    if (*((long*)x1) != *((long*)x2))
                        return false;
                }

                if ((l & 4) != 0)
                {
                    if (*((int*)x1) != *((int*)x2))
                        return false; x1 += 4; x2 += 4;
                }

                if ((l & 2) != 0)
                {
                    if (*((short*)x1) != *((short*)x2))
                        return false; x1 += 2; x2 += 2;
                }

                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2))
                        return false;

                return true;
            }
        }

        /*
        public static int CompareFileVersions(string filePath1, string filePath2)
        {
            var versionInfo1 = FileVersionInfo.GetVersionInfo(filePath1);
            var versionInfo2 = FileVersionInfo.GetVersionInfo(filePath2);

            if (String.IsNullOrEmpty(versionInfo1.FileVersion) || String.IsNullOrEmpty(versionInfo2.FileVersion))
                return 0;

            var version1 = new Version(versionInfo1.FileVersion);
            var version2 = new Version(versionInfo2.FileVersion);

            return version1.CompareTo(version2);  
        }

        public static bool IsFileLocked(FileInfo fileInfo)
        {
            try
            {
                using (FileStream stream = fileInfo.Open(FileMode.Open, System.IO.FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
        */
    }
}
