using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment;
using System.Text;

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

    [TestClass]
    public class ParseFromMicrTests {
        [TestMethod]
        public void parseMicr() {
            string exampleRouting = "011110756",
                   exampleAccountNumber = "123456789",
                   exampleCheckNumber = "1234",
                   exampleMicr = string.Format("t{0}t{1}o{2}", exampleRouting, exampleAccountNumber, exampleCheckNumber);

            Check check = null;

            int newOffset = Check.ParseFromMicr(ASCIIEncoding.ASCII.GetBytes(exampleMicr), 0, out check);

            Assert.AreEqual(exampleRouting, check.RoutingNumber);
            Assert.AreEqual(exampleAccountNumber, check.AccountNumber);
            Assert.AreEqual(exampleCheckNumber, check.CheckNumber);
        }
    }
}