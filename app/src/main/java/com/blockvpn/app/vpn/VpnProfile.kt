package com.blockvpn.app.vpn

data class VpnProfile(
    val id: String,
    val name: String,
    val region: String,
    val configText: String
)
