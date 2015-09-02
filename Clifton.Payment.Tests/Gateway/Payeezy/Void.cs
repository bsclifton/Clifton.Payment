using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    public partial class PayeezyGatewayTests {
        [TestMethod]
        public void PayeezyCC_Void_HappyPath() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var voidResponse = GetReference().CreditCardVoid(authResponse.TransactionId, GetReferenceNumber(), authResponse.TransactionTag, dollarAmount1);

            Assert.IsNotNull(voidResponse.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, voidResponse.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Void, voidResponse.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyCC_Void_DifferentDollarAmount() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var voidResponse = GetReference().CreditCardVoid(authResponse.TransactionId, GetReferenceNumber(), authResponse.TransactionTag, dollarAmount2);

            Assert.AreEqual(PayeezyGateway.TransactionStatus.NotProcessed, voidResponse.ParsedTransactionStatus);
        }
    }
}
