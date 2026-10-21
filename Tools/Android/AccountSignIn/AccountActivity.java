package com.umpsa.whatthefish.accounts;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.os.CancellationSignal;
import android.os.Handler;
import android.os.Looper;
import androidx.credentials.*;
import androidx.credentials.exceptions.*;
import com.google.android.libraries.identity.googleid.*;
import com.huawei.hms.support.account.AccountAuthManager;
import com.huawei.hms.support.account.request.AccountAuthParamsHelper;

/** Separate non-exported screen: Unity's game activity and scanner are not replaced. */
public final class AccountActivity extends Activity {
    private static final int HUAWEI = 4101;
    int request;
    private boolean completed;
    private final CancellationSignal cancellation = new CancellationSignal();
    private final Handler handler = new Handler(Looper.getMainLooper());
    private final Runnable timeout = () -> complete("error", "", "Sign-in timed out. Please try again.");

    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        request = getIntent().getIntExtra("request", -1);
        if (!AccountBridge.isActive(request)) { finish(); return; }
        AccountBridge.screen = this;
        handler.postDelayed(timeout, 180000);
        // A process recreation has no trustworthy in-memory transaction to resume.
        if (state != null) { complete("cancel", "", "Please try signing in again."); return; }
        try {
            if ("huawei".equals(getIntent().getStringExtra("provider"))) {
                // Identity only: no email, phone, contacts or provider profile permission.
                var params = new AccountAuthParamsHelper().setId().setIdToken().setForceRefresh().createParams();
                startActivityForResult(AccountAuthManager.getService(this, params).getSignInIntent(), HUAWEI);
            } else if ("google".equals(getIntent().getStringExtra("provider"))) {
                var option = new GetSignInWithGoogleOption.Builder(getIntent().getStringExtra("clientId")).build();
                var requestOptions = new GetCredentialRequest.Builder().addCredentialOption(option).build();
                CredentialManager.create(this).getCredentialAsync(this, requestOptions, cancellation,
                    command -> handler.post(command),
                    new CredentialManagerCallback<GetCredentialResponse, GetCredentialException>() {
                        public void onResult(GetCredentialResponse response) {
                            try {
                                Credential credential = response.getCredential();
                                if (!(credential instanceof CustomCredential)
                                    || !GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL.equals(credential.getType()))
                                    throw new IllegalArgumentException("Unexpected credential");
                                complete("ok", GoogleIdTokenCredential.createFrom(credential.getData()).getIdToken(), "");
                            } catch (Exception ignored) { complete("error", "", "Google returned an unreadable credential."); }
                        }
                        public void onError(GetCredentialException error) {
                            if (error instanceof GetCredentialCancellationException)
                                complete("cancel", "", "");
                            else if (error instanceof NoCredentialException)
                                complete("error", "", "Add a Google account in your phone settings, then try again.");
                            else complete("error", "", "Google sign-in is unavailable. Check Play services and your connection.");
                        }
                    });
            } else complete("error", "", "Unknown sign-in provider.");
        } catch (Exception ignored) { complete("error", "", "Sign-in could not start. Check your phone services and try again."); }
    }
    @Override protected void onActivityResult(int code, int resultCode, Intent data) {
        super.onActivityResult(code, resultCode, data);
        if (code != HUAWEI || completed) return;
        if (resultCode == RESULT_CANCELED && data == null) { complete("cancel", "", ""); return; }
        try {
            var task = AccountAuthManager.parseAuthResultFromIntent(data);
            if (task.isSuccessful()) complete("ok", task.getResult().getIdToken(), "");
            else if (resultCode == RESULT_CANCELED) complete("cancel", "", "");
            else complete("error", "", "Huawei sign-in failed. Check HMS Core, your connection and the app configuration.");
        } catch (Exception ignored) { complete("error", "", "Huawei returned an unreadable credential."); }
    }
    void complete(String status, String token, String message) {
        if (completed) return; completed = true;
        if ("ok".equals(status) && (token == null || token.isEmpty())) {
            status = "error"; token = ""; message = "The provider did not return an identity token.";
        }
        AccountBridge.finish(request, status, token, message);
        cancellation.cancel(); handler.removeCallbacks(timeout); finish();
    }
    @Override public void onBackPressed() { complete("cancel", "", ""); }
    @Override protected void onDestroy() {
        if (!completed) complete("cancel", "", "");
        if (AccountBridge.screen == this) AccountBridge.screen = null;
        handler.removeCallbacksAndMessages(null);
        super.onDestroy();
    }
}
