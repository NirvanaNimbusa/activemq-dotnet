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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Apache.NMS.Util
{
	/// <summary>
	/// Class to provide support for working with Xml objects.
	/// </summary>
	public class XmlUtil
	{
		public static string Serialize(object obj)
		{
			return Serialize(obj, Encoding.Unicode);
		}

		public static string Serialize(object obj, Encoding encoding)
		{
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				XmlSerializer serializer = new XmlSerializer(obj.GetType());
				XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, encoding);

				/*
				 * If the XML document has been altered with unknown
				 * nodes or attributes, handle them with the
				 * UnknownNode and UnknownAttribute events.
				 */
				serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				serializer.Serialize(xmlTextWriter, obj);
				memoryStream = (MemoryStream) xmlTextWriter.BaseStream;
				byte[] encodedBytes = memoryStream.ToArray();
				return encoding.GetString(encodedBytes, 0, encodedBytes.Length);
			}
			catch(Exception ex)
			{
				Tracer.ErrorFormat("Error serializing object: {0}", ex.Message);
				return null;
			}
		}

		public static object Deserialize(Type objType, string text)
		{
			return Deserialize(objType, text, Encoding.Unicode);
		}

		public static object Deserialize(Type objType, string text, Encoding encoding)
		{
			if(null == text)
			{
				return null;
			}

			try
			{
				XmlSerializer serializer = new XmlSerializer(objType);
				MemoryStream memoryStream = new MemoryStream(encoding.GetBytes(text));

				/*
				 * If the XML document has been altered with unknown
				 * nodes or attributes, handle them with the
				 * UnknownNode and UnknownAttribute events.
				 */
				serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				return serializer.Deserialize(memoryStream);
			}
			catch(Exception ex)
			{
				Tracer.ErrorFormat("Error deserializing object: {0}", ex.Message);
				return null;
			}
		}

		private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
		{
			Tracer.ErrorFormat("Unknown Node: {0}\t{1}", e.Name, e.Text);
		}

		private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			Tracer.ErrorFormat("Unknown attribute: {0}='{1}'", e.Attr.Name, e.Attr.Value);
		}
	}
}
