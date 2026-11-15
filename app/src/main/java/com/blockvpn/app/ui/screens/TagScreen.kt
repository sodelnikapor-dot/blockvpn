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
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
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
import com.blockvpn.app.ui.theme.StreetPalette

@Composable
fun TagScreen(
    palette: StreetPalette,
    initial: TagStyle,
    onSave: (TagStyle) -> Unit,
    onBack: () -> Unit,
    modifier: Modifier = Modifier
) {
    var text by remember { mutableStateOf(initial.text) }
    var colorIndex by remember { mutableStateOf(initial.colorIndex) }
    var styleIndex by remember { mutableStateOf(initial.styleIndex) }

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
            Text("МОЙ ТЕГ", color = palette.accent, fontSize = 22.sp, fontWeight = FontWeight.Black)
            TextButton(onClick = onBack) { Text("← Назад", color = palette.dim) }
        }

        Spacer(Modifier.height(16.dp))

        Box(
            Modifier
                .fillMaxWidth()
                .height(120.dp)
                .background(palette.surface, RoundedCornerShape(14.dp)),
            contentAlignment = Alignment.Center
        ) {
            TagView(
                tag = TagStyle(text.ifBlank { "ТЕГ" }, colorIndex, styleIndex),
                palette = palette,
                glow = false,
                animations = false
            )
        }

        Spacer(Modifier.height(16.dp))

        OutlinedTextField(
            value = text,
            onValueChange = { if (it.length <= 12 && !it.contains('\n')) text = it },
            label = { Text("Надпись (до 12 символов)") },
            singleLine = true,
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(Modifier.height(16.dp))
        Text("Цвет", color = palette.dim, fontSize = 13.sp)
        Spacer(Modifier.height(8.dp))
        Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            TagColors.forEachIndexed { i, c ->
                Box(
                    Modifier
                        .size(36.dp)
                        .background(c, CircleShape)
                        .border(
                            width = if (i == colorIndex) 3.dp else 1.dp,
                            color = if (i == colorIndex) palette.text else palette.dim.copy(alpha = 0.4f),
                            shape = CircleShape
                        )
                        .clickable { colorIndex = i }
                )
            }
        }

        Spacer(Modifier.height(16.dp))
        Text("Стиль", color = palette.dim, fontSize = 13.sp)
        Spacer(Modifier.height(8.dp))
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            listOf("Жирный", "С тенью", "Наклон").forEachIndexed { i, label ->
                val selected = i == styleIndex
                Box(
                    Modifier
                        .background(
                            if (selected) palette.accent.copy(alpha = 0.2f) else palette.surface,
                            RoundedCornerShape(8.dp)
                        )
                        .border(
                            1.dp,
                            if (selected) palette.accent else palette.dim.copy(alpha = 0.4f),
                            RoundedCornerShape(8.dp)
                        )
                        .clickable { styleIndex = i }
                        .padding(horizontal = 12.dp, vertical = 8.dp)
                ) {
                    Text(label, color = if (selected) palette.accent else palette.text, fontSize = 13.sp)
                }
            }
        }

        Spacer(Modifier.weight(1f))

        Button(
            onClick = { onSave(TagStyle(text.ifBlank { "BLOCK" }, colorIndex, styleIndex)) },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Сохранить тег", fontWeight = FontWeight.Black)
        }
    }
}
