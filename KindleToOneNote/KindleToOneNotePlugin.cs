using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Extensibility;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.OneNote;
using Microsoft.Office.Core;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Drawing.Imaging;

using System.Xml;
using System.Xml.Linq;

using SharpKindleHighlightsExtractorLib;

namespace KindleToOneNote
{
    [GuidAttribute("110ED24A-5074-48BC-9B7B-A930BB250583"), ProgId("KindleToOneNote.KindleToOneNotePlugin")]
    public class KindleToOneNotePlugin : IDTExtensibility2, IRibbonExtensibility
    {
        ApplicationClass onApp = new ApplicationClass();
        private object applicationObject;
        private object addInInstance;

        public void OnAddInsUpdate(ref Array custom) { }

        public void OnBeginShutdown(ref Array custom)
        {
            if (onApp != null)
            {
                onApp = null;
            }
        }

        public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
            try
            {
                //onApp = (ApplicationClass)Application;
                MessageBox.Show("CSOneNoteRibbonAddIn OnConnection");
                applicationObject = Application;
                addInInstance = AddInInst;
            }
            catch (Exception e)
            {
                System.IO.File.WriteAllText(@"C:\test\test.txt", e.Message);
            }
        }

        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            onApp = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void OnStartupComplete(ref Array custom) { }

        public string GetCustomUI(string RibbonID)
        {
            return Properties.Resources.ribbon;
        }

        private XDocument GetXDocForNewPage(string pageId, XNamespace ns, string title, string content)
        {
            var page = new XDocument(new XElement(ns + "Page",
                                 new XElement(ns + "Title",
                                     new XElement(ns + "OE", 
                                         new XElement(ns + "T",
                                             new XCData(title)))),
                                 new XElement(ns + "Outline",
                                   new XElement(ns + "OEChildren",
                                     new XElement(ns + "OE",
                                       new XElement(ns + "T",
                                         new XCData(content)))))));
            page.Root.SetAttributeValue("ID", pageId);
            return page;
        }

        public void KindleSync(IRibbonControl control)
        {
            // Extract information from the form.
            //
            AmazonRegistrationForm inputForm = new AmazonRegistrationForm();
            inputForm.ShowDialog();
            inputForm.Focus();

            if (inputForm.User == null)
            {
                return;
            }

            string username = inputForm.User.UserName;
            string password = inputForm.User.Password;

            string sectionId = onApp.Windows.CurrentWindow.CurrentSectionId;

            // Getting content from amazon site.
            //
            HighlightsExtractor extractor = new HighlightsExtractor();
            extractor.LogIn(username, password);

            foreach (BookWithHighlights bookWithHighlights in extractor.Crawl())
            {
                StringBuilder sb = new StringBuilder();
                foreach (string highlight in bookWithHighlights.Highlights)
                {
                    sb.AppendLine(highlight);
                    sb.AppendLine();
                }

                string newPageId;
                onApp.CreateNewPage(sectionId, out newPageId);

                string newPageContent;
                onApp.GetPageContent(newPageId, out newPageContent);
                var doc = XDocument.Parse(newPageContent);
                var ns = doc.Root.Name.Namespace;

                onApp.UpdatePageContent(GetXDocForNewPage(newPageId, ns, bookWithHighlights.BookName, sb.ToString()).ToString());
            }
        }

        public IStream GetImage(string ImageName)
        {
            MemoryStream mem = new MemoryStream();
            Properties.Resources.kindle.Save(mem, ImageFormat.Png);
            return new CCOMStreamWrapper(mem);
        }
    }
}
