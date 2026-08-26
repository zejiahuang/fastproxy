-keep class com.github.promeg.pinyinhelper.** { *; }
-keep class com.github.promeg.tinypinyin.** { *; }
-keep class org.ahocorasick.** { *; }
-keep class net.steampp.app.shadowsocks.** { *; }
-keepclassmembers class net.steampp.app.shadowsocks.** { *; }

# AndroidX Java classes are called via JNI from managed bindings, R8 cannot
# trace these references and strips them as unreachable. Keep all androidx.
-keep class androidx.** { *; }
-keepclassmembers class androidx.** { *; }
-keep class com.google.android.material.** { *; }
-keepclassmembers class com.google.android.material.** { *; }
