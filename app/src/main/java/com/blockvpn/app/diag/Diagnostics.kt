package com.blockvpn.app.diag

import android.content.Context
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.net.InetAddress

object Diagnostics {

    suspend fun resolve(host: String): String? = withContext(Dispatchers.IO) {
        runCatching {
            InetAddress.getByName(host).hostAddress
        }.getOrNull()
    }

    fun vpnTransportActive(context: Context): Boolean {
        val cm = context.getSystemService(ConnectivityManager::class.java) ?: return false
        val network = cm.activeNetwork ?: return false
        val caps = cm.getNetworkCapabilities(network) ?: return false
        return caps.hasTransport(NetworkCapabilities.TRANSPORT_VPN)
    }

    fun extractDnsFromConfig(configText: String): String? {
        val re = Regex("(?im)^\\s*DNS\\s*=\\s*(.+)$")
        return re.find(configText)?.groupValues?.get(1)?.trim()?.split(",")?.firstOrNull()
    }

    fun extractEndpointFromConfig(configText: String): String? {
        val re = Regex("(?im)^\\s*Endpoint\\s*=\\s*(.+)$")
        return re.find(configText)?.groupValues?.get(1)?.trim()
    }
}
