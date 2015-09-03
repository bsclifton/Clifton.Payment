using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    public partial class PayeezyGatewayTests {
        [TestMethod]
        public void PayeezyCC_PurchaseRefund_HappyPath() {
            var purchaseResponse = GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var refundResponse = GetReference().CreditCardRefund(purchaseResponse.TransactionId, validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());

            Assert.IsNotNull(refundResponse.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, refundResponse.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Refund, refundResponse.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyCC_PurchaseRefundTagged_HappyPath() {
            var purchaseResponse = GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var refundResponse = GetReference().CreditCardRefund(purchaseResponse.TransactionId, GetReferenceNumber(), purchaseResponse.TransactionTag, dollarAmount1);

            Assert.IsNotNull(refundResponse.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, refundResponse.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Refund, refundResponse.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyCC_AuthCaptureRefund_HappyPath() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var captureResponse = GetReference().CreditCardCapture(authResponse.TransactionId, GetReferenceNumber(), authResponse.TransactionTag, dollarAmount1);
            var refundResponse = GetReference().CreditCardRefund(captureResponse.TransactionId, GetReferenceNumber(), captureResponse.TransactionTag, dollarAmount1);

            Assert.IsNotNull(refundResponse.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, refundResponse.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Refund, refundResponse.ParsedTransactionType);
        }
    }
}