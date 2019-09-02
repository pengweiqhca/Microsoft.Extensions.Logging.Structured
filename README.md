Write Microsoft.Extensions.Logging to kafka.

How to use
=============

Install the [Nuget](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Structured.Kafka) package.

``` PS
Install-Package Microsoft.Extensions.Logging.Structured.Kafka
```
In your testing project, add the following framework

```cs
ILoggingBuilder lb = ....;
lb.AddKafka("localhost:19200", "test")
    .AddLayout("DateTime", new DateTimeOffsetLayout())
    //.AddLayout("XXX", new YYYLayout())
    .AddLayout("Exception", new ExceptionLayout());
```
