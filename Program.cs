﻿/* 
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
using System.Diagnostics;
using System.Text;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Transport.Tcp;
using Apache.NMS.Util;

namespace Apache.NMS.ActiveMQ.Test
{
  public class TestMain
  {
    protected static AutoResetEvent semaphore = new AutoResetEvent(false);
    protected static ITextMessage message = null;
    protected static TimeSpan receiveTimeout = TimeSpan.FromSeconds(60);
    protected static Stopwatch stopwatch = new Stopwatch();

    public static void Main(string[] args)
    {
      // Example connection strings:
      //    activemq:tcp://activemqhost:61616
      //    stomp:tcp://activemqhost:61613
      //    ems:tcp://tibcohost:7222
      //    msmq://localhost

      Uri connecturi = new Uri("activemq:tcp://localhost:61616");

      Console.WriteLine("About to connect to " + connecturi);
      TcpTransportManager.ReceiveBufferSize = 1024 * 64;
      TcpTransportManager.SendBufferSize = 1024 * 8;
      // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
      IConnectionFactory factory = new NMSConnectionFactory(connecturi);

      using (IConnection connection = factory.CreateConnection("test", "test"))
      using (ISession session = connection.CreateSession())
      {
        // Examples for getting a destination:
        //
        // Hard coded destinations:
        //    IDestination destination = session.GetQueue("FOO.BAR");
        //    Debug.Assert(destination is IQueue);
        //    IDestination destination = session.GetTopic("FOO.BAR");
        //    Debug.Assert(destination is ITopic);
        //
        // Embedded destination type in the name:
        //    IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
        //    Debug.Assert(destination is IQueue);
        //    IDestination destination = SessionUtil.GetDestination(session, "topic://FOO.BAR");
        //    Debug.Assert(destination is ITopic);
        //
        // Defaults to queue if type is not specified:
        //    IDestination destination = SessionUtil.GetDestination(session, "FOO.BAR");
        //    Debug.Assert(destination is IQueue);
        //
        // .NET 3.5 Supports Extension methods for a simplified syntax:
        //    IDestination destination = session.GetDestination("queue://FOO.BAR");
        //    Debug.Assert(destination is IQueue);
        //    IDestination destination = session.GetDestination("topic://FOO.BAR");
        //    Debug.Assert(destination is ITopic);
        IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");

        Console.WriteLine("Using destination: " + destination);

        // Create a consumer and producer
        using (IMessageConsumer consumer = session.CreateConsumer(destination))
        using (IMessageProducer producer = session.CreateProducer(destination))
        {
          // Start the connection so that messages will be processed.
          connection.Start();
          producer.DeliveryMode = MsgDeliveryMode.Persistent;
          producer.RequestTimeout = receiveTimeout;

          consumer.Listener += new MessageListener(OnMessage);

          // Send a message
          ITextMessage request = session.CreateTextMessage(GetLargeMessage());
          request.NMSCorrelationID = "abc";
          request.Properties["NMSXGroupID"] = "cheese";
          request.Properties["myHeader"] = "Cheddar";

          stopwatch.Start();
          producer.Send(request);


          // Wait for the message
          semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);

          if (message == null)
          {
            Console.WriteLine("No message received!");
          }
          else
          {
            Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
            Console.WriteLine("Received message with text: " + message.Text.Substring(0, 50));
          }
          Console.WriteLine("Press enter to exit");
          Console.ReadLine();
        }
      }
    }

    private static string GetLargeMessage()
    {
      StringBuilder sb = new StringBuilder();
      Random rnd = new Random();
      for (int i = 0; i <= 10 * 1024 * 1024; i++)
        sb.Append(rnd.Next(0, 9).ToString());
      return sb.ToString();
    }

    protected static void OnMessage(IMessage receivedMsg)
    {
      stopwatch.Stop();
      Console.WriteLine($"Total miliseconds : {stopwatch.ElapsedMilliseconds}");

      message = receivedMsg as ITextMessage;
      semaphore.Set();
    }
  }
}