package com.blockvpn.app.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

data class StreetPalette(
    val name: String,
    val background: Color,
    val surface: Color,
    val accent: Color,
    val accent2: Color,
    val text: Color,
    val dim: Color,
    val ok: Color,
    val warn: Color,
    val err: Color
)

val NightDistrict = StreetPalette(
    name = "Ночной квартал",
    background = Color(0xFF0C0C10),
    surface = Color(0xFF17171E),
    accent = Color(0xFFB7F542),
    accent2 = Color(0xFF7C4DFF),
    text = Color(0xFFF2F2F0),
    dim = Color(0xFF9A9AA5),
    ok = Color(0xFF7CE84A),
    warn = Color(0xFFFFC53D),
    err = Color(0xFFFF4D5E)
)

val NeonMall = StreetPalette(
    name = "Неон-молл",
    background = Color(0xFF0A0A12),
    surface = Color(0xFF151527),
    accent = Color(0xFFFF3D81),
    accent2 = Color(0xFF00E5FF),
    text = Color(0xFFF5F2FA),
    dim = Color(0xFF9C93B0),
    ok = Color(0xFF00E676),
    warn = Color(0xFFFFC53D),
    err = Color(0xFFFF4D5E)
)

val RustyWagon = StreetPalette(
    name = "Ржавый вагон",
    background = Color(0xFF12100C),
    surface = Color(0xFF1E1A13),
    accent = Color(0xFFFF8A00),
    accent2 = Color(0xFFD7B65F),
    text = Color(0xFFF4EFE6),
    dim = Color(0xFFA89C88),
    ok = Color(0xFF9CCC65),
    warn = Color(0xFFFFC53D),
    err = Color(0xFFFF5A3C)
)

val Palettes = listOf(NightDistrict, NeonMall, RustyWagon)

fun paletteAt(index: Int): StreetPalette = Palettes[index.coerceIn(0, Palettes.lastIndex)]

@Composable
fun BlockTheme(
    paletteIndex: Int,
    content: @Composable () -> Unit
) {
    val palette = paletteAt(paletteIndex)
    val scheme = darkColorScheme(
        primary = palette.accent,
        secondary = palette.accent2,
        background = palette.background,
        surface = palette.surface,
        onPrimary = Color(0xFF0C0C10),
        onSecondary = Color(0xFF0C0C10),
        onBackground = palette.text,
        onSurface = palette.text,
        error = palette.err
    )
    MaterialTheme(
        colorScheme = scheme,
        content = content
    )
}
