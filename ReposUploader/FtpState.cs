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
                this.wait=new ManualResetEvent(false);
            }

            public ManualResetEvent OperationComplete
            {
                get { return this.wait; }
            }

            public FtpWebRequest Request
            {
                get {
                if (this.request == null)
                    this.request = (FtpWebRequest) FtpWebRequest.Create("");
                return this.request; }
                set { this.request=value; }
            }

            public string FileName
            {
                get {
                if (this.fileName == null)
                    this.fileName = string.Empty;
                return this.fileName; }
                set { this.fileName=value; }
            }

            public Exception OperationException
            {
                get {
                //if (this.operationException == null)
                //    this.operationException = new Exception();
                return this.operationException;
            }
                set { this.operationException=value; }
            }

            public string StatusDescription
            {
                get {
                if (this.status == null)
                    this.status = string.Empty;                
                return this.status;
            }
                set { this.status=value; }
            }

            public void Dispose()
            {
            GC.SuppressFinalize(this);
            }
        }
    }