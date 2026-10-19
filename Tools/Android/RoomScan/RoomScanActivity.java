package com.umpsa.whatthefish.scan;

import android.Manifest;
import android.app.Activity;
import android.content.pm.PackageManager;
import android.graphics.Color;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.huawei.hms.hmsscankit.RemoteView;
import com.huawei.hms.ml.scan.HmsScan;

// Camera-only customized view: no gallery access or storage permission.
public final class RoomScanActivity extends Activity {
    private RemoteView scanner;
    private FrameLayout root;
    private boolean completed, started, resumed;
    private static final int CAMERA_REQUEST = 71;
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state);
        if (!RoomScanBridge.isActive()) { finish(); return; }
        RoomScanBridge.screen = this;
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        root = new FrameLayout(this); root.setBackgroundColor(Color.BLACK); setContentView(root);
        if (checkSelfPermission(Manifest.permission.CAMERA) == PackageManager.PERMISSION_GRANTED) openScanner();
        else requestPermissions(new String[]{Manifest.permission.CAMERA}, CAMERA_REQUEST);
    }
    private int dp(int value) { return Math.round(value * getResources().getDisplayMetrics().density); }
    private void openScanner() {
        if (scanner != null || completed) return;
        try {
            scanner = new RemoteView.Builder().setContext(this).setFormat(HmsScan.QRCODE_SCAN_TYPE).setContinuouslyScan(false).build();
            scanner.setOnResultCallback(results -> {
                if (results != null && results.length > 0 && results[0] != null) {
                    String value = results[0].getOriginalValue();
                    if (value != null && !value.isEmpty()) runOnUiThread(() -> complete("success", value.length() <= 64 ? value : ""));
                }
            });
            scanner.setOnErrorCallback(code -> runOnUiThread(() -> complete("error", "The camera couldn't start. Close other camera apps or enter the room code.")));
            scanner.onCreate(null);
            root.addView(scanner, new FrameLayout.LayoutParams(-1, -1));
            TextView caption = new TextView(this); caption.setText("Scan your host's room QR code");
            caption.setTextColor(Color.WHITE); caption.setTextSize(20); caption.setGravity(Gravity.CENTER); caption.setBackgroundColor(0xB0112631);
            FrameLayout.LayoutParams title = new FrameLayout.LayoutParams(-1, dp(64), Gravity.TOP);root.addView(caption, title);
            LinearLayout controls = new LinearLayout(this);controls.setGravity(Gravity.CENTER);controls.setPadding(dp(20),0,dp(20),0);controls.setBackgroundColor(0xB0112631);
            Button cancel = new Button(this);cancel.setText("Cancel");cancel.setOnClickListener(v -> complete("cancel", ""));
            controls.addView(cancel, new LinearLayout.LayoutParams(dp(160),dp(52)));
            if (getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA_FLASH)) {
                Button flash = new Button(this);flash.setText("Flashlight");flash.setOnClickListener(v -> {try {scanner.switchLight();flash.setText(scanner.getLightStatus()?"Light off":"Flashlight");}catch(Exception ignored){flash.setEnabled(false);}});
                LinearLayout.LayoutParams f = new LinearLayout.LayoutParams(dp(160),dp(52));f.leftMargin=dp(24);controls.addView(flash,f);
            }
            root.addView(controls,new FrameLayout.LayoutParams(-1,dp(72),Gravity.BOTTOM));
            if (started) scanner.onStart();
            if (resumed) scanner.onResume();
        } catch (Exception e) { complete("error", "The scanner couldn't start. You can still enter the room code."); }
    }
    void complete(String status,String value) { if(completed)return;completed=true;RoomScanBridge.finish(status,value);finish(); }
    @Override public void onRequestPermissionsResult(int request,String[] permissions,int[] results) {
        super.onRequestPermissionsResult(request,permissions,results);
        if(request==CAMERA_REQUEST) {
            if(results.length>0&&results[0]==PackageManager.PERMISSION_GRANTED)openScanner();
            else complete("permission","Camera permission is needed to scan. Allow it in Android settings, or enter the room code.");
        }
    }
    @Override public void onBackPressed(){complete("cancel","");}
    @Override protected void onStart(){super.onStart();started=true;if(scanner!=null)scanner.onStart();}
    @Override protected void onResume(){super.onResume();resumed=true;if(scanner!=null)scanner.onResume();}
    @Override protected void onPause(){if(scanner!=null)scanner.onPause();resumed=false;super.onPause();}
    @Override protected void onStop(){if(scanner!=null)scanner.onStop();started=false;super.onStop();}
    @Override protected void onDestroy(){
        if(scanner!=null)scanner.onDestroy();
        if(RoomScanBridge.screen==this)RoomScanBridge.screen=null;
        if(!completed&&!isChangingConfigurations())RoomScanBridge.finish("cancel","");
        super.onDestroy();
    }
}
