//
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements.  See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to You under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License.  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;

namespace Apache.NMS.MQTT.Commands
{
	/// <summary>
	/// Connection initiation command sent from client to server.
	/// 
	/// The payload contains one or more UTF-8 encoded strings. They specify a unqiue
    /// identifier for the client, a Will topic and message and the User Name and Password
    /// to use. All but the first are optional and their presence is determined based on flags
    /// in the variable header.
	/// </summary>
	public class CONNECT
	{
		public const byte TYPE = 1;

		private byte version = 3;
		public byte Version
		{
			get { return this.version; }
			set { this.version = value; }
		}

		private bool willRetain;
		public bool WillRetain
		{
			get { return this.willRetain; }
			set { this.willRetain = value; }
		}

		private byte willQoS = 3;
		public byte WillQoS
		{
			get { return this.willQoS; }
			set { this.willQoS = value; }
		}

		private String username;
		public String UserName
		{
			get { return this.username; }
			set { this.username = value; }
		}

		private String password;
		public String Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		private bool cleanSession;
		public bool CleanSession
		{
			get { return this.cleanSession; }
			set { this.cleanSession = value; }
		}

		private short keepAliveTimer = 10;
		public bool KeepAliveTimer
		{
			get { return this.keepAliveTimer; }
			set { this.keepAliveTimer = value; }
		}

		private String willTopic;
		public String WillTopic
		{
			get { return this.willTopic; }
			set { this.willTopic = value; }
		}

		private String willMessage;
		public String WillMessage
		{
			get { return this.willMessage; }
			set { this.willMessage = value; }
		}

		public CONNECT()
		{
		}
	}
}
