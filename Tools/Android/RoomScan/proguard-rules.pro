# Scan Kit 2.15.0.301 ships consumer rules for its own reflection/JNI classes.
# Keep the Unity-facing bridge entry points. Do not keep the entire HMS graph.
-keep class com.umpsa.whatthefish.scan.** { *; }

# Optional providers referenced by Huawei Network Kit 8.0.1.316 and security
# dependencies: OEM-only APIs, QUIC/Cronet, advanced networking, analytics and
# alternative crypto/TLS. These are absent from Huawei's published dependency
# graph and guarded by runtime availability/fallback checks. The local scanner
# does not enable these optional services. Exact R8 diagnostics are scoped here;
# never use -ignorewarnings or a blanket -dontwarn com.huawei.**.
-dontwarn android.telephony.HwTelephonyManager
-dontwarn com.android.org.conscrypt.OpenSSLSocketImpl
-dontwarn com.huawei.android.os.BuildEx$VERSION
-dontwarn com.huawei.hms.framework.network.restclient.dnkeeper.DNKeeperManager
-dontwarn com.huawei.hms.framework.network.restclient.dnkeeper.RequestHost
-dontwarn com.huawei.hms.framework.network.restclient.hwhttp.ClientConfiguration
-dontwarn com.huawei.hms.framework.network.restclient.hwhttp.dns.DnsResult
-dontwarn com.huawei.hms.hquic.HQUICManager$HQUICInitCallback
-dontwarn com.huawei.hms.hquic.HQUICManager
-dontwarn com.huawei.hms.hquic.HQUICProvider
-dontwarn com.huawei.hms.network.AdvanceNetworkKitProvider
-dontwarn com.huawei.hms.network.abtest.ABHelper
-dontwarn com.huawei.hms.network.api.advance.AdvanceNetworkKit
-dontwarn com.huawei.hms.network.api.advance.BandwidthCallBack
-dontwarn com.huawei.hms.network.api.advance.ReportCallBack
-dontwarn com.huawei.hms.network.api.advance.WrapperLogger
-dontwarn com.huawei.hms.network.httpclient.hianalytics.EditableMetrics
-dontwarn com.huawei.hms.support.hianalytics.HiAnalyticsUtils
-dontwarn com.huawei.hms.utils.HMSBIInitializer
-dontwarn com.huawei.libcore.io.ExternalStorageFile
-dontwarn com.huawei.libcore.io.ExternalStorageFileInputStream
-dontwarn com.huawei.libcore.io.ExternalStorageFileOutputStream
-dontwarn com.huawei.libcore.io.ExternalStorageRandomAccessFile
-dontwarn org.bouncycastle.crypto.BlockCipher
-dontwarn org.bouncycastle.crypto.engines.AESEngine
-dontwarn org.bouncycastle.crypto.prng.SP800SecureRandom
-dontwarn org.bouncycastle.crypto.prng.SP800SecureRandomBuilder
-dontwarn org.chromium.net.BidirectionalStream$Callback
-dontwarn org.chromium.net.CronetEngine
-dontwarn org.chromium.net.ExperimentalBidirectionalStream$Builder
-dontwarn org.chromium.net.ExperimentalBidirectionalStream
-dontwarn org.chromium.net.ExperimentalCronetEngine$Builder
-dontwarn org.chromium.net.ExperimentalCronetEngine
-dontwarn org.chromium.net.ExperimentalUrlRequest$Builder
-dontwarn org.chromium.net.ExperimentalUrlRequest
-dontwarn org.chromium.net.NetworkException
-dontwarn org.chromium.net.RequestFinishedInfo$Listener
-dontwarn org.chromium.net.UploadDataProvider
-dontwarn org.chromium.net.UrlRequest$Builder
-dontwarn org.chromium.net.UrlRequest$Callback
-dontwarn org.chromium.net.UrlRequest
-dontwarn org.chromium.net.UrlResponseInfo
-dontwarn org.chromium.net.impl.ImplVersion
-dontwarn org.conscrypt.Conscrypt$ProviderBuilder
-dontwarn org.conscrypt.Conscrypt
