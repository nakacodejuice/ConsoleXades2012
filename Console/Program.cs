using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xades.Abstractions;
using Xades.Helpers;
using Xades.Implementations;
using System.Security.Cryptography.Xml;



namespace Consoleapl
{
    class Program
    {
        private static readonly string[] _idAttributeNames = { "Id", "id", "ID", "iD", "_Id", "_id", "_ID", "_iD" };
        private static readonly IXadesService _xadesService;
       // public static IXadesService xadesService;

        static void Main(string[] args)
        {
            var args0 = "C:\\Xades-master\\in.xml";
            var args1 = "a06356a7e8bd4239ad69b3e9c949bca1";
            var args2 = "e8408bff2fd7853498277abcd98219a8a952380f";
            //var args2 = "‎896bbcfb7b416cf3fbc7fd0325f25f75655969fb";
            var args3 = "";
            var args4= "C:\\Xades-master\\out.xml";
            var xmlDocument = XmlDocumentHelper.Load(args0);
            /*XmlDsigExcC14NTransform t = new XmlDsigExcC14NTransform();
            t.LoadInput(xmlDocument);
            MemoryStream stream = (MemoryStream)t.GetOutput(typeof(MemoryStream));
            */
            /*
            XmlDocument xm = new XmlDocument { PreserveWhitespace = true };
            string xmstring = xmlDocument.OuterXml.Replace("\r", "");
            xm.LoadXml(xmstring);
            */
            var rootNode = xmlDocument.DocumentElement;
            var rootNodeId = GetRootId(rootNode);
           // _xadesService.Sign(xmlDocument.OuterXml, "{1}", "‎‎bcfefb11f83b628378cb2503623cce5521bcacc3", "123");
            var res = Sign(xmlDocument, args1, args2, args3);
            //XmlDocument xm = new XmlDocument { PreserveWhitespace = false };
            //xm.PreserveWhitespace = false;
           // xm.LoadXml(res);
            res.Save(args4);
            //Validate(xm, args1);
            //PrintMessage(message, ConsoleColor.Yellow);
        }


        private static string GetRootId(XmlNode rootId)
        {
            var idName = _idAttributeNames.SingleOrDefault(x => rootId.Attributes[x] != null);
            return !string.IsNullOrEmpty(idName) ? rootId.Attributes[idName].Value : null;
        }

        private static XmlDocument Sign(XmlDocument xml, string elementId, string CertificateThumbprint, string Password)
        {
            var xadesService = new GostXadesBesService();
            if (string.IsNullOrEmpty(elementId))
            {
           
                var rootNode = xml.DocumentElement;
                var rootNodeId = GetRootId(rootNode);
                if (!string.IsNullOrEmpty(rootNodeId))
                {
                    Warning(string.Format("Не задан элемент для подписи. Используется корневой элемент {0} с Id {1}", rootNode.Name, rootNodeId));
                    elementId = rootNodeId;
                }
                else
                {
                    elementId = Guid.NewGuid().ToString("N");
                    var attribulte = xml.CreateAttribute("Id");
                    attribulte.Value = elementId;
                    rootNode.Attributes.Append(attribulte);
                    Warning(string.Format("Не задан элемент для подписи. Используется корневой элемент {0} с Id {1} (атрибут сгенерирован)",rootNode.Name, elementId));
                }
            }

            return xadesService.Sign(xml.OuterXml, elementId, CertificateThumbprint, Password);
        }
        private static XmlDocument SignSTR(string xml, string elementId, string CertificateThumbprint, string Password)
        {
            var xadesService = new GostXadesBesService();


            return xadesService.Sign(xml, elementId, CertificateThumbprint, Password);
        }
        protected static void Warning(string message)
        {
            PrintMessage(message, ConsoleColor.Yellow);
        }

        private static void PrintMessage(string message, ConsoleColor color = ConsoleColor.White, bool canWrite = true)
        {
            if (canWrite)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        protected static XmlDocument SignNode(XmlDocument xml, string xpath, string CertificateThumbprint, string Password)
        {
            var manager = xml.CreateNamespaceManager();
            var node = xml.SelectSingleNode(xpath, manager);
            if (node == null)
            {
                throw new InvalidOperationException(string.Format("Не удалось найти узел{0}", xpath));
            }
            var nodeId = node.Attributes["id"];

            if (nodeId == null)
            {
                nodeId = xml.CreateAttribute("Id");
                node.Attributes.Append(nodeId);
            }

            if (string.IsNullOrEmpty(nodeId.Value))
            {
                nodeId.Value = Guid.NewGuid().ToString("N");
            }

            return  Sign(xml, nodeId.Value, CertificateThumbprint,Password);
        }

        protected static void Validate(XmlDocument xml, string elementId)
        {
            var xadesService = new GostXadesBesService();
            if (string.IsNullOrEmpty(elementId))
            {
                var rootNode = xml.DocumentElement;
                var rootNodeId = GetRootId(rootNode);
                if (!string.IsNullOrEmpty(rootNodeId))
                {
                    Warning(string.Format("Не задан элемент для проверки подписи. Используется элемент {0} с Id {1}", rootNode.Name, rootNodeId));
                    elementId = rootNodeId;
                }
                else
                {
                    throw new ArgumentException("Не задан Id элемента для проверки подписи и корневой элемент не имеет Id");
                }
            }
            xadesService.ValidateSignature(xml.OuterXml, elementId);
        }
    }
}
