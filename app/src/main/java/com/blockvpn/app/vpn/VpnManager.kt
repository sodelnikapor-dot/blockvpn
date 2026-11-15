package com.blockvpn.app.vpn

import android.content.Context
import android.os.SystemClock
import com.wireguard.android.backend.GoBackend
import com.wireguard.android.backend.Tunnel
import com.wireguard.config.Config
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.BufferedReader
import java.io.StringReader

class VpnManager(private val context: Context) {

    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.Main)
    private val backend by lazy { GoBackend(context) }

    private val _state = MutableStateFlow(VpnUiState.DISCONNECTED)
    val state: StateFlow<VpnUiState> = _state

    private val _lastError = MutableStateFlow<String?>(null)
    val lastError: StateFlow<String?> = _lastError

    private var tunnel: WgTunnel? = null
    private var desiredUp = false
    private var watcher: Job? = null
    private var connectedSince = 0L

    private inner class WgTunnel(private val profileId: String) : Tunnel {
        override fun getName(): String = "blockvpn-$profileId"
        override fun onStateChange(newState: Tunnel.State) = Unit
    }

    fun parseConfig(text: String): Config? = try {
        Config.parse(BufferedReader(StringReader(text)))
    } catch (e: Exception) {
        null
    }

    suspend fun connect(profile: VpnProfile): Boolean {
        if (desiredUp) return true
        val config = parseConfig(profile.configText)
        if (config == null) {
            _lastError.value = "Конфиг повреждён: проверь секции [Interface] и [Peer]"
            _state.value = VpnUiState.ERROR
            return false
        }
        desiredUp = true
        _lastError.value = null
        _state.value = VpnUiState.CONNECTING
        var attempt = 0
        while (desiredUp && attempt < MAX_ATTEMPTS) {
            try {
                val t = WgTunnel(profile.id)
                tunnel = t
                withContext(Dispatchers.IO) {
                    backend.setState(t, Tunnel.State.UP, config)
                }
                _state.value = VpnUiState.SECURED
                connectedSince = SystemClock.elapsedRealtime()
                startWatcher(profile)
                return true
            } catch (e: Exception) {
                attempt++
                _lastError.value = friendlyError(e)
                if (attempt < MAX_ATTEMPTS && desiredUp) {
                    _state.value = VpnUiState.RECONNECTING
                    delay(RETRY_DELAY_MS * attempt)
                }
            }
        }
        if (desiredUp) {
            desiredUp = false
            _state.value = VpnUiState.ERROR
        }
        return false
    }

    suspend fun disconnect() {
        desiredUp = false
        watcher?.cancel()
        watcher = null
        val t = tunnel
        if (t != null) {
            runCatching {
                withContext(Dispatchers.IO) { backend.setState(t, Tunnel.State.DOWN, null) }
            }
        }
        tunnel = null
        connectedSince = 0
        _state.value = VpnUiState.DISCONNECTED
    }

    private fun startWatcher(profile: VpnProfile) {
        watcher?.cancel()
        watcher = scope.launch {
            var misses = 0
            while (desiredUp) {
                delay(WATCH_INTERVAL_MS)
                val t = tunnel ?: break
                val current = runCatching {
                    withContext(Dispatchers.IO) { backend.getState(t) }
                }.getOrNull()
                if (current != Tunnel.State.UP) {
                    misses++
                    if (_state.value == VpnUiState.SECURED) {
                        _state.value = VpnUiState.RECONNECTING
                    }
                    if (misses >= 2) {
                        val config = parseConfig(profile.configText)
                        if (config != null) {
                            runCatching {
                                withContext(Dispatchers.IO) { backend.setState(t, Tunnel.State.UP, config) }
                            }
                        }
                        misses = 0
                    }
                } else {
                    if (_state.value != VpnUiState.SECURED) {
                        _state.value = VpnUiState.SECURED
                        connectedSince = SystemClock.elapsedRealtime()
                    }
                    misses = 0
                }
            }
        }
    }

    fun stats(): Pair<Long, Long>? {
        val t = tunnel ?: return null
        if (_state.value != VpnUiState.SECURED) return null
        val statistics = runCatching { backend.getStatistics(t) }.getOrNull() ?: return null
        return runCatching { statistics.totalRx() to statistics.totalTx() }.getOrNull()
    }

    fun lastHandshakeSecondsAgo(): Long? {
        val t = tunnel ?: return null
        if (_state.value != VpnUiState.SECURED) return null
        return runCatching {
            val statistics = backend.getStatistics(t)
            val key = statistics.peers().firstOrNull() ?: return null
            val stats = statistics.peer(key) ?: return null
            val elapsed = (System.currentTimeMillis() - stats.latestHandshakeEpochMillis) / 1000
            if (elapsed in 0..600) elapsed else null
        }.getOrNull()
    }

    fun connectedSeconds(): Long {
        if (_state.value != VpnUiState.SECURED || connectedSince == 0L) return 0
        return (SystemClock.elapsedRealtime() - connectedSince) / 1000
    }

    private fun friendlyError(e: Exception): String {
        val msg = e.message ?: return "Ошибка подключения"
        return when {
            msg.contains("PERMISSION", ignoreCase = true) ->
                "Нет разрешения на VPN. Разреши подключение в системном диалоге."
            msg.contains("UNSPECIFIED", ignoreCase = true) ->
                "Сервер недоступен. Проверь интернет и Endpoint в конфиге."
            else -> "Ошибка подключения: $msg"
        }
    }

    private companion object {
        const val MAX_ATTEMPTS = 5
        const val RETRY_DELAY_MS = 2000L
        const val WATCH_INTERVAL_MS = 3000L
    }
}
