/**
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
package org.apache.activemq.nms.openwire.tool.commands;

import java.util.HashSet;
import java.util.Set;

/**
 * Provides methods to get a Source file and Header file Code generator given a Class
 * name.
 *
 * @since 2.0
 */
public class CommandCodeGeneratorsFactory {

    private Set<String> commandsWithShortcuts;

    /*
     * Here we store all Commands that need to have a isXXX method generated
     * such as isMessage.  We then check in the <code>checkNeedsShortcut</code>
     * method and if the Command being generated is in this list we create a
     * method call to override the virtual method in the base Command interface.
     */
    {
        commandsWithShortcuts = new HashSet<String>();
        commandsWithShortcuts.add( "Response" );
        commandsWithShortcuts.add( "RemoveInfo" );
        commandsWithShortcuts.add( "MessageDispatch" );
        commandsWithShortcuts.add( "BrokerInfo" );
        commandsWithShortcuts.add( "KeepAliveInfo" );
        commandsWithShortcuts.add( "WireFormatInfo" );
        commandsWithShortcuts.add( "Message" );
        commandsWithShortcuts.add( "MessageAck" );
        commandsWithShortcuts.add( "ProducerAck" );
        commandsWithShortcuts.add( "ProducerInfo" );
        commandsWithShortcuts.add( "MessageDispatchNotification" );
        commandsWithShortcuts.add( "ShutdownInfo" );
        commandsWithShortcuts.add( "TransactionInfo" );
        commandsWithShortcuts.add( "ConnectionInfo" );
        commandsWithShortcuts.add( "ConsumerInfo" );
        commandsWithShortcuts.add( "RemoveSubscriptionInfo" );

    }

    /**
     * Given a class name return an instance of a CSharp Class File Generator
     * that can generate the file for the Class.
     *
     * @param className - name of the class to find the generator for
     *
     * @return a new Header File code generator.
     */
    public CommandCodeGenerator getCodeGenerator( String className ) {

        CommandCodeGenerator generator = null;
//        if( className.equals("Message") ) {
//            generator = new MessageHeaderGenerator();
//        } else if( className.equals("ConnectionId") ) {
//            generator = new ConnectionIdHeaderGenerator();
//        } else if( className.equals("ConsumerId") ) {
//            generator = new ConsumerIdHeaderGenerator();
//        } else if( className.equals("ProducerId") ) {
//            generator = new ProducerIdHeaderGenerator();
//        } else if( className.equals("SessionId") ) {
//            generator = new SessionIdHeaderGenerator();
//        } else if( className.equals("SessionInfo") ) {
//            generator = new SessionInfoHeaderGenerator();
//        } else {
            generator = new CommandClassGenerator();
//        }

        if( className.endsWith("Id") ) {
            generator.setComparable( true );
            generator.setAssignable( true );
        }

        if( this.commandsWithShortcuts.contains( className ) ) {
            generator.setGenIsClass( true );
        }

        return generator;
    }

}
