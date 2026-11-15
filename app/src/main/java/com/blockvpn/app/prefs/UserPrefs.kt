package com.blockvpn.app.prefs

import android.content.Context

class UserPrefs(context: Context) {

    private val sp = context.getSharedPreferences("blockvpn_ui", Context.MODE_PRIVATE)

    var tagText: String
        get() = sp.getString(KEY_TAG_TEXT, "BLOCK") ?: "BLOCK"
        set(value) = sp.edit().putString(KEY_TAG_TEXT, value).apply()

    var tagColorIndex: Int
        get() = sp.getInt(KEY_TAG_COLOR, 0)
        set(value) = sp.edit().putInt(KEY_TAG_COLOR, value).apply()

    var tagStyleIndex: Int
        get() = sp.getInt(KEY_TAG_STYLE, 0)
        set(value) = sp.edit().putInt(KEY_TAG_STYLE, value).apply()

    var themeIndex: Int
        get() = sp.getInt(KEY_THEME, 0)
        set(value) = sp.edit().putInt(KEY_THEME, value).apply()

    var animationsEnabled: Boolean
        get() = sp.getBoolean(KEY_ANIMATIONS, true)
        set(value) = sp.edit().putBoolean(KEY_ANIMATIONS, value).apply()

    var decorEnabled: Boolean
        get() = sp.getBoolean(KEY_DECOR, true)
        set(value) = sp.edit().putBoolean(KEY_DECOR, value).apply()

    var autoConnect: Boolean
        get() = sp.getBoolean(KEY_AUTOCONNECT, false)
        set(value) = sp.edit().putBoolean(KEY_AUTOCONNECT, value).apply()

    var activeProfileId: String?
        get() = sp.getString(KEY_ACTIVE_PROFILE, null)
        set(value) = sp.edit().putString(KEY_ACTIVE_PROFILE, value).apply()

    var totalConnects: Int
        get() = sp.getInt(KEY_TOTAL_CONNECTS, 0)
        set(value) = sp.edit().putInt(KEY_TOTAL_CONNECTS, value).apply()

    var totalMinutes: Int
        get() = sp.getInt(KEY_TOTAL_MINUTES, 0)
        set(value) = sp.edit().putInt(KEY_TOTAL_MINUTES, value).apply()

    var stickers: Set<String>
        get() = sp.getStringSet(KEY_STICKERS, emptySet()) ?: emptySet()
        set(value) = sp.edit().putStringSet(KEY_STICKERS, value).apply()

    private companion object {
        const val KEY_TAG_TEXT = "tag_text"
        const val KEY_TAG_COLOR = "tag_color"
        const val KEY_TAG_STYLE = "tag_style"
        const val KEY_THEME = "theme"
        const val KEY_ANIMATIONS = "animations"
        const val KEY_DECOR = "decor"
        const val KEY_AUTOCONNECT = "autoconnect"
        const val KEY_ACTIVE_PROFILE = "active_profile"
        const val KEY_TOTAL_CONNECTS = "total_connects"
        const val KEY_TOTAL_MINUTES = "total_minutes"
        const val KEY_STICKERS = "stickers"
    }
}
