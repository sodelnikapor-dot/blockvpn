package com.blockvpn.app.ui.screens

import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.draw.scale
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.blockvpn.app.ui.theme.StreetPalette
import com.blockvpn.app.vpn.VpnProfile
import com.blockvpn.app.vpn.VpnUiState
import com.blockvpn.app.util.formatBytes
import com.blockvpn.app.util.formatDuration

data class TagStyle(
    val text: String,
    val colorIndex: Int,
    val styleIndex: Int
)

val TagColors = listOf(
    Color(0xFFB7F542),
    Color(0xFFFF3D81),
    Color(0xFF00E5FF),
    Color(0xFFFFC53D),
    Color(0xFF7C4DFF),
    Color(0xFFFF8A00)
)

fun stateLabel(state: VpnUiState): String = when (state) {
    VpnUiState.DISCONNECTED -> "ОТКЛЮЧЕНО"
    VpnUiState.CONNECTING -> "ПОДКЛЮЧЕНИЕ…"
    VpnUiState.SECURED -> "ЗАЩИЩЕНО"
    VpnUiState.RECONNECTING -> "ПЕРЕПОДКЛЮЧЕНИЕ…"
    VpnUiState.ERROR -> "ОШИБКА"
}

@Composable
fun StatusCircle(
    state: VpnUiState,
    palette: StreetPalette,
    animations: Boolean,
    modifier: Modifier = Modifier
) {
    val color = when (state) {
        VpnUiState.SECURED -> palette.ok
        VpnUiState.CONNECTING, VpnUiState.RECONNECTING -> palette.warn
        VpnUiState.ERROR -> palette.err
        else -> palette.dim
    }
    val transition = rememberInfiniteTransition(label = "status")
    val pulse by transition.animateFloat(
        initialValue = 0.96f,
        targetValue = 1.04f,
        animationSpec = infiniteRepeatable(tween(900), RepeatMode.Reverse),
        label = "pulse"
    )
    val animated = animations && state != VpnUiState.DISCONNECTED && state != VpnUiState.ERROR
    val scale = if (animated) pulse else 1f

    Box(modifier = modifier.scale(scale), contentAlignment = Alignment.Center) {
        Box(
            Modifier
                .size(230.dp)
                .background(
                    Brush.radialGradient(
                        listOf(color.copy(alpha = 0.30f), Color.Transparent)
                    ),
                    CircleShape
                )
        )
        Box(
            Modifier
                .size(185.dp)
                .background(
                    Brush.radialGradient(
                        listOf(color.copy(alpha = 0.22f), color.copy(alpha = 0.04f))
                    ),
                    CircleShape
                )
                .border(3.dp, color, CircleShape)
        )
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Text(
                text = stateLabel(state),
                color = color,
                fontSize = 20.sp,
                fontWeight = FontWeight.Black,
                textAlign = TextAlign.Center
            )
        }
    }
}

@Composable
fun TagView(
    tag: TagStyle,
    palette: StreetPalette,
    glow: Boolean,
    animations: Boolean,
    modifier: Modifier = Modifier
) {
    val tagColor = TagColors[tag.colorIndex.coerceIn(0, TagColors.lastIndex)]
    val transition = rememberInfiniteTransition(label = "tag")
    val glowAlpha by transition.animateFloat(
        initialValue = 0.5f,
        targetValue = 1f,
        animationSpec = infiniteRepeatable(tween(700), RepeatMode.Reverse),
        label = "glow"
    )
    val borderAlpha = if (glow && animations) glowAlpha else 0.9f
    val rotation = if (tag.styleIndex == 2) 2f else -2f

    Box(
        modifier = modifier
            .rotate(rotation)
            .background(palette.surface, RoundedCornerShape(10.dp))
            .border(2.dp, tagColor.copy(alpha = borderAlpha), RoundedCornerShape(10.dp))
            .padding(horizontal = 12.dp, vertical = 6.dp)
    ) {
        Text(
            text = tag.text.uppercase(),
            color = tagColor,
            fontSize = 18.sp,
            fontWeight = FontWeight.Black,
            fontStyle = if (tag.styleIndex == 2) FontStyle.Italic else FontStyle.Normal,
            style = if (tag.styleIndex == 1) {
                androidx.compose.ui.text.TextStyle(
                    shadow = androidx.compose.ui.graphics.Shadow(
                        color = tagColor.copy(alpha = 0.8f),
                        blurRadius = 8f
                    )
                )
            } else {
                androidx.compose.ui.text.TextStyle()
            }
        )
    }
}

@Composable
fun BottomNav(
    palette: StreetPalette,
    current: String,
    onNavigate: (String) -> Unit,
    modifier: Modifier = Modifier
) {
    val items = listOf(
        "main" to "Главная",
        "servers" to "Серверы",
        "tag" to "Тег",
        "settings" to "Настройки",
        "help" to "Помощь"
    )
    Row(
        modifier = modifier
            .fillMaxWidth()
            .background(palette.surface)
            .padding(vertical = 6.dp),
        horizontalArrangement = Arrangement.SpaceEvenly
    ) {
        items.forEach { (route, label) ->
            val selected = route == current
            Text(
                text = label,
                color = if (selected) palette.accent else palette.dim,
                fontSize = 13.sp,
                fontWeight = if (selected) FontWeight.Bold else FontWeight.Normal,
                modifier = Modifier
                    .clickable { onNavigate(route) }
                    .padding(horizontal = 8.dp, vertical = 6.dp)
            )
        }
    }
}

@Composable
fun MainScreen(
    palette: StreetPalette,
    state: VpnUiState,
    errorText: String?,
    isDevMode: Boolean,
    profile: VpnProfile?,
    tag: TagStyle,
    animations: Boolean,
    decor: Boolean,
    connectedSeconds: Long,
    rx: Long,
    tx: Long,
    handshakeSecondsAgo: Long?,
    stickers: List<Pair<Sticker, Boolean>>,
    onConnect: () -> Unit,
    onDisconnect: () -> Unit,
    onRetry: () -> Unit,
    onOpenServers: () -> Unit,
    onOpenTag: () -> Unit,
    onOpenSettings: () -> Unit,
    onOpenHelp: () -> Unit,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier
            .fillMaxSize()
            .background(
                Brush.verticalGradient(
                    listOf(palette.background, palette.background.copy(alpha = 0.97f))
                )
            )
            .statusBarsPadding()
            .padding(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(
                text = "BLOCKVPN",
                color = palette.accent,
                fontSize = 22.sp,
                fontWeight = FontWeight.Black
            )
            if (decor) {
                TagView(
                    tag = tag,
                    palette = palette,
                    glow = state == VpnUiState.SECURED,
                    animations = animations
                )
            }
        }

        Spacer(Modifier.height(8.dp))

        if (isDevMode) {
            Box(
                Modifier
                    .fillMaxWidth()
                    .background(palette.warn.copy(alpha = 0.15f), RoundedCornerShape(10.dp))
                    .border(1.dp, palette.warn, RoundedCornerShape(10.dp))
                    .padding(10.dp)
            ) {
                Text(
                    text = "РЕЖИМ РАЗРАБОТКИ: серверы не настроены. " +
                        "Импортируй конфиг WireGuard в разделе «Серверы». " +
                        "VPN сейчас НЕ активен.",
                    color = palette.warn,
                    fontSize = 13.sp
                )
            }
        }

        Spacer(Modifier.height(12.dp))

        StatusCircle(
            state = state,
            palette = palette,
            animations = animations
        )

        Spacer(Modifier.height(12.dp))

        profile?.let { p ->
            OutlinedButton(
                onClick = onOpenServers,
                colors = ButtonDefaults.outlinedButtonColors(contentColor = palette.text)
            ) {
                Text("Сервер: ${p.name} · ${p.region}", fontSize = 14.sp)
            }
        } ?: run {
            OutlinedButton(
                onClick = onOpenServers,
                colors = ButtonDefaults.outlinedButtonColors(contentColor = palette.dim)
            ) {
                Text("Сервер не выбран →", fontSize = 14.sp)
            }
        }

        Spacer(Modifier.height(8.dp))

        when (state) {
            VpnUiState.SECURED, VpnUiState.CONNECTING, VpnUiState.RECONNECTING -> {
                Button(
                    onClick = onDisconnect,
                    colors = ButtonDefaults.buttonColors(
                        containerColor = palette.err,
                        contentColor = Color(0xFF0C0C10)
                    ),
                    modifier = Modifier.widthIn(min = 200.dp)
                ) {
                    Text(if (state == VpnUiState.SECURED) "ОТКЛЮЧИТЬСЯ" else "ОСТАНОВИТЬ", fontWeight = FontWeight.Black)
                }
            }
            else -> {
                Button(
                    onClick = onConnect,
                    enabled = profile != null,
                    colors = ButtonDefaults.buttonColors(
                        containerColor = palette.accent,
                        contentColor = Color(0xFF0C0C10),
                        disabledContainerColor = palette.dim.copy(alpha = 0.3f),
                        disabledContentColor = palette.dim
                    ),
                    modifier = Modifier.widthIn(min = 200.dp)
                ) {
                    Text("ПОДКЛЮЧИТЬСЯ", fontWeight = FontWeight.Black)
                }
            }
        }

        if (state == VpnUiState.ERROR) {
            Spacer(Modifier.height(8.dp))
            OutlinedButton(onClick = onRetry) {
                Text("Повторить подключение")
            }
        }

        errorText?.let {
            Spacer(Modifier.height(8.dp))
            Text(
                text = it,
                color = palette.err,
                fontSize = 13.sp,
                textAlign = TextAlign.Center
            )
        }

        Spacer(Modifier.weight(1f))

        if (decor) {
            Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                stickers.take(5).forEach { (sticker, unlocked) ->
                    Box(
                        Modifier
                            .alpha(if (unlocked) 1f else 0.25f)
                            .background(palette.surface, RoundedCornerShape(8.dp))
                            .padding(6.dp)
                    ) {
                        Text(sticker.emoji, fontSize = 16.sp)
                    }
                }
            }
            Spacer(Modifier.height(8.dp))
        }

        Row(
            modifier = Modifier
                .fillMaxWidth()
                .background(palette.surface, RoundedCornerShape(12.dp))
                .padding(12.dp),
            horizontalArrangement = Arrangement.SpaceEvenly
        ) {
            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                Text("ВРЕМЯ", color = palette.dim, fontSize = 11.sp)
                Text(formatDuration(connectedSeconds), color = palette.text, fontSize = 15.sp, fontWeight = FontWeight.Bold)
            }
            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                Text("ЗАГРУЖЕНО", color = palette.dim, fontSize = 11.sp)
                Text(formatBytes(rx), color = palette.text, fontSize = 15.sp, fontWeight = FontWeight.Bold)
            }
            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                Text("ОТПРАВЛЕНО", color = palette.dim, fontSize = 11.sp)
                Text(formatBytes(tx), color = palette.text, fontSize = 15.sp, fontWeight = FontWeight.Bold)
            }
            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                Text("РУКОПОЖАТИЕ", color = palette.dim, fontSize = 11.sp)
                Text(
                    text = handshakeSecondsAgo?.let { "${it}с" } ?: "—",
                    color = palette.text,
                    fontSize = 15.sp,
                    fontWeight = FontWeight.Bold
                )
            }
        }

        Spacer(Modifier.height(8.dp))

        BottomNav(
            palette = palette,
            current = "main",
            onNavigate = { route ->
                when (route) {
                    "main" -> {}
                    else -> onNavigateRoute(route, onOpenServers, onOpenTag, onOpenSettings, onOpenHelp)
                }
            }
        )
    }
}

private fun onNavigateRoute(
    route: String,
    onOpenServers: () -> Unit,
    onOpenTag: () -> Unit,
    onOpenSettings: () -> Unit,
    onOpenHelp: () -> Unit
) {
    when (route) {
        "servers" -> onOpenServers()
        "tag" -> onOpenTag()
        "settings" -> onOpenSettings()
        "help" -> onOpenHelp()
    }
}
