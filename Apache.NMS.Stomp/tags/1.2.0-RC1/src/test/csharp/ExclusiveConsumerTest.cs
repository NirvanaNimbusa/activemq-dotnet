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
using Apache.NMS.Test;
using Apache.NMS.Stomp;
using Apache.NMS.Stomp.Commands;
using NUnit.Framework;
using NUnit.Framework.Extensions;

namespace Apache.NMS.Stomp.Test
{
    [TestFixture]
    public class ExclusiveConsumerTest : NMSTestSupport
    {
        protected static string DESTINATION_NAME = "TestDestination";
        protected static string TEST_CLIENT_ID = "ExclusiveConsumerTestClientId";

        private IConnection createConnection(bool start)
        {
            IConnection conn = CreateConnection(TEST_CLIENT_ID + ":" + new Random().Next());
            if(start)
            {
                conn.Start();
            }

            return conn;
        }

        [Test]
        public void TestExclusiveConsumerSelectedCreatedFirst()
        {
            IConnection conn = createConnection(true);

            ISession exclusiveSession = null;
            ISession fallbackSession = null;
            ISession senderSession = null;

            try
            {
                exclusiveSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                fallbackSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                senderSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);

                IQueue exclusiveQueue = exclusiveSession.GetQueue("TEST.QUEUE1?consumer.exclusive=true");
                IMessageConsumer exclusiveConsumer = exclusiveSession.CreateConsumer(exclusiveQueue);

                IQueue fallbackQueue = fallbackSession.GetQueue("TEST.QUEUE1");
                IMessageConsumer fallbackConsumer = fallbackSession.CreateConsumer(fallbackQueue);

                IQueue senderQueue = senderSession.GetQueue("TEST.QUEUE1");

                IMessageProducer producer = senderSession.CreateProducer(senderQueue);

                IMessage msg = senderSession.CreateTextMessage("test");
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                producer.Send(msg);
                Thread.Sleep(500);

                // Verify exclusive consumer receives the message.
                Assert.IsNotNull(exclusiveConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
            }
            finally
            {
                fallbackSession.Close();
                senderSession.Close();
                conn.Close();
            }
        }

        [Test]
        public void TestExclusiveConsumerSelectedCreatedAfter()
        {
            IConnection conn = createConnection(true);

            ISession exclusiveSession = null;
            ISession fallbackSession = null;
            ISession senderSession = null;

            try
            {
                exclusiveSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                fallbackSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                senderSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);

                IQueue fallbackQueue = fallbackSession.GetQueue("TEST.QUEUE5");
                IMessageConsumer fallbackConsumer = fallbackSession.CreateConsumer(fallbackQueue);

                IQueue exclusiveQueue = exclusiveSession.GetQueue("TEST.QUEUE5?consumer.exclusive=true");
                IMessageConsumer exclusiveConsumer = exclusiveSession.CreateConsumer(exclusiveQueue);

                IQueue senderQueue = senderSession.GetQueue("TEST.QUEUE5");

                IMessageProducer producer = senderSession.CreateProducer(senderQueue);

                IMessage msg = senderSession.CreateTextMessage("test");
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                producer.Send(msg);
                Thread.Sleep(500);

                // Verify exclusive consumer receives the message.
                Assert.IsNotNull(exclusiveConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
            }
            finally
            {
                fallbackSession.Close();
                senderSession.Close();
                conn.Close();
            }
        }

        [Test]
        public void TestFailoverToAnotherExclusiveConsumerCreatedFirst()
        {
            IConnection conn = createConnection(true);

            ISession exclusiveSession1 = null;
            ISession exclusiveSession2 = null;
            ISession fallbackSession = null;
            ISession senderSession = null;

            try
            {
                exclusiveSession1 = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                exclusiveSession2 = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                fallbackSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                senderSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);

                // This creates the exclusive consumer first which avoids AMQ-1024
                // bug.
                IQueue exclusiveQueue = exclusiveSession1.GetQueue("TEST.QUEUE2?consumer.exclusive=true");

                IMessageConsumer exclusiveConsumer1 = exclusiveSession1.CreateConsumer(exclusiveQueue);
                IMessageConsumer exclusiveConsumer2 = exclusiveSession2.CreateConsumer(exclusiveQueue);

                IQueue fallbackQueue = fallbackSession.GetQueue("TEST.QUEUE2");
                IMessageConsumer fallbackConsumer = fallbackSession.CreateConsumer(fallbackQueue);

                IQueue senderQueue = senderSession.GetQueue("TEST.QUEUE2");

                IMessageProducer producer = senderSession.CreateProducer(senderQueue);
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                IMessage msg = senderSession.CreateTextMessage("test");
                producer.Send(msg);
                Thread.Sleep(500);

                // Verify exclusive consumer receives the message.
                Assert.IsNotNull(exclusiveConsumer1.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(exclusiveConsumer2.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));

                // Close the exclusive consumer to verify the non-exclusive consumer
                // takes over
                exclusiveConsumer1.Close();

                producer.Send(msg);
                producer.Send(msg);

                Assert.IsNotNull(exclusiveConsumer2.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
            }
            finally
            {
                fallbackSession.Close();
                senderSession.Close();
                conn.Close();
            }
        }

        [Test]
        public void TestFailoverToAnotherExclusiveConsumerCreatedAfter()
        {
            IConnection conn = createConnection(true);

            ISession exclusiveSession1 = null;
            ISession exclusiveSession2 = null;
            ISession fallbackSession = null;
            ISession senderSession = null;

            try
            {
                exclusiveSession1 = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                exclusiveSession2 = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                fallbackSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                senderSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);

                // This creates the exclusive consumer first which avoids AMQ-1024
                // bug.
                IQueue exclusiveQueue = exclusiveSession1.GetQueue("TEST.QUEUE6?consumer.exclusive=true");
                IMessageConsumer exclusiveConsumer1 = exclusiveSession1.CreateConsumer(exclusiveQueue);

                IQueue fallbackQueue = fallbackSession.GetQueue("TEST.QUEUE6");
                IMessageConsumer fallbackConsumer = fallbackSession.CreateConsumer(fallbackQueue);

                IMessageConsumer exclusiveConsumer2 = exclusiveSession2.CreateConsumer(exclusiveQueue);

                IQueue senderQueue = senderSession.GetQueue("TEST.QUEUE6");

                IMessageProducer producer = senderSession.CreateProducer(senderQueue);
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                IMessage msg = senderSession.CreateTextMessage("test");
                producer.Send(msg);
                Thread.Sleep(500);

                // Verify exclusive consumer receives the message.
                Assert.IsNotNull(exclusiveConsumer1.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(exclusiveConsumer2.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));

                // Close the exclusive consumer to verify the non-exclusive consumer
                // takes over
                exclusiveConsumer1.Close();

                producer.Send(msg);
                producer.Send(msg);

                Assert.IsNotNull(exclusiveConsumer2.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
            }
            finally
            {
                fallbackSession.Close();
                senderSession.Close();
                conn.Close();
            }
        }

        [Test]
        public void TestFailoverToNonExclusiveConsumer()
        {
            IConnection conn = createConnection(true);

            ISession exclusiveSession = null;
            ISession fallbackSession = null;
            ISession senderSession = null;

            try
            {
                exclusiveSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                fallbackSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                senderSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);

                // This creates the exclusive consumer first which avoids AMQ-1024
                // bug.
                IQueue exclusiveQueue = exclusiveSession.GetQueue("TEST.QUEUE3?consumer.exclusive=true");
                IMessageConsumer exclusiveConsumer = exclusiveSession.CreateConsumer(exclusiveQueue);

                IQueue fallbackQueue = fallbackSession.GetQueue("TEST.QUEUE3");
                IMessageConsumer fallbackConsumer = fallbackSession.CreateConsumer(fallbackQueue);

                IQueue senderQueue = senderSession.GetQueue("TEST.QUEUE3");

                IMessageProducer producer = senderSession.CreateProducer(senderQueue);
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                IMessage msg = senderSession.CreateTextMessage("test");
                producer.Send(msg);
                Thread.Sleep(500);

                // Verify exclusive consumer receives the message.
                Assert.IsNotNull(exclusiveConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));

                // Close the exclusive consumer to verify the non-exclusive consumer
                // takes over
                exclusiveConsumer.Close();

                producer.Send(msg);

                Assert.IsNotNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));

            }
            finally
            {
                fallbackSession.Close();
                senderSession.Close();
                conn.Close();
            }
        }

        [Test]
        public void TestFallbackToExclusiveConsumer()
        {
            IConnection conn = createConnection(true);

            ISession exclusiveSession = null;
            ISession fallbackSession = null;
            ISession senderSession = null;
            
            try
            {
                exclusiveSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                fallbackSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);
                senderSession = conn.CreateSession(AcknowledgementMode.AutoAcknowledge);

                // This creates the exclusive consumer first which avoids AMQ-1024
                // bug.
                IQueue exclusiveQueue = exclusiveSession.GetQueue("TEST.QUEUE4?consumer.exclusive=true");
                IMessageConsumer exclusiveConsumer = exclusiveSession.CreateConsumer(exclusiveQueue);

                IQueue fallbackQueue = fallbackSession.GetQueue("TEST.QUEUE4");
                IMessageConsumer fallbackConsumer = fallbackSession.CreateConsumer(fallbackQueue);

                IQueue senderQueue = senderSession.GetQueue("TEST.QUEUE4");

                IMessageProducer producer = senderSession.CreateProducer(senderQueue);
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                IMessage msg = senderSession.CreateTextMessage("test");
                producer.Send(msg);
                Thread.Sleep(500);

                // Verify exclusive consumer receives the message.
                Assert.IsNotNull(exclusiveConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));

                // Close the exclusive consumer to verify the non-exclusive consumer
                // takes over
                exclusiveConsumer.Close();

                producer.Send(msg);

                // Verify other non-exclusive consumer receices the message.
                Assert.IsNotNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));

                // Create exclusive consumer to determine if it will start receiving
                // the messages.
                exclusiveConsumer = exclusiveSession.CreateConsumer(exclusiveQueue);

                producer.Send(msg);
                Assert.IsNotNull(exclusiveConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
                Assert.IsNull(fallbackConsumer.Receive(TimeSpan.FromMilliseconds(1000)));
            }
            finally
            {
                fallbackSession.Close();
                senderSession.Close();
                conn.Close();
            }
        }

    }
}
