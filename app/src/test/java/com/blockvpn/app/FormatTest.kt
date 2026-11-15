package com.blockvpn.app

import com.blockvpn.app.util.formatBytes
import com.blockvpn.app.util.formatDuration
import org.junit.Assert.assertEquals
import org.junit.Test

class FormatTest {

    @Test
    fun `bytes under 1KB stay raw`() {
        assertEquals("512 Б", formatBytes(512))
    }

    @Test
    fun `kilobytes are formatted`() {
        assertEquals("1.5 КБ", formatBytes(1536))
    }

    @Test
    fun `megabytes are formatted`() {
        assertEquals("2.0 МБ", formatBytes(2L * 1024 * 1024))
    }

    @Test
    fun `gigabytes are formatted`() {
        assertEquals("1.00 ГБ", formatBytes(1L shl 30))
    }

    @Test
    fun `zero bytes`() {
        assertEquals("0 Б", formatBytes(0))
    }

    @Test
    fun `duration under hour`() {
        assertEquals("05:09", formatDuration(309))
    }

    @Test
    fun `duration over hour`() {
        assertEquals("01:02:03", formatDuration(3723))
    }

    @Test
    fun `zero duration`() {
        assertEquals("00:00", formatDuration(0))
    }
}
