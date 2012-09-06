using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Tests.Payments
{
    [TestFixture]
    public class Read
    {
        [Test]
        public void TestReadFromFile()
        {
            var result = ReadFromFile(@"testfile.txt");

            Assert.That(result, Is.StringContaining("content of the file"));
        }

        public string ReadFromFile(string filePath)
        {
            if(!File.Exists(filePath))
                throw new InvalidOperationException("ReadFromFile() - Unable to find " + Environment.CurrentDirectory + @"\" + filePath);

            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
