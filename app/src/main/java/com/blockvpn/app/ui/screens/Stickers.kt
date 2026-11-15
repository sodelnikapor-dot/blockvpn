package com.blockvpn.app.ui.screens

data class Sticker(
    val id: String,
    val emoji: String,
    val title: String,
    val hint: String
)

val AllStickers = listOf(
    Sticker("first_connect", "🔥", "Первый коннект", "Подключись к серверу"),
    Sticker("tag_created", "🎨", "Свой тег", "Создай тег в разделе «Тег»"),
    Sticker("theme_flip", "🌆", "Смена района", "Переключи тему оформления"),
    Sticker("ten_connects", "💯", "Стабильный", "10 подключений"),
    Sticker("hour_up", "⏱️", "Час под защитой", "60 минут суммарного соединения")
)
