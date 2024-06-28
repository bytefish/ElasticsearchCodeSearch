// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Web.Client.Infrastructure
{
    public class DataSizeUtils
    {
        /// <summary>
        /// The size of a byte, in bytes. Always 1, provided for consistency only.
        /// </summary>
        public const long ByteSize = 1L;

        /// <summary>
        /// The size of a kilobyte, in bytes. This structure defines a KB as 1,024 bytes.
        /// </summary>
        public const long KilobyteSize = 1024L;

        /// <summary>
        /// The size of a megabyte, in bytes. This structure defines a MB as 1,024^2 or 1,048,576 bytes.
        /// </summary>
        public const long MegabyteSize = 1024L * 1024L;

        /// <summary>
        /// The size of a gigabyte, in bytes. This structure defines a GB as 1,024^3 or 1,073,741,824 bytes.
        /// </summary>
        public const long GigabyteSize = 1024L * 1024L * 1024L;

        /// <summary>
        /// The size of a terabyte, in bytes. This structure defines a TB as 1,024^4 or 1,099,511,627,776 bytes.
        /// </summary>
        public const long TerabyteSize = 1024L * 1024L * 1024L * 1024L;

        /// <summary>
        /// Gets the value in terabytes.
        /// </summary>
        public static decimal TotalTerabytes(long bytes)
        {
            return bytes / (decimal)TerabyteSize;
        }

        /// <summary>
        /// Gets the value in gigabytes.
        /// </summary>
        public static decimal TotalGigabytes(long bytes)
        {
            return bytes / (decimal)GigabyteSize;
        }

        /// <summary>
        /// Gets the value in megabytes.
        /// </summary>
        public static decimal TotalMegabytes(long bytes)
        {
            return bytes / (decimal)MegabyteSize;
        }

        /// <summary>
        /// Gets the value in kilobytes.
        /// </summary>
        public static decimal TotalKilobytes(long bytes)
        {
            return bytes / (decimal)KilobyteSize;
        }

        /// <summary>
        /// Gets the value in terabytes.
        /// </summary>
        public static string TotalTerabytesString(long bytes)
        {
            var terabyte = TotalTerabytes(bytes);

            return $"{terabyte:F2} TB";

        }

        /// <summary>
        /// Gets the value in gigabytes.
        /// </summary>
        public static string TotalGigabytesString(long bytes)
        {
            var gigabytes = TotalGigabytes(bytes);

            return $"{gigabytes:F2} GB";
        }

        /// <summary>
        /// Gets the value in megabytes.
        /// </summary>
        public static string TotalMegabytesString(long bytes)
        {
            var megabytes = TotalMegabytes(bytes);

            return $"{megabytes:F2} MB";
        }

        /// <summary>
        /// Gets the value in kilobytes.
        /// </summary>
        public static string TotalKilobytesString(long bytes)
        {
            var kilobytes = TotalKilobytes(bytes);

            return $"{kilobytes:F2} KB";
        }
    }
}
