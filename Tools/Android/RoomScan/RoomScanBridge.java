package com.umpsa.whatthefish.scan;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.Color;
import com.huawei.hms.hmsscankit.ScanUtil;
import com.huawei.hms.ml.scan.HmsBuildBitmapOption;
import com.huawei.hms.ml.scan.HmsScan;
import java.io.ByteArrayOutputStream;
import org.json.JSONObject;

public final class RoomScanBridge {
    private static String result = "";
    private static int activeRequest;
    private static boolean active;
    static RoomScanActivity screen;

    public static byte[] generate(String invitation) throws Exception {
        ScanUtil.setOperationTrackingEnabled(false);
        HmsBuildBitmapOption options = new HmsBuildBitmapOption.Creator()
            .setBitmapMargin(4).setBitmapColor(Color.BLACK).setBitmapBackgroundColor(Color.WHITE)
            .setQRErrorCorrection(HmsBuildBitmapOption.ErrorCorrectionLevel.M).create();
        Bitmap bitmap = ScanUtil.buildBitmap(invitation, HmsScan.QRCODE_SCAN_TYPE, 512, 512, options);
        try (ByteArrayOutputStream bytes = new ByteArrayOutputStream()) {
            if (!bitmap.compress(Bitmap.CompressFormat.PNG, 100, bytes)) throw new IllegalStateException("QR encoding failed");
            return bytes.toByteArray();
        } finally { bitmap.recycle(); }
    }
    public static synchronized void start(Activity parent, int request) {
        if (active) return;
        active = true; activeRequest = request; result = "";
        parent.runOnUiThread(() -> {
            try {
                ScanUtil.setOperationTrackingEnabled(false);
                parent.startActivity(new Intent(parent, RoomScanActivity.class));
            } catch (Exception e) { finish("error", "The scanner couldn't open. Enter the room code instead."); }
        });
    }
    static synchronized boolean isActive() { return active; }
    static synchronized void finish(String status, String value) {
        if (!active) return;
        try { result = new JSONObject().put("request", activeRequest).put("status", status).put("value", value).toString(); }
        catch (Exception ignored) { result = ""; }
        active = false;
    }
    public static synchronized String takeResult() { String value = result; result = ""; return value; }
    public static void cancel() {
        RoomScanActivity current = screen;
        if (current != null) current.runOnUiThread(() -> current.complete("cancel", ""));
        else finish("cancel", "");
    }
}
