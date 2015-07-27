using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway {
    [TestClass]
    public class PayeezyGatewayTests {
        [TestMethod]
        public void PayeezyGateway_CreditCardPurchase_HappyPath() {
            PayeezyGateway gateway = new PayeezyGateway();
            gateway.CreditCardPurchase("4111111111111111", "01", "16", "1.25", "Bubba Smith", Guid.NewGuid().ToString());
        }
    }
}