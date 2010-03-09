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
        private string clientCertLocation;
        private string clientCertPassword;
        
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
        /// Indicates the location of the Client Certificate to use when the Broker
        /// is configured for Client Auth (not common).  The SslTransport will supply
        /// this certificate to the SslStream via the SelectLocalCertificate method.
        /// </summary>
        public string ClientCertLocation
        {
            get { return this.clientCertLocation; }
            set { this.clientCertLocation = value; }
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
        
        protected override Stream CreateSocketStream()
        {
            if(this.sslStream != null)
            {
                return this.sslStream;
            }
            
            LocalCertificateSelectionCallback clientCertSelect = null;
            
            if(this.clientCertLocation != null )
            {
                clientCertSelect = new LocalCertificateSelectionCallback(SelectLocalCertificate);
            }

            this.sslStream = new SslStream(
                new NetworkStream(this.socket), 
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                clientCertSelect );

            try
            {
                Tracer.Debug("Authorizing as Client for Server: " + this.RemoteAddress.Host);
                sslStream.AuthenticateAsClient(this.RemoteAddress.Host);
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
            Tracer.Debug("Client is selecting a local certificate.");
        
            X509Certificate2 certificate = new X509Certificate2( clientCertLocation, clientCertPassword );
                        
            return certificate;
        }
        
    }
}

#endif
