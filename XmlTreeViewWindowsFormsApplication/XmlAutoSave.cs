using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace XmlTreeViewWindowsFormsApplication
{
    public sealed class XmlAutoSave : Component
    {
        private readonly Timer _timer = new Timer{ AutoReset = false, Interval = 50 };

        public EventHandler OnAutoSave;

        public double DelayInterval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public XmlAutoSave()
        {
            _timer.Elapsed += (object sender, ElapsedEventArgs e) => OnAutoSave?.Invoke(_xmlDocument, new EventArgs());
        }

        public XmlAutoSave(IContainer container) : this()
        {
            container.Add(this);
        }

        protected override void Dispose(bool disposing)
        {
            _timer.Dispose();
            UnregisterEvents();
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
