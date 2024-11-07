using System;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using ACadSharp;
using ACadSharp.IO;
using ACadSharp.Objects;

namespace ConnectionUnitTests
{
    [TestClass]
    public class CadModeling
    {
        private const char _separatorChar = ';';
        internal const string MagicHeader = "ISO-10303-21";
        internal const string MagicFooter = "END-" + MagicHeader;
        internal const string HeaderText = "HEADER";
        internal const string AnchorText = "ANCHOR";
        internal const string ReferenceText = "REFERENCE";
        internal const string DataText = "DATA";
        internal const string SignatureText = "SIGNATURE";
        internal const string EndSectionText = "ENDSEC";


        [TestMethod]
        public void OpenDwgFile()
        {
            string path = @"D:\CadModeling\TestModels\eMG-100-G60.stp";

            byte[] data = File.ReadAllBytes(path);
            string s = Encoding.ASCII.GetString(data);

            var v = String.Join("\n", s.Split(new char[] { '\n' }).Where(str => !string.IsNullOrWhiteSpace(str)));

            string body = s.Split(new string[] { MagicHeader + _separatorChar, MagicFooter + _separatorChar }, StringSplitOptions.RemoveEmptyEntries).First();

            string head = body.Split(new string[] { HeaderText + _separatorChar, EndSectionText + _separatorChar }, StringSplitOptions.RemoveEmptyEntries).First();

            string Data = body.Split(new string[] { DataText + _separatorChar, EndSectionText + _separatorChar }, StringSplitOptions.RemoveEmptyEntries).First();

            //using (StreamReader stream = new StreamReader(path))
            //{

            //}

            //using (DwgReader reader = new DwgReader(path))
            //{
            //    CadDocument document = reader.Read();
            //}

            //using (var stream = new FileStream(path, FileMode.Open))
            //{

            //}
        }
    }
}
