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
using System.Threading;
using Apache.NMS;
using Apache.NMS.Util;
using Apache.NMS.Test;
using NUnit.Framework;
using NUnit.Framework.Extensions;

namespace Apache.NMS.Stomp.Test
{
    [TestFixture]
    public class AsyncConsumeTest : NMSTestSupport
    {
        protected static string DESTINATION_NAME = "AsyncConsumeDestination";
        protected static string TEST_CLIENT_ID = "AsyncConsumeClientId";
        protected static string RESPONSE_CLIENT_ID = "AsyncConsumeResponseClientId";
        protected AutoResetEvent semaphore;
        protected bool received;
        protected IMessage receivedMsg;
        protected int postfix;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            semaphore = new AutoResetEvent(false);
            received = false;
            receivedMsg = null;

            Random rand = new Random();
            this.postfix = rand.Next();
        }

        [TearDown]
        public override void TearDown()
        {
            receivedMsg = null;
            base.TearDown();
        }

        [RowTest]
        [Row(MsgDeliveryMode.Persistent)]
        [Row(MsgDeliveryMode.NonPersistent)]
        public void TestAsynchronousConsume(MsgDeliveryMode deliveryMode)
        {
            using(IConnection connection = CreateConnection(TEST_CLIENT_ID + ":" + this.postfix))
            {
                connection.Start();
                using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.CreateTemporaryQueue();
                    using(IMessageConsumer consumer = session.CreateConsumer(destination))
                    using(IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        producer.RequestTimeout = receiveTimeout;
                        consumer.Listener += new MessageListener(OnMessage);

                        IMessage request = session.CreateMessage();
                        request.NMSCorrelationID = "AsyncConsume";
                        request.NMSType = "Test";
                        producer.Send(request);

                        WaitForMessageToArrive();
                        Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
                    }
                }
            }
        }

        [RowTest]
        [Row(MsgDeliveryMode.Persistent)]
        [Row(MsgDeliveryMode.NonPersistent)]
        public void TestCreateConsumerAfterSend(MsgDeliveryMode deliveryMode)
        {
            using(IConnection connection = CreateConnection(TEST_CLIENT_ID + ":" + this.postfix))
            {
                connection.Start();
                using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.CreateTemporaryQueue();
                    using(IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        producer.RequestTimeout = receiveTimeout;

                        IMessage request = session.CreateMessage();
                        request.NMSCorrelationID = "AsyncConsumeAfterSend";
                        request.NMSType = "Test";
                        producer.Send(request);

                        using(IMessageConsumer consumer = session.CreateConsumer(destination))
                        {
                            consumer.Listener += new MessageListener(OnMessage);
                            WaitForMessageToArrive();
                            Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
                        }
                    }
                }
            }
        }

        [RowTest]
        [Row(MsgDeliveryMode.Persistent)]
        [Row(MsgDeliveryMode.NonPersistent)]
        public void TestCreateConsumerBeforeSendAddListenerAfterSend(MsgDeliveryMode deliveryMode)
        {
            using(IConnection connection = CreateConnection(TEST_CLIENT_ID + ":" + this.postfix))
            {
                connection.Start();
                using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.CreateTemporaryQueue();
                    using(IMessageConsumer consumer = session.CreateConsumer(destination))
                    using(IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        producer.RequestTimeout = receiveTimeout;

                        IMessage request = session.CreateMessage();
                        request.NMSCorrelationID = "AsyncConsumeAfterSendLateListener";
                        request.NMSType = "Test";
                        producer.Send(request);

                        // now lets add the listener
                        consumer.Listener += new MessageListener(OnMessage);
                        WaitForMessageToArrive();
                        Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
                    }
                }
            }
        }

        [RowTest]
        [Row(MsgDeliveryMode.Persistent)]
        [Row(MsgDeliveryMode.NonPersistent)]
        public void TestAsynchronousTextMessageConsume(MsgDeliveryMode deliveryMode)
        {
            using(IConnection connection = CreateConnection(TEST_CLIENT_ID + ":" + this.postfix))
            {
                connection.Start();
                using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.CreateTemporaryQueue();
                    using(IMessageConsumer consumer = session.CreateConsumer(destination))
                    {
                        consumer.Listener += new MessageListener(OnMessage);
                        using(IMessageProducer producer = session.CreateProducer(destination))
                        {
                            producer.DeliveryMode = deliveryMode;
                            producer.RequestTimeout = receiveTimeout;

                            ITextMessage request = session.CreateTextMessage("Hello, World!");
                            request.NMSCorrelationID = "AsyncConsumeTextMessage";
                            request.Properties["NMSXGroupID"] = "cheese";
                            request.Properties["myHeader"] = "James";

                            producer.Send(request);

                            WaitForMessageToArrive();
                            Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
                            Assert.AreEqual(request.Properties["NMSXGroupID"], receivedMsg.Properties["NMSXGroupID"], "Invalid NMSXGroupID.");
                            Assert.AreEqual(request.Properties["myHeader"], receivedMsg.Properties["myHeader"], "Invalid myHeader.");
                            Assert.AreEqual(request.Text, ((ITextMessage) receivedMsg).Text, "Invalid text body.");
                        }
                    }
                }
            }
        }

        [RowTest]
        [Row(MsgDeliveryMode.Persistent)]
        [Row(MsgDeliveryMode.NonPersistent)]
        public void TestTemporaryQueueAsynchronousConsume(MsgDeliveryMode deliveryMode)
        {
            using(IConnection connection = CreateConnection(TEST_CLIENT_ID + ":" + this.postfix))
            {
                connection.Start();
                using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.CreateTemporaryQueue();
                    ITemporaryQueue tempReplyDestination = session.CreateTemporaryQueue();

                    using(IMessageConsumer consumer = session.CreateConsumer(destination))
                    using(IMessageConsumer tempConsumer = session.CreateConsumer(tempReplyDestination))
                    using(IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        producer.RequestTimeout = receiveTimeout;
                        tempConsumer.Listener += new MessageListener(OnMessage);
                        consumer.Listener += new MessageListener(OnQueueMessage);

                        IMessage request = session.CreateMessage();
                        request.NMSCorrelationID = "TemqQueueAsyncConsume";
                        request.NMSType = "Test";
                        request.NMSReplyTo = tempReplyDestination;
                        producer.Send(request);

                        WaitForMessageToArrive();
                        Assert.AreEqual("TempQueueAsyncResponse", receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
                    }
                }
            }
        }

        protected void OnQueueMessage(IMessage message)
        {
            Assert.AreEqual("TemqQueueAsyncConsume", message.NMSCorrelationID, "Invalid correlation ID.");
            using(IConnection connection = CreateConnection(RESPONSE_CLIENT_ID + ":" + this.postfix))
            {
                connection.Start();
                using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    using(IMessageProducer producer = session.CreateProducer(message.NMSReplyTo))
                    {
                        producer.DeliveryMode = message.NMSDeliveryMode;
                        producer.RequestTimeout = receiveTimeout;

                        ITextMessage response = session.CreateTextMessage("Asynchronous Response Message Text");
                        response.NMSCorrelationID = "TempQueueAsyncResponse";
                        response.NMSType = message.NMSType;
                        producer.Send(response);
                    }
                }
            }
        }

        protected void OnMessage(IMessage message)
        {
            receivedMsg = message;
            received = true;
            semaphore.Set();
        }

        protected void WaitForMessageToArrive()
        {
            semaphore.WaitOne((int) receiveTimeout.TotalMilliseconds, true);
            Assert.IsTrue(received, "Should have received a message by now!");
        }
    }
}
