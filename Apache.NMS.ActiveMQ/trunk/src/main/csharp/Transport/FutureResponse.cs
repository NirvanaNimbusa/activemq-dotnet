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
using Apache.NMS.ActiveMQ.Commands;
using System;
using System.Threading;
using Apache.NMS.ActiveMQ.Util;
using Apache.NMS;

namespace Apache.NMS.ActiveMQ.Transport
{

	/// <summary>
	/// Handles asynchronous responses
	/// </summary>
	public class FutureResponse
	{

		private static int maxWait = -1;
		public int Timeout
		{
			get { return maxWait; }
			set { maxWait = value; }
		}

		private readonly CountDownLatch latch = new CountDownLatch(1);
		private Response response;

		public WaitHandle AsyncWaitHandle
		{
			get { return latch.AsyncWaitHandle; }
		}

		public Response Response
		{
			// Blocks the caller until a value has been set
			get
			{
				bool waitForResponse = false;

				lock(latch)
				{
					if(null == response)
					{
						waitForResponse = true;
					}
				}

				if(waitForResponse)
				{
					try
					{
						if(!latch.await(maxWait))
						{
							// TODO: Throw timeout exception?
						}
					}
					catch (Exception e)
					{
						Tracer.Error("Caught while waiting on monitor: " + e);
					}
				}

				lock(latch)
				{
					return response;
				}
			}

			set
			{
				lock(latch)
				{
					response = value;
				}

				latch.countDown();
			}
		}
	}
}

