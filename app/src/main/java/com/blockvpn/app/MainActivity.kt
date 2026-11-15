package com.blockvpn.app

import android.app.Activity
import android.net.VpnService
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.lifecycle.lifecycleScope
import com.blockvpn.app.prefs.UserPrefs
import com.blockvpn.app.ui.screens.AllStickers
import com.blockvpn.app.ui.screens.HelpScreen
import com.blockvpn.app.ui.screens.MainScreen
import com.blockvpn.app.ui.screens.ServersScreen
import com.blockvpn.app.ui.screens.SettingsScreen
import com.blockvpn.app.ui.screens.TagScreen
import com.blockvpn.app.ui.screens.TagStyle
import com.blockvpn.app.ui.theme.BlockTheme
import com.blockvpn.app.vpn.ProfileStore
import com.blockvpn.app.vpn.VpnManager
import com.blockvpn.app.vpn.VpnProfile
import kotlinx.coroutines.launch

class MainActivity : ComponentActivity() {

    private lateinit var prefs: UserPrefs
    private lateinit var store: ProfileStore
    private lateinit var vpn: VpnManager

    private var pendingProfile: VpnProfile? = null

    private val vpnPermissionLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            val profile = pendingProfile
            pendingProfile = null
            if (result.resultCode == Activity.RESULT_OK && profile != null) {
                startConnect(profile)
            } else {
                Toast.makeText(this, "Без разрешения системы VPN невозможен", Toast.LENGTH_LONG).show()
            }
        }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        prefs = UserPrefs(this)
        store = ProfileStore(this)
        vpn = VpnManager(this)

        if (prefs.autoConnect) {
            val profile = store.load().firstOrNull { it.id == prefs.activeProfileId }
            if (profile != null) {
                val prepare = VpnService.prepare(this)
                if (prepare == null) {
                    startConnect(profile)
                } else {
                    pendingProfile = profile
                    vpnPermissionLauncher.launch(prepare)
                }
            }
        }

        setContent {
            var screen by remember { mutableStateOf("main") }
            var profiles by remember { mutableStateOf(store.load()) }
            var activeId by remember { mutableStateOf(prefs.activeProfileId) }
            var tag by remember {
                mutableStateOf(TagStyle(prefs.tagText, prefs.tagColorIndex, prefs.tagStyleIndex))
            }
            var animations by remember { mutableStateOf(prefs.animationsEnabled) }
            var decor by remember { mutableStateOf(prefs.decorEnabled) }
            var autoConnect by remember { mutableStateOf(prefs.autoConnect) }
            var themeIndex by remember { mutableStateOf(prefs.themeIndex) }

            val vpnState by vpn.state.collectAsState()
            val errorText by vpn.lastError.collectAsState()

            var liveSeconds by remember { mutableStateOf(0L) }
            var liveRx by remember { mutableStateOf(0L) }
            var liveTx by remember { mutableStateOf(0L) }
            var liveHandshake by remember { mutableStateOf<Long?>(null) }

            val activeProfile = profiles.firstOrNull { it.id == activeId }
            val isDevMode = profiles.isEmpty()

            BlockTheme(paletteIndex = themeIndex) {
                val palette = com.blockvpn.app.ui.theme.paletteAt(themeIndex)

                when (screen) {
                    "servers" -> ServersScreen(
                        palette = palette,
                        profiles = profiles,
                        activeId = activeId,
                        onSelect = { p ->
                            activeId = p.id
                            prefs.activeProfileId = p.id
                            if (vpnState != com.blockvpn.app.vpn.VpnUiState.DISCONNECTED) {
                                lifecycleScope.launch {
                                    vpn.disconnect()
                                    startConnect(p)
                                }
                            }
                        },
                        onDelete = { p ->
                            store.remove(p.id)
                            profiles = store.load()
                            if (activeId == p.id) {
                                activeId = profiles.firstOrNull()?.id
                                prefs.activeProfileId = activeId
                            }
                        },
                        onAdd = { name, region, config ->
                            val created = store.add(name, region, config)
                            profiles = store.load()
                            if (activeId == null) {
                                activeId = created.id
                                prefs.activeProfileId = created.id
                            }
                            true
                        },
                        onBack = { screen = "main" },
                        canValidate = { text -> vpn.parseConfig(text) != null }
                    )

                    "tag" -> TagScreen(
                        palette = palette,
                        initial = tag,
                        onSave = { newTag ->
                            tag = newTag
                            prefs.tagText = newTag.text
                            prefs.tagColorIndex = newTag.colorIndex
                            prefs.tagStyleIndex = newTag.styleIndex
                            unlockStickers("tag_created")
                            screen = "main"
                        },
                        onBack = { screen = "main" }
                    )

                    "settings" -> SettingsScreen(
                        palette = palette,
                        themeIndex = themeIndex,
                        animations = animations,
                        decor = decor,
                        autoConnect = autoConnect,
                        onThemeChange = { i ->
                            if (i != themeIndex) unlockStickers("theme_flip")
                            themeIndex = i
                            prefs.themeIndex = i
                        },
                        onAnimationsChange = { b ->
                            animations = b
                            prefs.animationsEnabled = b
                        },
                        onDecorChange = { b ->
                            decor = b
                            prefs.decorEnabled = b
                        },
                        onAutoConnectChange = { b ->
                            autoConnect = b
                            prefs.autoConnect = b
                        },
                        onBack = { screen = "main" }
                    )

                    "help" -> HelpScreen(
                        palette = palette,
                        vpnState = vpnState,
                        activeConfigText = activeProfile?.configText,
                        onBack = { screen = "main" }
                    )

                    else -> MainScreen(
                        palette = palette,
                        state = vpnState,
                        errorText = errorText,
                        isDevMode = isDevMode,
                        profile = activeProfile,
                        tag = tag,
                        animations = animations,
                        decor = decor,
                        connectedSeconds = liveSeconds,
                        rx = liveRx,
                        tx = liveTx,
                        handshakeSecondsAgo = liveHandshake,
                        stickers = AllStickers.map { it to (it.id in prefs.stickers) },
                        onConnect = {
                            val profile = activeProfile
                            if (profile == null) {
                                Toast.makeText(this, "Сначала добавь сервер", Toast.LENGTH_SHORT).show()
                            } else {
                                requestAndConnect(profile)
                            }
                        },
                        onDisconnect = { lifecycleScope.launch { vpn.disconnect() } },
                        onRetry = {
                            val profile = activeProfile
                            if (profile != null) requestAndConnect(profile)
                        },
                        onOpenServers = { screen = "servers" },
                        onOpenTag = { screen = "tag" },
                        onOpenSettings = { screen = "settings" },
                        onOpenHelp = { screen = "help" }
                    )
                }
            }

            androidx.compose.runtime.LaunchedEffect(vpnState) {
                if (vpnState != com.blockvpn.app.vpn.VpnUiState.SECURED) {
                    liveSeconds = 0L
                    liveRx = 0L
                    liveTx = 0L
                    liveHandshake = null
                }
                var lastMinute = -1
                while (vpnState == com.blockvpn.app.vpn.VpnUiState.SECURED) {
                    liveSeconds = vpn.connectedSeconds()
                    vpn.stats()?.let {
                        liveRx = it.first
                        liveTx = it.second
                    }
                    liveHandshake = vpn.lastHandshakeSecondsAgo()
                    if (liveSeconds / 60 > lastMinute) {
                        lastMinute = (liveSeconds / 60).toInt()
                        prefs.totalMinutes = prefs.totalMinutes + 1
                        if (prefs.totalMinutes >= 60) unlockStickers("hour_up")
                    }
                    kotlinx.coroutines.delay(1000)
                }
            }
        }
    }

    private fun requestAndConnect(profile: VpnProfile) {
        val prepare = VpnService.prepare(this)
        if (prepare != null) {
            pendingProfile = profile
            vpnPermissionLauncher.launch(prepare)
        } else {
            startConnect(profile)
        }
    }

    private fun startConnect(profile: VpnProfile) {
        lifecycleScope.launch {
            val ok = vpn.connect(profile)
            if (ok) {
                prefs.totalConnects = prefs.totalConnects + 1
                unlockStickers("first_connect")
                if (prefs.totalConnects >= 10) unlockStickers("ten_connects")
            }
        }
    }

    private fun unlockStickers(vararg ids: String) {
        val current = prefs.stickers.toMutableSet()
        ids.forEach { current.add(it) }
        prefs.stickers = current
    }
}
