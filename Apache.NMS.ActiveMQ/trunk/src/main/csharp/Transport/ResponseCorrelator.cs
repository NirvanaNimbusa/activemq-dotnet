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
using System.Collections;

using Apache.NMS.ActiveMQ.Commands;
using Apache.NMS.ActiveMQ.Transport;
using Apache.NMS;

namespace Apache.NMS.ActiveMQ.Transport
{
	
    /// <summary>
    /// A Transport which gaurds access to the next transport using a mutex.
    /// </summary>
    public class ResponseCorrelator : TransportFilter
    {
		private readonly IDictionary requestMap = Hashtable.Synchronized(new Hashtable());
		private readonly Object mutex = new Object();
        private short nextCommandId;
        private int requestTimeout = -1;

        public ResponseCorrelator(ITransport next, int requestTimeout) : base(next)
		{
			this.requestTimeout = requestTimeout;
        }

        short GetNextCommandId()
		{
            lock(mutex)
			{
                return ++nextCommandId;
            }
        }

        public override void Oneway(Command command)
        {
			int commandId = GetNextCommandId();

            command.CommandId = commandId;
            command.ResponseRequired = false;
            next.Oneway(command);
        }

        public override FutureResponse AsyncRequest(Command command)
        {
			int commandId = GetNextCommandId();

        	command.CommandId = commandId;
            command.ResponseRequired = true;
            FutureResponse future = new FutureResponse();
            requestMap[commandId] = future;
			next.Oneway(command);
            return future;

        }

        public override Response Request(Command command)
        {
            FutureResponse future = AsyncRequest(command);
            future.Timeout = requestTimeout;
            Response response = future.Response;
            if (response != null && response is ExceptionResponse)
            {
                ExceptionResponse er = (ExceptionResponse) response;
                BrokerError brokerError = er.Exception;
				if (brokerError == null)
				{
	                throw new BrokerException();
				}
				else
				{
	                throw new BrokerException(brokerError);
				}
            }
            return response;
        }

        protected override void OnCommand(ITransport sender, Command command)
        {
            if(command is Response)
			{
                Response response = (Response) command;
				int correlationId = response.CorrelationId;

				FutureResponse future = (FutureResponse) requestMap[correlationId];
                
				if(future != null)
                {
					requestMap.Remove(correlationId);
					future.Response = response;

					if(response is ExceptionResponse)
					{
						ExceptionResponse er = (ExceptionResponse) response;
						BrokerError brokerError = er.Exception;
						BrokerException exception = new BrokerException(brokerError);
						this.exceptionHandler(this, exception);
					}
				}
				else
				{
					Tracer.Error("Unknown response ID: " + response.CommandId + " for response: " + response);
				}
            }
            else if(command is ShutdownInfo)
            {
                // lets shutdown
                this.commandHandler(sender, command);
            }
			else
			{
                this.commandHandler(sender, command);
            }
        }
    }
}

