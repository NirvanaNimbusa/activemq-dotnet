//
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements.  See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to You under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License.  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;

namespace Apache.NMS.MQTT.Transport
{
	/// <summary>
	/// Response type.
	/// </summary>
    public class Response : BaseCommand
    {
        short correlationId;

        ///
        /// <summery>
        ///  Returns a string containing the information for this DataStructure
        ///  such as its type and value of its elements.
        /// </summery>
        ///
        public override string ToString()
        {
            return GetType().Name + "[ " + 
                "commandId = " + this.CommandId + ", " + 
                "responseRequired = " + this.ResponseRequired + ", " + 
                "CorrelationId = " + CorrelationId + " ]";
        }

        public short CorrelationId
        {
            get { return correlationId; }
            set { this.correlationId = value; }
        }

        ///
        /// <summery>
        ///  Return an answer of true to the isResponse() query.
        /// </summery>
        ///
        public override bool IsResponse
        {
            get { return true; }
        }

        public virtual bool IsErrorResponse
        {
            get { return false; }
        }

    };
}

