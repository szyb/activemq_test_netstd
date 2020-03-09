# activemq_test_netstd
This is POC example code for slow performance for large (1MB) message dequeuing from ActiveMQ by using Apache.NMS.ActiveMQ.NetStd (Version="1.7.2.4115") on linux OS.
The sample code was taken (and then slightly modified) from https://activemq.apache.org/components/nms/examples/nms-simple-asynchronous-consumer-example
The same code works perfect on Windows, even if conneted to linux ActiveMQ instance.

# tests
## windows output

```
About to connect to activemq:tcp://localhost:61616
Using destination: queue://FOO.BAR
Total miliseconds : 35
Received message with ID:   ID:windows-61832-637193565406991857-1:0:1:1:1
Received message with text: 52058488026704016514522615308243313364766510150440
Press enter to exit
```

## linux output

```
$ dotnet activemq_test_netstd.dll
About to connect to activemq:tcp://localhost:61616
Using destination: queue://FOO.BAR
Total miliseconds : 18978
Received message with ID:   ID:debian3-41651-637193557967467242-1:0:1:1:1
Received message with text: 00251146042861365658230272373018357254848456023734
```

## results
In windows environment queuing and dequing is pretty fast (35ms in the example above), but on linux this operation took ~19sec.
