package com.blockvpn.app.vpn

import android.content.Context
import org.json.JSONArray
import org.json.JSONObject
import java.io.File
import java.util.UUID

class ProfileStore(context: Context) {

    private val file = File(context.filesDir, "profiles.enc")

    fun load(): List<VpnProfile> {
        if (!file.exists()) return emptyList()
        return try {
            val plain = KeystoreBox.decrypt(file.readBytes())
            val arr = JSONArray(String(plain, Charsets.UTF_8))
            (0 until arr.length()).map { i ->
                val o = arr.getJSONObject(i)
                VpnProfile(
                    id = o.getString("id"),
                    name = o.getString("name"),
                    region = o.getString("region"),
                    configText = o.getString("config")
                )
            }
        } catch (e: Exception) {
            emptyList()
        }
    }

    fun save(profiles: List<VpnProfile>) {
        val arr = JSONArray()
        profiles.forEach { p ->
            arr.put(
                JSONObject()
                    .put("id", p.id)
                    .put("name", p.name)
                    .put("region", p.region)
                    .put("config", p.configText)
            )
        }
        file.writeBytes(KeystoreBox.encrypt(arr.toString().toByteArray(Charsets.UTF_8)))
    }

    fun add(name: String, region: String, configText: String): VpnProfile {
        val profiles = load().toMutableList()
        val profile = VpnProfile(UUID.randomUUID().toString(), name, region, configText.trim())
        profiles.add(profile)
        save(profiles)
        return profile
    }

    fun remove(id: String) {
        save(load().filterNot { it.id == id })
    }
}
