using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment;

namespace Clifton.Payment.Tests {
    [TestClass]
    public class HasValidRoutingNumberTests {
        //Test using all fedwire participants. Downloaded from:
        //https://www.frbservices.org/EPaymentsDirectory/download.html

        //Download link:
        //https://www.frbservices.org/EPaymentsDirectory/fpddir.txt

        //Description of format: https://www.frbservices.org/EPaymentsDirectory/fedwireFormat.html
        [TestMethod]
        public void hasValidRoutingTestAllFedwire() {
            FileStream fs = File.OpenRead("fpddir.txt");
            StreamReader sr = new StreamReader(fs);
            string fileContents = sr.ReadToEnd();
            sr.Close();

            string[] lines = fileContents.Trim().Split('\n');
            foreach (string line in lines) {
                string routingNumber = line.Trim().Substring(0, 9);
                bool result = Check.IsValidRoutingNumber(routingNumber);
                Assert.IsTrue(result);
            }
        }
    }
}