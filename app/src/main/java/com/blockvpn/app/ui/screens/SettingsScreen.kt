package com.blockvpn.app.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Switch
import androidx.compose.material3.SwitchDefaults
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import android.content.Intent
import android.provider.Settings
import com.blockvpn.app.ui.theme.Palettes
import com.blockvpn.app.ui.theme.StreetPalette

@Composable
fun SettingsScreen(
    palette: StreetPalette,
    themeIndex: Int,
    animations: Boolean,
    decor: Boolean,
    autoConnect: Boolean,
    onThemeChange: (Int) -> Unit,
    onAnimationsChange: (Boolean) -> Unit,
    onDecorChange: (Boolean) -> Unit,
    onAutoConnectChange: (Boolean) -> Unit,
    onBack: () -> Unit,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current

    Column(
        modifier = modifier
            .fillMaxSize()
            .background(palette.background)
            .statusBarsPadding()
            .verticalScroll(rememberScrollState())
            .padding(16.dp)
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text("НАСТРОЙКИ", color = palette.accent, fontSize = 22.sp, fontWeight = FontWeight.Black)
            TextButton(onClick = onBack) { Text("← Назад", color = palette.dim) }
        }

        Spacer(Modifier.height(16.dp))

        Text("Тема района", color = palette.dim, fontSize = 13.sp)
        Spacer(Modifier.height(8.dp))
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            Palettes.forEachIndexed { i, p ->
                val selected = i == themeIndex
                Column(
                    Modifier
                        .background(
                            if (selected) p.accent.copy(alpha = 0.2f) else palette.surface,
                            RoundedCornerShape(10.dp)
                        )
                        .padding(10.dp)
                ) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Box(
                            Modifier
                                .padding(end = 6.dp)
                                .size(14.dp)
                                .background(p.accent, RoundedCornerShape(4.dp))
                        )
                        Text(p.name, color = if (selected) p.accent else palette.text, fontSize = 13.sp)
                    }
                }
            }
        }

        Spacer(Modifier.height(16.dp))

        SettingsSwitch("Анимации", "Пульс статуса и подсветка тега", animations, palette, onAnimationsChange)
        SettingsSwitch("Декор", "Тег, стикеры и оформление главного экрана", decor, palette, onDecorChange)
        SettingsSwitch("Автоподключение при запуске", "Подключаться к активному серверу при открытии приложения", autoConnect, palette, onAutoConnectChange)

        Spacer(Modifier.height(16.dp))

        Column(
            Modifier
                .fillMaxWidth()
                .background(palette.surface, RoundedCornerShape(12.dp))
                .padding(14.dp)
        ) {
            Text("Защита от утечек (kill switch)", color = palette.text, fontSize = 15.sp, fontWeight = FontWeight.Bold)
            Spacer(Modifier.height(6.dp))
            Text(
                "Включи в системных настройках Android для BLOCKVPN: " +
                    "«VPN всегда активен» и «Блокировать соединения без VPN». " +
                    "Тогда трафик не пойдёт мимо туннеля при обрыве.",
                color = palette.dim,
                fontSize = 13.sp
            )
            Spacer(Modifier.height(10.dp))
            Button(onClick = {
                runCatching {
                    context.startActivity(Intent(Settings.ACTION_VPN_SETTINGS).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK))
                }
            }) {
                Text("Открыть настройки VPN")
            }
        }

        Spacer(Modifier.height(16.dp))

        Column(
            Modifier
                .fillMaxWidth()
                .background(palette.surface, RoundedCornerShape(12.dp))
                .padding(14.dp)
        ) {
            Text("Приватность", color = palette.text, fontSize = 15.sp, fontWeight = FontWeight.Bold)
            Spacer(Modifier.height(6.dp))
            Text(
                "Приложение не собирает историю посещений и не анализирует трафик. " +
                    "Ключи хранятся в системном хранилище Android Keystore. " +
                    "Стикеры, тег и темы — только на устройстве.",
                color = palette.dim,
                fontSize = 13.sp
            )
        }
    }
}

@Composable
private fun SettingsSwitch(
    title: String,
    subtitle: String,
    checked: Boolean,
    palette: StreetPalette,
    onChange: (Boolean) -> Unit
) {
    Row(
        Modifier
            .fillMaxWidth()
            .padding(vertical = 6.dp),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Column(Modifier.weight(1f)) {
            Text(title, color = palette.text, fontSize = 15.sp, fontWeight = FontWeight.Bold)
            Text(subtitle, color = palette.dim, fontSize = 12.sp)
        }
        Switch(
            checked = checked,
            onCheckedChange = onChange,
            colors = SwitchDefaults.colors(checkedTrackColor = palette.accent)
        )
    }
}
