package com.blockvpn.app.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.blockvpn.app.diag.Diagnostics
import com.blockvpn.app.ui.theme.StreetPalette
import com.blockvpn.app.vpn.VpnProfile

@Composable
fun ServersScreen(
    palette: StreetPalette,
    profiles: List<VpnProfile>,
    activeId: String?,
    onSelect: (VpnProfile) -> Unit,
    onDelete: (VpnProfile) -> Unit,
    onAdd: (name: String, region: String, config: String) -> Boolean,
    onBack: () -> Unit,
    canValidate: (String) -> Boolean,
    modifier: Modifier = Modifier
) {
    var showAdd by remember { mutableStateOf(false) }
    var addError by remember { mutableStateOf<String?>(null) }

    Column(
        modifier = modifier
            .fillMaxSize()
            .background(palette.background)
            .statusBarsPadding()
            .padding(16.dp)
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text("СЕРВЕРЫ", color = palette.accent, fontSize = 22.sp, fontWeight = FontWeight.Black)
            TextButton(onClick = onBack) { Text("← Назад", color = palette.dim) }
        }

        Spacer(Modifier.height(12.dp))

        if (profiles.isEmpty()) {
            Box(
                Modifier
                    .fillMaxWidth()
                    .background(palette.surface, RoundedCornerShape(12.dp))
                    .padding(16.dp)
            ) {
                Text(
                    "Пока нет ни одного сервера. Добавь конфиг WireGuard (.conf): " +
                        "секции [Interface] и [Peer] с Endpoint и ключами.",
                    color = palette.dim,
                    fontSize = 14.sp
                )
            }
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                items(profiles, key = { it.id }) { p ->
                    val selected = p.id == activeId
                    Box(
                        Modifier
                            .fillMaxWidth()
                            .background(palette.surface, RoundedCornerShape(12.dp))
                            .border(
                                width = if (selected) 2.dp else 1.dp,
                                color = if (selected) palette.accent else palette.dim.copy(alpha = 0.3f),
                                shape = RoundedCornerShape(12.dp)
                            )
                            .clickable { onSelect(p) }
                            .padding(12.dp)
                    ) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column(Modifier.weight(1f)) {
                                Text(p.name, color = palette.text, fontSize = 16.sp, fontWeight = FontWeight.Bold)
                                Text(p.region, color = palette.dim, fontSize = 13.sp)
                                Text(
                                    Diagnostics.extractEndpointFromConfig(p.configText) ?: "endpoint не указан",
                                    color = palette.dim,
                                    fontSize = 12.sp
                                )
                            }
                            if (selected) {
                                Text("АКТИВЕН", color = palette.accent, fontSize = 12.sp, fontWeight = FontWeight.Black)
                            }
                            TextButton(onClick = { onDelete(p) }) {
                                Text("Удалить", color = palette.err, fontSize = 12.sp)
                            }
                        }
                    }
                }
            }
        }

        Spacer(Modifier.weight(1f))

        Button(
            onClick = { showAdd = true; addError = null },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("+ Добавить сервер", fontWeight = FontWeight.Black)
        }
    }

    if (showAdd) {
        var name by remember { mutableStateOf("") }
        var region by remember { mutableStateOf("") }
        var config by remember { mutableStateOf("") }

        AlertDialog(
            onDismissRequest = { showAdd = false },
            title = { Text("Новый сервер") },
            text = {
                Column {
                    OutlinedTextField(
                        value = name,
                        onValueChange = { name = it },
                        label = { Text("Название") },
                        singleLine = true
                    )
                    OutlinedTextField(
                        value = region,
                        onValueChange = { region = it },
                        label = { Text("Регион (например, Германия)") },
                        singleLine = true
                    )
                    OutlinedTextField(
                        value = config,
                        onValueChange = { config = it },
                        label = { Text("Конфиг WireGuard") },
                        minLines = 6,
                        maxLines = 10
                    )
                    addError?.let {
                        Spacer(Modifier.height(6.dp))
                        Text(it, color = palette.err, fontSize = 13.sp)
                    }
                }
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        when {
                            name.isBlank() -> addError = "Укажи название"
                            !canValidate(config) -> addError = "Это не похоже на конфиг WireGuard: проверь [Interface], [Peer], ключи и Endpoint"
                            else -> {
                                if (onAdd(name.trim(), region.ifBlank { "Неизвестно" }, config)) {
                                    showAdd = false
                                } else {
                                    addError = "Не удалось сохранить"
                                }
                            }
                        }
                    }
                ) { Text("Сохранить") }
            },
            dismissButton = {
                TextButton(onClick = { showAdd = false }) { Text("Отмена") }
            },
            containerColor = MaterialTheme.colorScheme.surface
        )
    }
}
