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
using System.Reflection;
using Apache.NMS.ActiveMQ.Commands;
using Apache.NMS.ActiveMQ.OpenWire.V1;
using Apache.NMS.ActiveMQ.Transport;
using System;
using System.IO;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Transport.Tcp;

namespace Apache.NMS.ActiveMQ.OpenWire
{
	/// <summary>
	/// Implements the <a href="http://activemq.apache.org/openwire.html">OpenWire</a> protocol.
	/// </summary>
	public class OpenWireFormat : IWireFormat
	{
		private readonly object marshalLock = new object();
		private BaseDataStreamMarshaller[] dataMarshallers;
		private const byte NULL_TYPE = 0;

		private int version;
		private bool cacheEnabled = false;
		private bool stackTraceEnabled = false;
		private bool tcpNoDelayEnabled = false;
		private bool sizePrefixDisabled = false;
		private bool tightEncodingEnabled = false;
		private long maxInactivityDuration = 0;
		private long maxInactivityDurationInitialDelay = 0;
		private int cacheSize = 0;
		private int minimumVersion = 1;

		private WireFormatInfo preferedWireFormatInfo = new WireFormatInfo();
		private ITransport transport;

		public OpenWireFormat()
		{
			// See the following link for defaults: http://activemq.apache.org/configuring-wire-formats.html
			// See also the following link for OpenWire format info: http://activemq.apache.org/openwire-version-2-specification.html
			PreferedWireFormatInfo.CacheEnabled = false;
			PreferedWireFormatInfo.StackTraceEnabled = false;
			PreferedWireFormatInfo.TcpNoDelayEnabled = true;
			PreferedWireFormatInfo.SizePrefixDisabled = false;
			PreferedWireFormatInfo.TightEncodingEnabled = false;
			PreferedWireFormatInfo.MaxInactivityDuration = 30000;
			PreferedWireFormatInfo.MaxInactivityDurationInitialDelay = 10000;
			PreferedWireFormatInfo.CacheSize = 0;
			PreferedWireFormatInfo.Version = 2;

			dataMarshallers = new BaseDataStreamMarshaller[256];
			Version = 1;
		}

		public ITransport Transport
		{
			get { return transport; }
			set { transport = value; }
		}

		public int Version
		{
			get { return version; }
			set
			{
				Assembly dll = Assembly.GetExecutingAssembly();
				Type type = dll.GetType("Apache.NMS.ActiveMQ.OpenWire.V" + value + ".MarshallerFactory", false);
				IMarshallerFactory factory = (IMarshallerFactory) Activator.CreateInstance(type);
				factory.configure(this);
				version = value;
			}
		}

		public bool CacheEnabled
		{
			get { return cacheEnabled; }
			set { cacheEnabled = value; }
		}

		public bool StackTraceEnabled
		{
			get { return stackTraceEnabled; }
			set { stackTraceEnabled = value; }
		}

		public bool TcpNoDelayEnabled
		{
			get { return tcpNoDelayEnabled; }
			set { tcpNoDelayEnabled = value; }
		}

		public bool SizePrefixDisabled
		{
			get { return sizePrefixDisabled; }
			set { sizePrefixDisabled = value; }
		}

		public bool TightEncodingEnabled
		{
			get { return tightEncodingEnabled; }
			set { tightEncodingEnabled = value; }
		}

		public long MaxInactivityDuration
		{
			get { return maxInactivityDuration; }
			set { maxInactivityDuration = value; }
		}

		public long MaxInactivityDurationInitialDelay
		{
			get { return maxInactivityDurationInitialDelay; }
			set { maxInactivityDurationInitialDelay = value; }
		}

		public int CacheSize
		{
			get { return cacheSize; }
			set { cacheSize = value; }
		}

		public WireFormatInfo PreferedWireFormatInfo
		{
			get { return preferedWireFormatInfo; }
			set { preferedWireFormatInfo = value; }
		}

		public void clearMarshallers()
		{
			lock(this.marshalLock)
			{
				for(int i = 0; i < dataMarshallers.Length; i++)
				{
					dataMarshallers[i] = null;
				}
			}
		}

		public void addMarshaller(BaseDataStreamMarshaller marshaller)
		{
			byte type = marshaller.GetDataStructureType();
			lock(this.marshalLock)
			{
				dataMarshallers[type & 0xFF] = marshaller;
			}
		}

		private BaseDataStreamMarshaller GetDataStreamMarshallerForType(byte dataType)
		{
			BaseDataStreamMarshaller dsm = this.dataMarshallers[dataType & 0xFF];
			if(null == dsm)
			{
				throw new IOException("Unknown data type: " + dataType);
			}
			return dsm;
		}

		public void Marshal(Object o, BinaryWriter ds)
		{
			int size = 1;
			if(o != null)
			{
				DataStructure c = (DataStructure) o;
				byte type = c.GetDataStructureType();
				BaseDataStreamMarshaller dsm;
				bool _tightEncodingEnabled;
				bool _sizePrefixDisabled;

				lock(this.marshalLock)
				{
					dsm = GetDataStreamMarshallerForType(type);
					_tightEncodingEnabled = this.tightEncodingEnabled;
					_sizePrefixDisabled = this.sizePrefixDisabled;
				}

				if(_tightEncodingEnabled)
				{
					BooleanStream bs = new BooleanStream();
					size += dsm.TightMarshal1(this, c, bs);
					size += bs.MarshalledSize();

					if(!_sizePrefixDisabled)
					{
						ds.Write(size);
					}

					ds.Write(type);
					bs.Marshal(ds);
					dsm.TightMarshal2(this, c, ds, bs);
				}
				else
				{
					BinaryWriter looseOut = ds;
					MemoryStream ms = null;

					// If we are prefixing then we need to first write it to memory,
					// otherwise we can write direct to the stream.
					if(!_sizePrefixDisabled)
					{
						ms = new MemoryStream();
						looseOut = new OpenWireBinaryWriter(ms);
						looseOut.Write(size);
					}

					looseOut.Write(type);
					dsm.LooseMarshal(this, c, looseOut);

					if(!_sizePrefixDisabled)
					{
						ms.Position = 0;
						looseOut.Write((int) ms.Length - 4);
						ds.Write(ms.GetBuffer(), 0, (int) ms.Length);
					}
				}
			}
			else
			{
				ds.Write(size);
				ds.Write(NULL_TYPE);
			}
		}

		public Object Unmarshal(BinaryReader dis)
		{
			// lets ignore the size of the packet
			if(!sizePrefixDisabled)
			{
				dis.ReadInt32();
			}

			// first byte is the type of the packet
			byte dataType = dis.ReadByte();

			if(dataType != NULL_TYPE)
			{
				BaseDataStreamMarshaller dsm;
				bool _tightEncodingEnabled;

				lock(this.marshalLock)
				{
					dsm = GetDataStreamMarshallerForType(dataType);
					_tightEncodingEnabled = this.tightEncodingEnabled;
				}

				Tracer.Debug("Parsing type: " + dataType + " with: " + dsm);
				Object data = dsm.CreateObject();

				if(_tightEncodingEnabled)
				{
					BooleanStream bs = new BooleanStream();
					bs.Unmarshal(dis);
					dsm.TightUnmarshal(this, data, dis, bs);
					return data;
				}
				else
				{
					dsm.LooseUnmarshal(this, data, dis);
					return data;
				}
			}
			else
			{
				return null;
			}
		}

		public int TightMarshalNestedObject1(DataStructure o, BooleanStream bs)
		{
			bs.WriteBoolean(o != null);
			if(null == o)
			{
				return 0;
			}

			if(o.IsMarshallAware())
			{
				MarshallAware ma = (MarshallAware) o;
				byte[] sequence = ma.GetMarshalledForm(this);
				bs.WriteBoolean(sequence != null);
				if(sequence != null)
				{
					return 1 + sequence.Length;
				}
			}

			byte type = o.GetDataStructureType();
			if(type == 0)
			{
				throw new IOException("No valid data structure type for: " + o + " of type: " + o.GetType());
			}

			BaseDataStreamMarshaller dsm;
			lock(this.marshalLock)
			{
				dsm = GetDataStreamMarshallerForType(type);
			}

			Tracer.Debug("Marshalling type: " + type + " with structure: " + o);
			return 1 + dsm.TightMarshal1(this, o, bs);
		}

		public void TightMarshalNestedObject2(DataStructure o, BinaryWriter ds, BooleanStream bs)
		{
			if(!bs.ReadBoolean())
			{
				return;
			}

			byte type = o.GetDataStructureType();
			ds.Write(type);

			if(o.IsMarshallAware() && bs.ReadBoolean())
			{
				MarshallAware ma = (MarshallAware) o;
				byte[] sequence = ma.GetMarshalledForm(this);
				ds.Write(sequence, 0, sequence.Length);
			}
			else
			{
				BaseDataStreamMarshaller dsm;

				lock(this.marshalLock)
				{
					dsm = GetDataStreamMarshallerForType(type);
				}

				dsm.TightMarshal2(this, o, ds, bs);
			}
		}

		public DataStructure TightUnmarshalNestedObject(BinaryReader dis, BooleanStream bs)
		{
			if(bs.ReadBoolean())
			{
				DataStructure data;
				BaseDataStreamMarshaller dsm;
				byte dataType = dis.ReadByte();

				lock(this.marshalLock)
				{
					dsm = GetDataStreamMarshallerForType(dataType);
				}

				data = dsm.CreateObject();
				if(data.IsMarshallAware() && bs.ReadBoolean())
				{
					dis.ReadInt32();
					dis.ReadByte();

					BooleanStream bs2 = new BooleanStream();
					bs2.Unmarshal(dis);
					dsm.TightUnmarshal(this, data, dis, bs2);

					// TODO: extract the sequence from the dis and associate it.
					//                MarshallAware ma = (MarshallAware)data
					//                ma.setCachedMarshalledForm(this, sequence);
				}
				else
				{
					dsm.TightUnmarshal(this, data, dis, bs);
				}

				return data;
			}
			else
			{
				return null;
			}
		}

		public void LooseMarshalNestedObject(DataStructure o, BinaryWriter dataOut)
		{
			dataOut.Write(o != null);
			if(o != null)
			{
				BaseDataStreamMarshaller dsm;
				byte type = o.GetDataStructureType();
				dataOut.Write(type);

				lock(this.marshalLock)
				{
					dsm = GetDataStreamMarshallerForType(type);
				}

				dsm.LooseMarshal(this, o, dataOut);
			}
		}

		public DataStructure LooseUnmarshalNestedObject(BinaryReader dis)
		{
			if(dis.ReadBoolean())
			{
				BaseDataStreamMarshaller dsm;
				byte dataType = dis.ReadByte();
				DataStructure data;

				lock(this.marshalLock)
				{
					dsm = GetDataStreamMarshallerForType(dataType);
				}

				data = dsm.CreateObject();
				dsm.LooseUnmarshal(this, data, dis);
				return data;
			}
			else
			{
				return null;
			}
		}

		public void renegotiateWireFormat(WireFormatInfo info)
		{
			lock(this.marshalLock)
			{
				if(info.Version < minimumVersion)
				{
					throw new IOException("Remote wire format (" + info.Version + ") is lower than the minimum version required (" + minimumVersion + ")");
				}

				this.Version = Math.Min(PreferedWireFormatInfo.Version, info.Version);
				this.cacheEnabled = info.CacheEnabled && PreferedWireFormatInfo.CacheEnabled;
				this.stackTraceEnabled = info.StackTraceEnabled && PreferedWireFormatInfo.StackTraceEnabled;
				this.tcpNoDelayEnabled = info.TcpNoDelayEnabled && PreferedWireFormatInfo.TcpNoDelayEnabled;
				this.sizePrefixDisabled = info.SizePrefixDisabled && PreferedWireFormatInfo.SizePrefixDisabled;
				this.tightEncodingEnabled = info.TightEncodingEnabled && PreferedWireFormatInfo.TightEncodingEnabled;
				this.maxInactivityDuration = info.MaxInactivityDuration;
				this.maxInactivityDurationInitialDelay = info.MaxInactivityDurationInitialDelay;
				this.cacheSize = info.CacheSize;

				TcpTransport tcpTransport = this.transport as TcpTransport;
				if(null != tcpTransport)
				{
					tcpTransport.TcpNoDelayEnabled = this.tcpNoDelayEnabled;
				}
			}
		}
	}
}
