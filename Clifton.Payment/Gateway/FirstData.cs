using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clifton.Payment.Payeezy;

namespace Clifton.Payment.Gateway {
    public class FirstData : BaseGateway {
        public override void ProcessCreditCard(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName) {
            int parsedExpirationMonth, parsedExpirationYear;

            base.ValidateCreditCard(cardNumber, expirationMonth, expirationYear, out parsedExpirationMonth, out parsedExpirationYear);

            ServiceSoapClient ws = new ServiceSoapClient();
            Transaction txn = new Transaction();

            var config = Clifton.Payment.Configuration.GetConfig();
            txn.ExactID = config.Gateways["FirstData"].Id;
            txn.Password = config.Gateways["FirstData"].Password;
            txn.Transaction_Type = "00";
            txn.Card_Number = cardNumber;
            txn.CardHoldersName = cardHoldersName;
            txn.DollarAmount = dollarAmount;
            txn.Expiry_Date = string.Format("{0,2}{1,2}", parsedExpirationMonth, parsedExpirationYear);

            TransactionResult result = ws.SendAndCommit(txn);

            Console.WriteLine(result.CTR);
            Console.WriteLine("e4 resp code: " + result.EXact_Resp_Code);
            Console.WriteLine("e4 message: " + result.EXact_Message);
            Console.WriteLine("bank resp code: " + result.Bank_Resp_Code);
            Console.WriteLine("bank message: " + result.Bank_Message);
        }
    }
}