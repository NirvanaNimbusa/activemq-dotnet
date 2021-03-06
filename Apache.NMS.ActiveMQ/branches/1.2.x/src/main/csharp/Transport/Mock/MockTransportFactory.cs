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

using System;
using System.Collections.Specialized;
using Apache.NMS.Util;

namespace Apache.NMS.ActiveMQ.Transport.Mock
{
	/// <summary>
	/// Factory class to create the MockTransport when given on a URI as mock://XXX
	/// </summary>
	public class MockTransportFactory : ITransportFactory
	{
		public MockTransportFactory()
		{
		}

		#region Properties

		private TimeSpan requestTimeout = NMSConstants.defaultRequestTimeout;
		public int RequestTimeout
		{
			get { return (int) requestTimeout.TotalMilliseconds; }
			set { requestTimeout = TimeSpan.FromMilliseconds(value); }
		}

		private bool useLogging = false;
		public bool UseLogging
		{
			get { return useLogging; }
			set { useLogging = value; }
		}

		private string wireFormat = "OpenWire";
		public string WireFormat
		{
			get { return wireFormat; }
			set { wireFormat = value; }
		}

		private bool failOnReceiveMessage = false;
		public bool FailOnReceiveMessage
		{
			get { return failOnReceiveMessage; }
			set { failOnReceiveMessage = value; }
		}

		private int numReceivedMessagesBeforeFail = 0;
		public int NumReceivedMessagesBeforeFail
		{
			get { return numReceivedMessagesBeforeFail; }
			set { numReceivedMessagesBeforeFail = value; }
		}

		private bool failOnSendMessage = false;
		public bool FailOnSendMessage
		{
			get { return failOnSendMessage; }
			set { this.failOnSendMessage = value; }
		}

		private int numSentMessagesBeforeFail = 0;
		public int NumSentMessagesBeforeFail
		{
			get { return numSentMessagesBeforeFail; }
			set { numSentMessagesBeforeFail = value; }
		}

		private bool failOnCreate = false;
		public bool FailOnCreate
		{
			get { return failOnCreate; }
			set { this.failOnCreate = value; }
		}

		#endregion

		public ITransport CreateTransport(Uri location)
		{
			ITransport transport = CompositeConnect(location);

			transport = new MutexTransport(transport);
			transport = new ResponseCorrelator(transport);
			transport.RequestTimeout = this.requestTimeout;

			return transport;
		}

		public ITransport CompositeConnect(Uri location)
		{
			Tracer.Debug("MockTransportFactory: Create new Transport with options: " + location.Query);

			// Extract query parameters from broker Uri
			StringDictionary map = URISupport.ParseQuery(location.Query);

			// Set transport. properties on this (the factory)
			URISupport.SetProperties(this, map, "transport.");

			if(String.Compare(this.wireFormat, "stomp", true) != 0 &&
			   String.Compare(this.wireFormat, "openwire", true) != 0)
			{
				throw new IOException("Unsupported WireFormat Supplied for MockTransport");
			}

			if(this.FailOnCreate == true)
			{
				throw new IOException("Failed to Create new MockTransport.");
			}

			// Create the Mock Transport
			MockTransport transport = new MockTransport();

			transport.FailOnReceiveMessage = this.FailOnReceiveMessage;
			transport.NumReceivedMessagesBeforeFail = this.NumReceivedMessagesBeforeFail;
			transport.FailOnSendMessage = this.FailOnSendMessage;
			transport.NumSentMessagesBeforeFail = this.NumSentMessagesBeforeFail;

			return transport;
		}

		public ITransport CompositeConnect(Uri location, SetTransport setTransport)
		{
			throw new NMSConnectionException("Asynchronous composite connection not supported with Mock transport.");
		}
	}
}
