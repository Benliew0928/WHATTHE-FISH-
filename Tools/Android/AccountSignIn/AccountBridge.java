package com.umpsa.whatthefish.accounts;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import com.google.android.gms.common.GoogleApiAvailabilityLight;
import com.huawei.hms.api.HuaweiApiAvailability;
import java.util.Locale;
import org.json.JSONObject;

/** Tokens remain in process memory until Unity consumes the single result. Never log them. */
public final class AccountBridge {
    private static int request;
    private static boolean active;
    private static String result = "";
    static AccountActivity screen;

    public static String configuration(Activity activity) {
        try {
            Bundle data = activity.getPackageManager().getApplicationInfo(
                activity.getPackageName(), PackageManager.GET_META_DATA).metaData;
            return new JSONObject()
                .put("googleClientId", data.getString("wtf.google.client", ""))
                .put("huaweiEndpoint", data.getString("wtf.huawei.endpoint", ""))
                .put("huaweiDevice", "huawei".equals(Build.MANUFACTURER.toLowerCase(Locale.ROOT))
                    || "huawei".equals(Build.BRAND.toLowerCase(Locale.ROOT)))
                .put("googleAvailable", GoogleApiAvailabilityLight.getInstance()
                    .isGooglePlayServicesAvailable(activity) == 0)
                .put("huaweiAvailable", HuaweiApiAvailability.getInstance()
                    .isHuaweiMobileServicesAvailable(activity) == 0).toString();
        } catch (Exception ignored) { return "{}"; }
    }

    public static synchronized void start(Activity parent, int id, String provider, String clientId) {
        if (active) throw new IllegalStateException("Sign-in already in progress");
        request = id; active = true; result = "";
        parent.runOnUiThread(() -> {
            try {
                parent.startActivity(new Intent(parent, AccountActivity.class)
                    .putExtra("request", id).putExtra("provider", provider).putExtra("clientId", clientId));
            } catch (Exception ignored) { finish(id, "error", "", "The account screen could not open."); }
        });
    }
    static synchronized boolean isActive(int id) { return active && request == id; }
    static synchronized void finish(int id, String status, String token, String message) {
        if (!isActive(id)) return;
        try {
            result = new JSONObject().put("request", id).put("status", status)
                .put("token", token).put("message", message).toString();
        } catch (Exception ignored) { result = ""; }
        active = false;
    }
    public static synchronized String takeResult() {
        String value = result; result = ""; return value;
    }
    public static void cancel(int id) {
        finish(id, "cancel", "", "");
        AccountActivity current = screen;
        if (current != null && current.request == id)
            current.runOnUiThread(() -> current.complete("cancel", "", ""));
    }
    public static void signOut(Activity activity) {
        activity.runOnUiThread(() -> {
            try {
                com.huawei.hms.support.account.AccountAuthManager.getService(activity,
                    new com.huawei.hms.support.account.request.AccountAuthParamsHelper().setId().createParams())
                    .signOut();
            } catch (Exception ignored) { }
            try {
                androidx.credentials.CredentialManager.create(activity).clearCredentialStateAsync(
                    new androidx.credentials.ClearCredentialStateRequest(), null, command -> activity.runOnUiThread(command),
                    new androidx.credentials.CredentialManagerCallback<Void, androidx.credentials.exceptions.ClearCredentialException>() {
                        public void onResult(Void ignored) { }
                        public void onError(androidx.credentials.exceptions.ClearCredentialException ignored) { }
                    });
            } catch (Exception ignored) { }
        });
    }
}
