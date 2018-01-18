using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clifton.Payment.Gateway;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clifton.Payment.Tests.Gateway.Payeezy
{
    public partial class PayeezyGatewayTests
    {
        [TestMethod]
        public void Payeezy_Token_Purchase_HappyPath()
        {
            var response = GetReference().TokenPurchase(validToken, "Visa", month, twoDigitYear, dollarAmount1, cardHolderName, GetReferenceNumber());

            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Purchase, response.ParsedTransactionType);
        }
    }
}
