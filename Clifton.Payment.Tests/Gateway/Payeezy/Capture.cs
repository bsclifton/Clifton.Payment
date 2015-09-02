using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    public partial class PayeezyGatewayTests {
        [TestMethod]
        public void PayeezyCC_Capture_HappyPath() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var captureResponse = GetReference().CreditCardCapture(authResponse.TransactionId, GetReferenceNumber(), authResponse.TransactionTag, dollarAmount1);

            Assert.IsNotNull(captureResponse.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, captureResponse.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Capture, captureResponse.ParsedTransactionType);
        }
    }
}