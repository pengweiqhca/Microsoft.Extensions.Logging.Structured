using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests
{
    public class StructuredLoggingOptionsTest
    {
        [Fact, Obsolete]
        public void Layout()
        {
            var options = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Logging:Test:Layout:A", typeof(DateTimeLayout).FullName!},
                    {"Logging:Test:Layout:B", "Abc"},
                })
                .Build().GetSection("Logging:Test")
                .Get<StructuredLoggingOptions>();

            Assert.NotNull(options);
            Assert.Equal(options.Layout.Count, options.Layouts.Count);
            Assert.True(options.Layouts.TryGetValue("A", out var layout));
            Assert.IsType<DateTimeLayout>(layout);
            Assert.True(options.Layouts.TryGetValue("B", out layout));
            Assert.IsType<ConstLayout>(layout);
            Assert.Equal("Abc", ((ConstLayout)layout!).Format(default));
        }
    }
}
