using System;
using BlockVPN.Core;
using Xunit;

namespace BlockVPN.Tests
{
    public class FormatTests
    {
        [Theory]
        [InlineData(0UL, "0 Б")]
        [InlineData(512UL, "512 Б")]
        [InlineData(1024UL, "1 КиБ")]
        [InlineData(1536UL, "1.5 КиБ")]
        [InlineData(1048576UL, "1 МиБ")]
        [InlineData(1073741824UL, "1 ГиБ")]
        [InlineData(1099511627776UL, "1 ТиБ")]
        public void FormatBytes_Scales(ulong value, string expected)
        {
            Assert.Equal(expected, Format.FormatBytes(value));
        }

        [Fact]
        public void FormatDuration_Formats()
        {
            Assert.Equal("0:00:05", Format.FormatDuration(TimeSpan.FromSeconds(5)));
            Assert.Equal("0:03:25", Format.FormatDuration(TimeSpan.FromSeconds(205)));
            Assert.Equal("2:00:00", Format.FormatDuration(TimeSpan.FromHours(2)));
            Assert.Equal("27:30:00", Format.FormatDuration(TimeSpan.FromHours(27.5)));
        }

        [Fact]
        public void FormatDuration_Null_IsDash()
        {
            Assert.Equal("—", Format.FormatDuration(null));
        }

        [Fact]
        public void FormatHandshake_Null_IsDash()
        {
            Assert.Equal("—", Format.FormatHandshakeAgo(null));
        }

        [Fact]
        public void FormatHandshake_Recent_ShowsSeconds()
        {
            var result = Format.FormatHandshakeAgo(DateTime.UtcNow.AddSeconds(-10));
            Assert.Contains("назад", result);
        }
    }
}
