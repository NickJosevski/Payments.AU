using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Payments.SecurePay
{
    public static class XmlHelpers
    {
        public static string ToStringWithDeclaration(this XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }

        public static TReturnType As<TReturnType>(this string xml) where TReturnType : class
        {
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = new XmlTextReader(stringReader))
            {
                return new XmlSerializer(typeof(TReturnType)).Deserialize(xmlReader) as TReturnType;
            }
        }

        public static string SerializeObject<T>(this T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());
            var textWriter = new Utf8StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static string ExceptBlanks(this string str)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                    case '\t':
                    case ' ':
                        continue;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public static string Print(this SecurePayMessage message)
        {
            return message.SerializeObject();
        }

        public static string Print(this string xml)
        {
            using (var stream = new MemoryStream())
            using (var writer = new XmlTextWriter(stream, Encoding.Unicode))
            {
                var document = new XmlDocument();

                // Create an XmlDocument with the xml
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                stream.Flush();

                // Have to rewind the MemoryStream in order to read the content
                stream.Position = 0;

                // Read MemoryStream contents into a StreamReader
                var sReader = new StreamReader(stream);

                // Extract the text from the StreamReader
                return sReader.ReadToEnd();
            }
        }
    }
    
    public sealed class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter()
        { }

        public Utf8StringWriter(StringBuilder sb)
            : base(sb) { }

        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }
}