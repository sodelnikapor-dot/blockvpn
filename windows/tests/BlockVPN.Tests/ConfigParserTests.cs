using System;
using System.Collections.Generic;
using BlockVPN.Core;
using Xunit;

namespace BlockVPN.Tests
{
    public class ConfigParserTests
    {
        private const string ValidConfig =
@"[Interface]
PrivateKey = yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk=
Address = 10.66.66.2/32, fd42:42:42::2/128
DNS = 1.1.1.1, 1.0.0.1

[Peer]
PublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=
Endpoint = vpn.example.com:51820
AllowedIPs = 0.0.0.0/0, ::/0
PersistentKeepalive = 25";

        [Fact]
        public void ValidConfig_Parses()
        {
            var cfg = WgConfigParser.Parse(ValidConfig);
            Assert.NotNull(cfg.PrivateKey);
            Assert.Equal(2, cfg.Dns.Count);
            Assert.Single(cfg.Peers);
            Assert.Equal("vpn.example.com:51820", cfg.Peers[0].Endpoint);
            Assert.Equal(25, cfg.Peers[0].PersistentKeepalive);
        }

        [Fact]
        public void ValidConfig_HasNoValidationErrors()
        {
            Assert.Empty(WgConfigParser.Validate(ValidConfig));
        }

        [Fact]
        public void EmptyConfig_Fails()
        {
            Assert.NotEmpty(WgConfigParser.Validate(""));
            Assert.NotEmpty(WgConfigParser.Validate("   \n  "));
        }

        [Fact]
        public void MissingInterfaceSection_Fails()
        {
            var errors = WgConfigParser.Validate("[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = vpn.example.com:51820\nAllowedIPs = 0.0.0.0/0\n");
            Assert.Contains(errors, e => e.Contains("[Interface]"));
        }

        [Fact]
        public void MissingPrivateKey_Fails()
        {
            var errors = WgConfigParser.Validate("[Interface]\nAddress = 10.0.0.2/32\n\n[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = vpn.example.com:51820\nAllowedIPs = 0.0.0.0/0\n");
            Assert.Contains(errors, e => e.Contains("PrivateKey"));
        }

        [Fact]
        public void BadKey_Fails()
        {
            var errors = WgConfigParser.Validate("[Interface]\nPrivateKey = not-base64!!\n\n[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = vpn.example.com:51820\nAllowedIPs = 0.0.0.0/0\n");
            Assert.Contains(errors, e => e.Contains("PrivateKey"));
        }

        [Fact]
        public void BadEndpoint_Fails()
        {
            var errors = WgConfigParser.Validate("[Interface]\nPrivateKey = yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk=\n\n[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = vpn.example.com\nAllowedIPs = 0.0.0.0/0\n");
            Assert.Contains(errors, e => e.Contains("Endpoint"));
        }

        [Fact]
        public void Ipv6Endpoint_InBrackets_IsValid()
        {
            var errors = WgConfigParser.Validate("[Interface]\nPrivateKey = yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk=\n\n[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = [2001:db8::1]:51820\nAllowedIPs = 0.0.0.0/0\n");
            Assert.Empty(errors);
        }

        [Fact]
        public void BadAllowedIp_Fails()
        {
            var errors = WgConfigParser.Validate("[Interface]\nPrivateKey = yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk=\n\n[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = vpn.example.com:51820\nAllowedIPs = not-an-ip/0\n");
            Assert.Contains(errors, e => e.Contains("AllowedIPs"));
        }

        [Fact]
        public void BadPrefix_Fails()
        {
            var errors = WgConfigParser.Validate("[Interface]\nPrivateKey = yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk=\nAddress = 10.0.0.2/99\n\n[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = vpn.example.com:51820\nAllowedIPs = 0.0.0.0/0\n");
            Assert.Contains(errors, e => e.Contains("Address"));
        }

        [Fact]
        public void MissingPeerSection_Fails()
        {
            var errors = WgConfigParser.Validate("[Interface]\nPrivateKey = yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk=\nAddress = 10.0.0.2/32\n");
            Assert.Contains(errors, e => e.Contains("[Peer]"));
        }

        [Fact]
        public void CommentsAndWhitespace_AreTolerated()
        {
            var cfg = WgConfigParser.Parse("# comment\n\r\n[Interface]  \n PrivateKey = yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk= \n\n[Peer]\nPublicKey = Fp0v0M7sTfF9f9cJzX8mQwZ6bN4hK2pR7tU1yE3oX5c=\nEndpoint = vpn.example.com:51820\nAllowedIPs=0.0.0.0/0\n");
            Assert.Single(cfg.Peers);
        }

        [Fact]
        public void ExtractDns_ReturnsJoinedList()
        {
            Assert.Equal("1.1.1.1, 1.0.0.1", WgConfigParser.ExtractDns(ValidConfig));
        }

        [Fact]
        public void ExtractEndpoint_ReturnsEndpoint()
        {
            Assert.Equal("vpn.example.com:51820", WgConfigParser.ExtractEndpoint(ValidConfig));
        }

        [Fact]
        public void ExtractEndpoint_InvalidConfig_ReturnsNull()
        {
            Assert.Null(WgConfigParser.ExtractEndpoint("garbage"));
        }

        [Fact]
        public void IsValidKey_ChecksLength()
        {
            Assert.True(WgConfigParser.IsValidKey("yLd7KhKnP6WxT9Zm0uPZQ1cH3gJz1vXkO2rF5tY8nWk="));
            Assert.False(WgConfigParser.IsValidKey("short"));
            Assert.False(WgConfigParser.IsValidKey(null));
            Assert.False(WgConfigParser.IsValidKey(""));
        }
    }
}
