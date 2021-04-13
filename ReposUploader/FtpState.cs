using System;
using System.Net;
using System.Threading;

namespace ReposUploader
{
    public class FtpState : IDisposable
    {
        private readonly ManualResetEvent wait;
        private FtpWebRequest request;
        private string fileName;
        private Exception operationException;
        string status;

        public FtpState()
        {
            this.wait = new ManualResetEvent(false);
        }

        public ManualResetEvent OperationComplete
        {
            get { return this.wait; }
        }

        public FtpWebRequest Request
        {
            get { return this.request; }
            set { this.request = value; }
        }

        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        public Exception OperationException
        {
            get { return this.operationException; }
            set { this.operationException = value; }
        }

        public string StatusDescription
        {
            get { return this.status; }
            set { this.status = value; }
        }

        public void Dispose()
        {
            //Nada que hacer
        }
    }
}