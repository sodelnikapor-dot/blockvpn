package com.blockvpn.app.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.blockvpn.app.BuildConfig
import com.blockvpn.app.diag.Diagnostics
import com.blockvpn.app.ui.theme.StreetPalette
import com.blockvpn.app.vpn.VpnUiState
import kotlinx.coroutines.launch

private data class Faq(val q: String, val a: String)

private val FaqList = listOf(
    Faq(
        "Не подключается: «Сервер недоступен»",
        "Проверь интернет без VPN, затем Endpoint в конфиге (адрес:порт). " +
            "Попробуй другой сервер. Убедись, что ключи из конфига актуальны."
    ),
    Faq(
        "Системный диалог не появляется",
        "Если ты ранее отказал в разрешении VPN, Android может скрыть диалог: " +
            "Настройки → Приложения → BLOCKVPN → Разрешения, либо Настройки → VPN."
    ),
    Faq(
        "Как включить защиту от утечек",
        "Настройки → «Открыть настройки VPN» → для BLOCKVPN включи " +
            "«VPN всегда активен» и «Блокировать соединения без VPN»."
    ),
    Faq(
        "Интернет пропал после обрыва",
        "Так работает «Блокировать соединения без VPN»: без активного туннеля трафик блокируется. " +
            "Подключись снова или выключи эту опцию в системных настройках VPN."
    ),
    Faq(
        "DNS-утечки",
        "Приложение использует DNS из конфига туннеля. Проверь кнопкой «Проверить DNS» ниже: " +
            "она покажет, какой адрес возвращает система для тестового домена."
    )
)

@Composable
fun HelpScreen(
    palette: StreetPalette,
    vpnState: VpnUiState,
    activeConfigText: String?,
    onBack: () -> Unit,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current
    val scope = androidx.compose.runtime.rememberCoroutineScope()
    var vpnCheck by remember { mutableStateOf<String?>(null) }
    var dnsCheck by remember { mutableStateOf<String?>(null) }

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
            Text("ПОМОЩЬ", color = palette.accent, fontSize = 22.sp, fontWeight = FontWeight.Black)
            TextButton(onClick = onBack) { Text("← Назад", color = palette.dim) }
        }

        Spacer(Modifier.height(12.dp))

        Text("Диагностика", color = palette.dim, fontSize = 13.sp)
        Spacer(Modifier.height(8.dp))

        OutlinedButton(
            onClick = {
                vpnCheck = if (Diagnostics.vpnTransportActive(context)) {
                    "Активный VPN-интерфейс обнаружен в системе"
                } else {
                    "Активного VPN-интерфейса нет"
                }
            },
            modifier = Modifier.fillMaxWidth()
        ) { Text("Проверить активный VPN") }
        vpnCheck?.let {
            Text(it, color = palette.text, fontSize = 13.sp, modifier = Modifier.padding(top = 4.dp))
        }

        Spacer(Modifier.height(8.dp))

        OutlinedButton(
            onClick = {
                val dns = activeConfigText?.let { Diagnostics.extractDnsFromConfig(it) }
                if (dns == null) {
                    dnsCheck = "В активном конфиге не указан DNS — проверка невозможна"
                } else {
                    scope.launch {
                        val ip = Diagnostics.resolve("example.com")
                        dnsCheck = "DNS из конфига: $dns. Система разрешила example.com в ${
                            ip ?: "не удалось (возможна блокировка DNS)"
                        }"
                    }
                }
            },
            modifier = Modifier.fillMaxWidth()
        ) { Text("Проверить DNS") }
        dnsCheck?.let {
            Text(it, color = palette.text, fontSize = 13.sp, modifier = Modifier.padding(top = 4.dp))
        }

        Spacer(Modifier.height(8.dp))
        Text(
            "Версия приложения: ${BuildConfig.VERSION_NAME} · Протокол: WireGuard",
            color = palette.dim,
            fontSize = 12.sp
        )

        Spacer(Modifier.height(16.dp))
        Text("Частые проблемы", color = palette.dim, fontSize = 13.sp)
        Spacer(Modifier.height(8.dp))

        FaqList.forEach { item ->
            var expanded by remember { mutableStateOf(false) }
            Column(
                Modifier
                    .fillMaxWidth()
                    .background(palette.surface, RoundedCornerShape(12.dp))
                    .clickable { expanded = !expanded }
                    .padding(12.dp)
            ) {
                Text(item.q, color = palette.text, fontSize = 14.sp, fontWeight = FontWeight.Bold)
                if (expanded) {
                    Spacer(Modifier.height(6.dp))
                    Text(item.a, color = palette.dim, fontSize = 13.sp)
                }
            }
            Spacer(Modifier.height(6.dp))
        }
    }
}
