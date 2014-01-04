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
using Apache.NMS.Test;
using Apache.NMS.MQTT;
using NUnit.Framework;

namespace Apache.NMS.MQTT.Test
{
    [TestFixture]
    public class MQTTSessionTest : NMSTestSupport
    {
        private IConnection connection;

        [SetUp]
        public void SetUp()
        {
            Apache.NMS.Tracer.Trace = new NmsConsoleTracer();
            base.SetUp();
            connection = CreateConnection();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            if (connection != null)
            {
                connection.Close();
            }
        }

        [Test]
        public void TestCanCreateAutoAckSession()
        {
            ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            Assert.AreEqual(AcknowledgementMode.AutoAcknowledge, session.AcknowledgementMode);
        }

        [Test]
        public void TestCanCreateClientAckSession()
        {
            ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
            Assert.AreEqual(AcknowledgementMode.ClientAcknowledge, session.AcknowledgementMode);
        }

        [Test]
        [TestCase(ExpectedException = typeof(NotSupportedException))]
        public void TestCannotCreateTransactedAckSession()
        {
            connection.CreateSession(AcknowledgementMode.Transactional);
        }
    }
}
