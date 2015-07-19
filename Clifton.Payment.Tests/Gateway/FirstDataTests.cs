using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway {
    [TestClass]
    public class FirstDataTests {
        [TestMethod]
        public void CreditCardPurchase_HappyPath() {
            FirstData gateway = new FirstData();
            gateway.CreditCardPurchase("4111111111111111", "01", "16", "1.25", "Bubba Smith");
        }
    }
}