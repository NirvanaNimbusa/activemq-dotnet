/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/*
 *
 *  Marshaler code for OpenWire format for MessageDispatchNotification
 *
 *  NOTE!: This file is auto generated - do not modify!
 *         if you need to make a change, please see the Java Classes
 *         in the nms-activemq-openwire-generator module
 *
 */

using System;
using System.IO;

using Apache.NMS.ActiveMQ.Commands;

namespace Apache.NMS.ActiveMQ.OpenWire.V10
{
    /// <summary>
    ///  Marshalling code for Open Wire Format for MessageDispatchNotification
    /// </summary>
    class MessageDispatchNotificationMarshaller : BaseCommandMarshaller
    {
        /// <summery>
        ///  Creates an instance of the Object that this marshaller handles.
        /// </summery>
        public override DataStructure CreateObject() 
        {
            return new MessageDispatchNotification();
        }

        /// <summery>
        ///  Returns the type code for the Object that this Marshaller handles..
        /// </summery>
        public override byte GetDataStructureType() 
        {
            return MessageDispatchNotification.ID_MESSAGEDISPATCHNOTIFICATION;
        }

        // 
        // Un-marshal an object instance from the data input stream
        // 
        public override void TightUnmarshal(OpenWireFormat wireFormat, Object o, BinaryReader dataIn, BooleanStream bs) 
        {
            base.TightUnmarshal(wireFormat, o, dataIn, bs);

            MessageDispatchNotification info = (MessageDispatchNotification)o;
            info.ConsumerId = (ConsumerId) TightUnmarshalCachedObject(wireFormat, dataIn, bs);
            info.Destination = (ActiveMQDestination) TightUnmarshalCachedObject(wireFormat, dataIn, bs);
            info.DeliverySequenceId = TightUnmarshalLong(wireFormat, dataIn, bs);
            info.MessageId = (MessageId) TightUnmarshalNestedObject(wireFormat, dataIn, bs);
        }

        //
        // Write the booleans that this object uses to a BooleanStream
        //
        public override int TightMarshal1(OpenWireFormat wireFormat, Object o, BooleanStream bs)
        {
            MessageDispatchNotification info = (MessageDispatchNotification)o;

            int rc = base.TightMarshal1(wireFormat, o, bs);
            rc += TightMarshalCachedObject1(wireFormat, (DataStructure)info.ConsumerId, bs);
            rc += TightMarshalCachedObject1(wireFormat, (DataStructure)info.Destination, bs);
            rc += TightMarshalLong1(wireFormat, info.DeliverySequenceId, bs);
        rc += TightMarshalNestedObject1(wireFormat, (DataStructure)info.MessageId, bs);

            return rc + 0;
        }

        // 
        // Write a object instance to data output stream
        //
        public override void TightMarshal2(OpenWireFormat wireFormat, Object o, BinaryWriter dataOut, BooleanStream bs)
        {
            base.TightMarshal2(wireFormat, o, dataOut, bs);

            MessageDispatchNotification info = (MessageDispatchNotification)o;
            TightMarshalCachedObject2(wireFormat, (DataStructure)info.ConsumerId, dataOut, bs);
            TightMarshalCachedObject2(wireFormat, (DataStructure)info.Destination, dataOut, bs);
            TightMarshalLong2(wireFormat, info.DeliverySequenceId, dataOut, bs);
            TightMarshalNestedObject2(wireFormat, (DataStructure)info.MessageId, dataOut, bs);
        }

        // 
        // Un-marshal an object instance from the data input stream
        // 
        public override void LooseUnmarshal(OpenWireFormat wireFormat, Object o, BinaryReader dataIn) 
        {
            base.LooseUnmarshal(wireFormat, o, dataIn);

            MessageDispatchNotification info = (MessageDispatchNotification)o;
            info.ConsumerId = (ConsumerId) LooseUnmarshalCachedObject(wireFormat, dataIn);
            info.Destination = (ActiveMQDestination) LooseUnmarshalCachedObject(wireFormat, dataIn);
            info.DeliverySequenceId = LooseUnmarshalLong(wireFormat, dataIn);
            info.MessageId = (MessageId) LooseUnmarshalNestedObject(wireFormat, dataIn);
        }

        // 
        // Write a object instance to data output stream
        //
        public override void LooseMarshal(OpenWireFormat wireFormat, Object o, BinaryWriter dataOut)
        {

            MessageDispatchNotification info = (MessageDispatchNotification)o;

            base.LooseMarshal(wireFormat, o, dataOut);
            LooseMarshalCachedObject(wireFormat, (DataStructure)info.ConsumerId, dataOut);
            LooseMarshalCachedObject(wireFormat, (DataStructure)info.Destination, dataOut);
            LooseMarshalLong(wireFormat, info.DeliverySequenceId, dataOut);
            LooseMarshalNestedObject(wireFormat, (DataStructure)info.MessageId, dataOut);
        }
    }
}
