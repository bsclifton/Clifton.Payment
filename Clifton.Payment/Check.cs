using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Payment {
    public class Check {
        public string AccountNumber = string.Empty;
        public string RoutingNumber = string.Empty;
        public string CheckNumber = string.Empty;

        private bool IsValidMicrCharacter(char c) {
            if ((c >= '0') && (c <= '9')) return true;
            if ((c == 'T') || (c == 't')) return true;
            if ((c == 'O') || (c == 'o')) return true;

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
    }
}