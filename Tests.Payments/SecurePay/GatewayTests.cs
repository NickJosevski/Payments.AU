using System;
using System.IO;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    public class GatewayTests
    {
        protected int ChargeAmount1;

        protected int ChargeAmount2;

        protected SecurePayCardInfo ValidCard;

        protected void SetupCardsAndChargeAmounts()
        {
            var currentVal = Int32.Parse(ReadFromFile(@"..\..\increasing-amount.txt").Trim());

            ChargeAmount1 = currentVal;
            ChargeAmount2 = currentVal + 100;

            WriteToFile(@"..\..\increasing-amount.txt", currentVal + 100);


            ValidCard = new SecurePayCardInfo { Number = "4444333322221111", ExpiryMonth = 10, ExpiryYear = 15 };
        }

        protected void AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(SecurePayMessage r)
        {
            Assert.IsNotNull(r);
            Assert.That(r.Status.StatusDescription, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r.Status.StatusDescription, Is.EqualTo("Normal"));
            Assert.That(r.Status.StatusCode, Is.EqualTo(0));
            Assert.That(r.Periodic.PeriodicList.PeriodicItem[0].Successful, Is.EqualTo("yes"));
        }

        protected string ReadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException(
                    "ReadFromFile() - Unable to find " + Environment.CurrentDirectory + @"\" + filePath);

            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                return reader.ReadToEnd();
            }
        }

        protected void WriteToFile(string filePath, int amount)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException(
                    "WriteToFile() - Unable to find " + Environment.CurrentDirectory + @"\" + filePath);

            using (var writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine(amount);
            }
        }

        protected void DebugDisplay(string xml)
        {
            Console.WriteLine("REQUEST:");
            Console.WriteLine("********************************************************************************");
            Console.WriteLine(xml.Print());
            Console.WriteLine("********************************************************************************");
            Console.WriteLine("");
        }
    }
}