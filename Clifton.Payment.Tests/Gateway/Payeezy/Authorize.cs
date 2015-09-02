using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    public partial class PayeezyGatewayTests {
        [TestMethod]
        public void PayeezyCC_Authorize_HappyPath() {
            var response = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());

            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Authorize, response.ParsedTransactionType);
        }
    }
}