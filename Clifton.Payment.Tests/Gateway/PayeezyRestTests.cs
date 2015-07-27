using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway {
    [TestClass]
    public class PayeezyRestTests {
        [TestMethod]
        public void PayeezyRest_CreditCardPurchase_HappyPath() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase("4111111111111111", "01", "16", "125", "Bubba Smith", Guid.NewGuid().ToString());
        }
    }
}
