using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    [TestClass]
    public partial class PayeezyGatewayTests {
        protected const string cardHolderName = "Bubba Smith";
        protected const string validVisa = "4111111111111111";
        protected const string validDiners = "36438936438936";
        protected const string validAmex = "373953192351004";
        protected const string invalidCard = "1234567812345678";
        protected const string invalidVisa = "4111111111111112";
        protected const string month = "01";
        protected const string twoDigitYear = "20";
        protected const string dollarAmount1 = "1.23";
        protected const string dollarAmount2 = "3.21";
        protected const string dollarAmountDeclined = "5000.42";
        protected const string cvv = "123";
        protected const string validToken = "7143582050568291";

        private PayeezyGateway GetReference() {

            return new PayeezyGateway(
                apiKey: "gfWgVODJ0yWYm58IWgQz61rgmdI4AOuG",
                apiSecret: "725964375283bbcce83fba4a98ebf0ebf5abeb0a9630f9d9a643e1c967f10765",
                token: "fdoa-c1f2081520431ab54aea1651c1cbe9efc1f2081520431ab5"
            );

            /*
             * return new PayeezyGateway(
                apiKey: "y6pWAJNyJyjGv66IsVuWnklkKUPFbb0a",
                apiSecret: "86fbae7030253af3cd15faef2a1f4b67353e41fb6799f576b5093ae52901e6f7",
                token: "fdoa-a480ce8951daa73262734cf102641994c1e55e7cdf4c02b6"
            );*/
        }

        private string GetReferenceNumber() {
            return Guid.NewGuid().ToString();
        }
    }
}
 