using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Payment {
    public sealed class Check {
        private const char FieldSeparator = '<';

        public string AccountNumber = string.Empty;
        public string RoutingNumber = string.Empty;
        public string CheckNumber = string.Empty;

        private static bool IsValidMicrCharacter(char c) {
            if ((c >= '0') && (c <= '9')) return true;
            if ((c == 'T') || (c == 't')) return true;
            if ((c == 'O') || (c == 'o')) return true;

            if (c == '?') throw new Exception("? is an invalid character");

            return false;
        }

        public static bool IsValidRoutingNumber(string routingNumber) {
            int i, checksum;

            for (i = 0, checksum = 0; i < routingNumber.Length; i += 3) {
                checksum += ((routingNumber[i] - '0') * 3);
                checksum += ((routingNumber[i + 1] - '0') * 7);
                checksum += ((routingNumber[i + 2] - '0'));
            }

            //checksum must be a non-zero multiple of 10
            if (checksum != 0 && (checksum % 10 == 0)) {
                return true;
            }

            return false;
        }

        private static string ParseMicrUntil(byte[] data, ref int offset, List<char> untilTheseChars) {
            string parsedData = string.Empty;
            char thisChar;

            while (offset < data.Length) {
                if (untilTheseChars.Contains((char)data[offset])) {
                    break;
                }

                if (IsValidMicrCharacter(thisChar = (char)data[offset++])) {
                    parsedData += thisChar;
                }
            }

            return parsedData;
        }

        /// <summary>
        /// Separators depend on the check reader. These were ones I dealt with.
        /// </summary>
        /// <see cref="http://www.whatismicr.com/MICR_education_center.html"/>
        public static int ParseFromMicr(byte[] micrData, int offset, out Check parsedCheck) {
            string routingNumber = string.Empty, accountNumber = string.Empty, checkNumber = string.Empty;

            while (offset < micrData.Length) {
                if (!IsValidMicrCharacter((char)micrData[offset])) {
                    if (++offset >= micrData.Length) {
                        break;
                    }
                    continue;
                }

                switch ((char)micrData[offset]) {
                    case 'T':
                    case 't':
                        ++offset;
                        routingNumber = ParseMicrUntil(micrData, ref offset, new List<char>() { 'T', 't' });
                        offset++;
                        break;

                    case 'O':
                    case 'o':
                        ++offset;
                        checkNumber = ParseMicrUntil(micrData, ref offset, new List<char>() { 'O', 'o', FieldSeparator });
                        break;

                    default:
                        accountNumber = ParseMicrUntil(micrData, ref offset, new List<char>() { 'O', 'o', FieldSeparator });
                        break;
                }
            }

            parsedCheck = new Check {
                RoutingNumber = routingNumber,
                AccountNumber = accountNumber,
                CheckNumber = checkNumber
            };

            return offset;
        }
    }
}