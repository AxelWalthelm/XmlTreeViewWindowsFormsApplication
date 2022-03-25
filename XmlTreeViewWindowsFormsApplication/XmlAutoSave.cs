using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace XmlTreeViewWindowsFormsApplication
{
    [ToolboxItem(true)]
    //[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Custom)]
    [ToolboxBitmap(typeof(Timer))]
    public sealed class XmlAutoSave : Component
    {
        private readonly Timer _timer = new Timer();
        public EventHandler OnAutoSave;

        public int Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public XmlAutoSave()
        {
            _timer.Tick += OnTick;
        }

        public XmlAutoSave(IContainer container) : this()
        {
            container.Add(this);
        }

        private void OnTick(object sender, EventArgs e)
        {
            _timer.Enabled = false;
            OnAutoSave?.Invoke(_xmlDocument, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                UnregisterEvents();
            }

            base.Dispose(disposing);
        }

        private XmlDocument _xmlDocument;
        public XmlDocument XmlDocument
        {
            get { return _xmlDocument; }
            set
            {
                UnregisterEvents();
                _xmlDocument = value;
                RegisterEvents();
            }
        }

        private void RegisterEvents()
        {
            if (_xmlDocument == null)
                return;

            _xmlDocument.NodeChanged += OnDocumentNodeModified;
            _xmlDocument.NodeInserted += OnDocumentNodeModified;
            _xmlDocument.NodeRemoved += OnDocumentNodeModified;
        }

        private void UnregisterEvents()
        {
            if (_xmlDocument == null)
                return;

            _xmlDocument.NodeChanged -= OnDocumentNodeModified;
            _xmlDocument.NodeInserted -= OnDocumentNodeModified;
            _xmlDocument.NodeRemoved -= OnDocumentNodeModified;
        }

        private void OnDocumentNodeModified(object sender, XmlNodeChangedEventArgs e)
        {
            _timer.Enabled = false;
            _timer.Enabled = true;
        }
    }
}
