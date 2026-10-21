package com.umpsa.whatthefish.scan;

import android.Manifest;
import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.Color;
import android.net.Uri;
import android.os.Bundle;
import android.view.Gravity;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.huawei.hms.hmsscankit.RemoteView;
import com.huawei.hms.ml.scan.HmsScan;

// Camera and selected-image scanning. The picker grants access only to its chosen URI.
public final class RoomScanActivity extends Activity {
    private RemoteView scanner;
    private FrameLayout root;
    private boolean completed, started, resumed, picking, decoding;
    private TextView caption;
    private static final int CAMERA_REQUEST = 71;
    private static final int IMAGE_REQUEST = 72;
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state);
        if (!RoomScanBridge.isActive()) { finish(); return; }
        RoomScanBridge.screen = this;
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        root = new FrameLayout(this); root.setBackgroundColor(Color.BLACK); setContentView(root);
        if (getIntent().getBooleanExtra("fromImage", false)) {
            showControls("Choose an image containing your host's room QR code");
            if(state == null) chooseImage();
        } else if (checkSelfPermission(Manifest.permission.CAMERA) == PackageManager.PERMISSION_GRANTED) openScanner();
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
                    if (value != null && !value.isEmpty()) runOnUiThread(() -> {if(!picking&&!decoding)complete("success", value.length() <= 64 ? value : "");});
                }
            });
            scanner.setOnErrorCallback(code -> runOnUiThread(() -> {if(!picking&&!decoding)complete("error", "The camera couldn't start. Choose a QR image or enter the room code.");}));
            scanner.onCreate(null);
            root.addView(scanner, new FrameLayout.LayoutParams(-1, -1));
            showControls("Scan your host's room QR code");
            if (started) scanner.onStart();
            if (resumed) scanner.onResume();
        } catch (Exception e) { complete("error", "The scanner couldn't start. You can still choose a QR image or enter the room code."); }
    }
    private void showControls(String text) {
            caption = new TextView(this); caption.setText(text);
            caption.setTextColor(Color.WHITE); caption.setTextSize(20); caption.setGravity(Gravity.CENTER); caption.setBackgroundColor(0xB0112631);
            FrameLayout.LayoutParams title = new FrameLayout.LayoutParams(-1, dp(64), Gravity.TOP);root.addView(caption, title);
            LinearLayout controls = new LinearLayout(this);controls.setGravity(Gravity.CENTER);controls.setPadding(dp(20),0,dp(20),0);controls.setBackgroundColor(0xB0112631);
            Button cancel = new Button(this);cancel.setText("Cancel");cancel.setOnClickListener(v -> complete("cancel", ""));
            controls.addView(cancel, new LinearLayout.LayoutParams(dp(160),dp(52)));
            Button image = new Button(this);image.setText("Choose QR image");image.setOnClickListener(v -> chooseImage());
            LinearLayout.LayoutParams pick = new LinearLayout.LayoutParams(dp(180),dp(52));pick.leftMargin=dp(12);controls.addView(image,pick);
            if (scanner != null && getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA_FLASH)) {
                Button flash = new Button(this);flash.setText("Flashlight");flash.setOnClickListener(v -> {try {scanner.switchLight();flash.setText(scanner.getLightStatus()?"Light off":"Flashlight");}catch(Exception ignored){flash.setEnabled(false);}});
                LinearLayout.LayoutParams f = new LinearLayout.LayoutParams(dp(160),dp(52));f.leftMargin=dp(24);controls.addView(flash,f);
            }
            root.addView(controls,new FrameLayout.LayoutParams(-1,dp(72),Gravity.BOTTOM));
    }
    private void chooseImage() {
        if(completed||picking||decoding)return;
        picking=true;
        try {
            // ACTION_OPEN_DOCUMENT works from API 19, including Huawei devices without GMS.
            Intent intent=new Intent(Intent.ACTION_OPEN_DOCUMENT).addCategory(Intent.CATEGORY_OPENABLE).setType("image/*");
            intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            startActivityForResult(intent, IMAGE_REQUEST);
        } catch(Exception e) {picking=false;complete("error","The image picker couldn't open. Try camera scanning or enter the room code.");}
    }
    @Override protected void onActivityResult(int request,int result,Intent data) {
        super.onActivityResult(request,result,data);
        if(request!=IMAGE_REQUEST||completed)return;
        picking=false;
        if(result!=RESULT_OK||data==null||data.getData()==null){complete("cancel","");return;}
        Uri uri=data.getData();
        if(!"content".equals(uri.getScheme())){complete("error","Choose an image from the phone's file picker.");return;}
        decoding=true;caption.setText("Reading QR image…");
        new Thread(() -> {
            String status="success",value;
            try {value=RoomQrImageDecoder.decode(getApplicationContext(),uri);}
            catch(RoomQrImageDecoder.ImageException e){status="error";value=e.getMessage();}
            catch(Exception | OutOfMemoryError e){status="error";value="That image couldn't be read. Choose another image or enter the room code.";}
            final String outcome=status, message=value;
            runOnUiThread(() -> {decoding=false;if(!isDestroyed()&&!completed)complete(outcome,message);});
        },"Room QR image").start();
    }
    void complete(String status,String value) { if(completed)return;completed=true;RoomScanBridge.finish(status,value);finish(); }
    @Override public void onRequestPermissionsResult(int request,String[] permissions,int[] results) {
        super.onRequestPermissionsResult(request,permissions,results);
        if(request==CAMERA_REQUEST) {
            if(results.length>0&&results[0]==PackageManager.PERMISSION_GRANTED)openScanner();
            else complete("permission","Camera access was denied. Choose a QR image, enter the code, or allow camera access in Android settings.");
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
