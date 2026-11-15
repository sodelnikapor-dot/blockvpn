package com.blockvpn.app

import com.blockvpn.app.diag.Diagnostics
import kotlinx.coroutines.test.runTest
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test

class DiagnosticsTest {

    private val sampleConfig = """
        [Interface]
        PrivateKey = ABCDEF123456=
        Address = 10.66.66.2/32
        DNS = 1.1.1.1

        [Peer]
        PublicKey = FEDCBA654321=
        Endpoint = 203.0.113.7:51820
        AllowedIPs = 0.0.0.0/0, ::/0
    """.trimIndent()

    @Test
    fun `dns extracted from interface section`() {
        assertEquals("1.1.1.1", Diagnostics.extractDnsFromConfig(sampleConfig))
    }

    @Test
    fun `dns missing returns null`() {
        val noDns = sampleConfig.replace("DNS = 1.1.1.1\n", "")
        assertNull(Diagnostics.extractDnsFromConfig(noDns))
    }

    @Test
    fun `endpoint extracted`() {
        assertEquals("203.0.113.7:51820", Diagnostics.extractEndpointFromConfig(sampleConfig))
    }

    @Test
    fun `endpoint missing returns null`() {
        assertNull(Diagnostics.extractEndpointFromConfig("[Interface]\nPrivateKey = x="))
    }

    @Test
    fun `multiple dns entries take first`() {
        val cfg = sampleConfig.replace("DNS = 1.1.1.1", "DNS = 1.1.1.1, 8.8.8.8")
        assertEquals("1.1.1.1", Diagnostics.extractDnsFromConfig(cfg))
    }

    @Test
    fun `resolve returns address or null without throwing`() = runTest {
        val result = Diagnostics.resolve("localhost")
        // в песочнице DNS может отсутствовать: допустимы оба исхода, но не исключение
        assertTrue(result == null || result!!.isNotEmpty())
    }
}
