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
namespace Apache.NMS
{
	/// <summary>
	/// A delegate that can receive messages async.
	/// </summary>
	public delegate void MessageListener(IMessage message);

	/// <summary>
	/// A consumer of messages
	/// </summary>
	public interface IMessageConsumer : System.IDisposable
	{
		
		/// <summary>
		/// Waits until a message is available and returns it
		/// </summary>
		IMessage Receive();
		
		/// <summary>
		/// If a message is available within the timeout duration it is returned otherwise this method returns null
		/// </summary>
		IMessage Receive(System.TimeSpan timeout);
		
		/// <summary>
		/// If a message is available immediately it is returned otherwise this method returns null
		/// </summary>
		IMessage ReceiveNoWait();
		
		/// <summary>
		/// An asynchronous listener which can be used to consume messages asynchronously
		/// </summary>
		event MessageListener Listener;

		/// <summary>
		/// Closes the message consumer. 
		/// </summary>
		/// <remarks>
		/// Clients should close message consumers them when they are not needed.
		/// This call blocks until a receive or message listener in progress has completed.
		/// A blocked message consumer receive call returns null when this message consumer is closed.
		/// </remarks>
		void Close();
	}
}


