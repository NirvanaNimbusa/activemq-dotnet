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
using Apache.NMS.Policies;

namespace Apache.NMS.ZMQ
{
	/// <summary>
	/// A Factory that can estbalish NMS connections to ZMQ subscriber
	/// </summary>
	public class ConnectionFactory : IConnectionFactory
	{
		private Uri brokerUri;
		private string clientID;
		private IRedeliveryPolicy redeliveryPolicy = new RedeliveryPolicy();

		public ConnectionFactory(Uri brokerUri, string clientID)
		{
			this.brokerUri = brokerUri;
			this.clientID = clientID;
		}

		/// <summary>
		/// Creates a new connection to ZMQ.
		/// </summary>
		public IConnection CreateConnection()
		{
			return CreateConnection(string.Empty, string.Empty, false);
		}

		/// <summary>
		/// Creates a new connection to ZMQ.
		/// </summary>
		public IConnection CreateConnection(string userName, string password)
		{
			return CreateConnection(userName, password, false);
		}

		/// <summary>
		/// Creates a new connection to ZMQ.
		/// </summary>
		public IConnection CreateConnection(string userName, string password, bool useLogging)
		{
			IConnection ReturnValue = null;
			Connection connection = new Connection();

			connection.RedeliveryPolicy = this.redeliveryPolicy.Clone() as IRedeliveryPolicy;
			connection.ConsumerTransformer = this.consumerTransformer;
			connection.ProducerTransformer = this.producerTransformer;
			connection.BrokerUri = this.BrokerUri;
			connection.ClientId = this.clientID;
			ReturnValue = connection;

			return ReturnValue;
		}

		/// <summary>
		/// Get/or set the broker Uri.
		/// </summary>
		public Uri BrokerUri
		{
			get { return brokerUri; }
			set { brokerUri = value; }
		}

		/// <summary>
		/// Get/or set the redelivery policy that new IConnection objects are
		/// assigned upon creation.
		/// </summary>
		public IRedeliveryPolicy RedeliveryPolicy
		{
			get { return this.redeliveryPolicy; }
			set
			{
				if(value != null)
				{
					this.redeliveryPolicy = value;
				}
			}
		}

		private ConsumerTransformerDelegate consumerTransformer;
		public ConsumerTransformerDelegate ConsumerTransformer
		{
			get { return this.consumerTransformer; }
			set { this.consumerTransformer = value; }
		}

		private ProducerTransformerDelegate producerTransformer;
		public ProducerTransformerDelegate ProducerTransformer
		{
			get { return this.producerTransformer; }
			set { this.producerTransformer = value; }
		}
	}
}
