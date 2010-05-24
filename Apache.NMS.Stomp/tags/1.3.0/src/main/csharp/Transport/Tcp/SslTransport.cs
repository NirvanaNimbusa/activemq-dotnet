/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if !NETCF

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Apache.NMS.Stomp.Transport.Tcp
{
    public class SslTransport : TcpTransport
    {
        private string serverName;
        private string clientCertSubject;
        private string clientCertFilename;
        private string clientCertPassword;
        private string keyStoreName;
        private string keyStoreLocation;
        private bool acceptInvalidBrokerCert = false;
        
        private SslStream sslStream;

        public SslTransport(Uri location, Socket socket, IWireFormat wireFormat) :
            base(location, socket, wireFormat)
        {
        }

        ~SslTransport()
        {
            Dispose(false);
        }

        /// <summary>
        /// Indicates the name of the Server's Certificate.  By default the Host name
        /// of the remote server is used, however if this doesn't match the name of the
        /// Server's certificate then this option can be set to override the default.
        /// </summary>
        public string ServerName
        {
            get { return this.serverName; }
            set { this.serverName = value; }
        }

        public string ClientCertSubject
        {
            get { return this.clientCertSubject; }
            set { this.clientCertSubject = value; }
        }

        /// <summary>
        /// Indicates the location of the Client Certificate to use when the Broker
        /// is configured for Client Auth (not common).  The SslTransport will supply
        /// this certificate to the SslStream via the SelectLocalCertificate method.
        /// </summary>
        public string ClientCertFilename
        {
            get { return this.clientCertFilename; }
            set { this.clientCertFilename = value; }
        }

        /// <summary>
        /// Password for the Client Certificate specified via configuration.
        /// </summary>
        public string ClientCertPassword
        {
            get { return this.clientCertPassword; }
            set { this.clientCertPassword = value; }
        }
       
        /// <summary>
        /// Indicates if the SslTransport should ignore any errors in the supplied Broker
        /// certificate and connect anyway, this is useful in testing with a default AMQ
        /// broker certificate that is self signed.
        /// </summary>
        public bool AcceptInvalidBrokerCert
        {
            get { return this.acceptInvalidBrokerCert; }
            set { this.acceptInvalidBrokerCert = value; }
        }

        public string KeyStoreName
        {
            get { return this.keyStoreName; }
            set { this.keyStoreName = value; }
        }

        public string KeyStoreLocation
        {
            get { return this.keyStoreLocation; }
            set { this.keyStoreLocation = value; }
        }

        protected override Stream CreateSocketStream()
        {
            if(this.sslStream != null)
            {
                return this.sslStream;
            }

            this.sslStream = new SslStream(
                new NetworkStream(this.socket), 
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                new LocalCertificateSelectionCallback(SelectLocalCertificate) );

            try
            {

                string remoteCertName = this.serverName ?? this.RemoteAddress.Host;
                Tracer.Debug("Authorizing as Client for Server: " + remoteCertName);
                sslStream.AuthenticateAsClient(remoteCertName, LoadCertificates(), SslProtocols.Default, false);
                Tracer.Debug("Server is Authenticated = " + sslStream.IsAuthenticated);
                Tracer.Debug("Server is Encrypted = " + sslStream.IsEncrypted);                
            }
            catch(Exception e)
            {
                Tracer.ErrorFormat("Exception: {0}", e.Message);
                if(e.InnerException != null)
                {
                    Tracer.ErrorFormat("Inner exception: {0}", e.InnerException.Message);
                }
                Tracer.Error("Authentication failed - closing the connection.");

                throw e;
            }

            return sslStream;
        }

        private bool ValidateServerCertificate(object sender,
                                               X509Certificate certificate,
                                               X509Chain chain,
                                               SslPolicyErrors sslPolicyErrors)
        {
            Tracer.DebugFormat("ValidateServerCertificate: Issued By {0}", certificate.Issuer);
            if(sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            Tracer.WarnFormat("Certificate error: {0}", sslPolicyErrors.ToString());
            if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                Tracer.Error("Chain Status errors: ");
                foreach( X509ChainStatus status in chain.ChainStatus )
                {
                    Tracer.Error("*** Chain Status error: " + status.Status);
                    Tracer.Error("*** Chain Status information: " + status.StatusInformation);
                }
            }
            else if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                Tracer.Error("Mismatch between Remote Cert Name.");
            }
            else if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                Tracer.Error("The Remote Certificate was not Available.");
            }

            // Configuration may or may not allow us to connect with an invliad broker cert.
            return AcceptInvalidBrokerCert;
        }
        
        private X509Certificate SelectLocalCertificate(object sender,
                                                       string targetHost, 
                                                       X509CertificateCollection localCertificates, 
                                                       X509Certificate remoteCertificate, 
                                                       string[] acceptableIssuers)
        {    
            Tracer.DebugFormat("Client is selecting a local certificate from {0} possibilities.", localCertificates.Count);

            if(localCertificates.Count == 1)
            {
                Tracer.Debug("Client has selected certificate with Subject = " + localCertificates[0].Subject);
                return localCertificates[0];
            }
            else if(localCertificates.Count > 1 && this.clientCertSubject != null)
            {
                foreach(X509Certificate2 certificate in localCertificates)
                {
                    Tracer.Debug("Checking Client Certificate := " + certificate.ToString());
                    if(String.Compare(certificate.Subject, this.clientCertSubject, true) == 0)
                    {
                        Tracer.Debug("Client has selected certificate with Subject = " + certificate.Subject);
                        return certificate;
                    }
                }
            }

            Tracer.Debug("Client did not select a Certificate, returning null.");
            return null;
        }

        private X509Certificate2Collection LoadCertificates()
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();

            if(!String.IsNullOrEmpty(this.clientCertFilename))
            {
                Tracer.Debug("Attempting to load Client Certificate from file := " + this.clientCertFilename);
                X509Certificate2 certificate = new X509Certificate2(this.clientCertFilename, this.clientCertPassword);
                Tracer.Debug("Loaded Client Certificate := " + certificate.ToString());

                collection.Add(certificate);
            }
            else
            {
                string name = String.IsNullOrEmpty(this.keyStoreName) ? StoreName.My.ToString() : this.keyStoreName;

                StoreLocation location = StoreLocation.CurrentUser;

                if(!String.IsNullOrEmpty(this.keyStoreLocation))
                {
                    if(String.Compare(this.keyStoreLocation, "CurrentUser", true) == 0)
                    {
                        location = StoreLocation.CurrentUser;
                    }
                    else if(String.Compare(this.keyStoreLocation, "LocalMachine", true) == 0)
                    {
                        location = StoreLocation.LocalMachine;
                    }
                    else
                    {
                        throw new NMSException("Invlalid StoreLocation given on URI");
                    }
                }

                X509Store store = new X509Store(name, location);

                collection = store.Certificates;
            }

            return collection;
        }
        
    }
}

#endif