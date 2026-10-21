package com.umpsa.whatthefish.profiles;

import android.Manifest;
import android.app.Activity;
import android.content.ClipData;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.*;
import android.media.ExifInterface;
import android.net.Uri;
import android.os.Bundle;
import android.provider.MediaStore;
import android.util.Base64;
import android.view.*;
import android.widget.*;
import androidx.core.content.FileProvider;
import java.io.*;

// Only the user-selected image is read. Originals, EXIF and location never leave the phone.
public final class PhotoActivity extends Activity {
    private String id; private File capture; private Bitmap bitmap; private Crop crop; private boolean choosing;
    @Override public void onCreate(Bundle state) {
        super.onCreate(state);id=getIntent().getStringExtra("id");
        if(id==null||!PhotoBridge.valid(id)||state!=null){finishWith("cancelled","");return;}
        getWindow().setFlags(WindowManager.LayoutParams.FLAG_SECURE,WindowManager.LayoutParams.FLAG_SECURE);
        if(getIntent().getBooleanExtra("camera",false)) {
            if(checkSelfPermission(Manifest.permission.CAMERA)!=PackageManager.PERMISSION_GRANTED)
                requestPermissions(new String[]{Manifest.permission.CAMERA},3);
            else camera();
        } else gallery();
    }
    @Override public void onRequestPermissionsResult(int code,String[] names,int[] grants) {
        super.onRequestPermissionsResult(code,names,grants);
        if(code==3&&grants.length>0&&grants[0]==PackageManager.PERMISSION_GRANTED)camera();
        else finishWith("permission","");
    }
    private void camera() {
        try {
            File dir=new File(getCacheDir(),"profile-capture");if(!dir.exists()&&!dir.mkdirs())throw new IOException();
            File[] old=dir.listFiles();if(old!=null)for(File file:old)if(file.isFile()&&file.lastModified()<System.currentTimeMillis()-86400000L)file.delete();
            capture=File.createTempFile("photo-",".jpg",dir);
            Uri uri=FileProvider.getUriForFile(this,getPackageName()+".profilephotos",capture);
            Intent intent=new Intent(MediaStore.ACTION_IMAGE_CAPTURE).putExtra(MediaStore.EXTRA_OUTPUT,uri);
            intent.setClipData(ClipData.newRawUri("Profile photo",uri));
            intent.addFlags(Intent.FLAG_GRANT_WRITE_URI_PERMISSION|Intent.FLAG_GRANT_READ_URI_PERMISSION);
            choosing=true;startActivityForResult(intent,1);
        }catch(Exception e){finishWith("camera_unavailable","");}
    }
    private void gallery() {
        try {
            Intent intent=new Intent(Intent.ACTION_OPEN_DOCUMENT).setType("image/*").addCategory(Intent.CATEGORY_OPENABLE);
            choosing=true;startActivityForResult(intent,2);
        }catch(Exception e){finishWith("gallery_unavailable","");}
    }
    @Override protected void onActivityResult(int code,int result,Intent data) {
        super.onActivityResult(code,result,data);choosing=false;
        if(result!=RESULT_OK){finishWith("cancelled","");return;}
        Uri uri=code==1&&capture!=null?Uri.fromFile(capture):data==null?null:data.getData();
        if(uri==null){finishWith("invalid_image","");return;}
        TextView loading=new TextView(this);loading.setText("Preparing your picture…");loading.setGravity(Gravity.CENTER);setContentView(loading);
        new Thread(() -> {
            try {
                byte[] bytes;
                try(InputStream stream=getContentResolver().openInputStream(uri);ByteArrayOutputStream output=new ByteArrayOutputStream()){
                    byte[] block=new byte[8192];int n,total=0;
                    while((n=stream.read(block))!=-1){total+=n;if(total>20*1024*1024)throw new IOException("Image too large");output.write(block,0,n);}
                    bytes=output.toByteArray();
                }
                BitmapFactory.Options bounds=new BitmapFactory.Options();bounds.inJustDecodeBounds=true;
                BitmapFactory.decodeByteArray(bytes,0,bytes.length,bounds);
                if(bounds.outWidth<1||bounds.outHeight<1||bounds.outWidth>50000||bounds.outHeight>50000)throw new IOException();
                BitmapFactory.Options options=new BitmapFactory.Options();options.inSampleSize=1;
                while(Math.max(bounds.outWidth,bounds.outHeight)/options.inSampleSize>1024)options.inSampleSize*=2;
                Bitmap decoded=BitmapFactory.decodeByteArray(bytes,0,bytes.length,options);if(decoded==null)throw new IOException();
                int orientation=ExifInterface.ORIENTATION_NORMAL;
                try {orientation=new ExifInterface(new ByteArrayInputStream(bytes)).getAttributeInt(ExifInterface.TAG_ORIENTATION,1);}catch(IOException ignored){}
                Matrix m=new Matrix();
                switch(orientation){
                    case 2:m.setScale(-1,1);break; case 3:m.setRotate(180);break;case 4:m.setScale(1,-1);break;
                    case 5:m.setRotate(90);m.postScale(-1,1);break;case 6:m.setRotate(90);break;
                    case 7:m.setRotate(270);m.postScale(-1,1);break;case 8:m.setRotate(270);break;
                }
                Bitmap corrected=Bitmap.createBitmap(decoded,0,0,decoded.getWidth(),decoded.getHeight(),m,true);
                if(corrected!=decoded)decoded.recycle();
                runOnUiThread(()->{if(isFinishing()||!PhotoBridge.valid(id)){corrected.recycle();return;}bitmap=corrected;showCrop();});
            }catch(Exception|OutOfMemoryError e){runOnUiThread(()->finishWith("invalid_image",""));}
        },"Profile photo decode").start();
    }
    private void showCrop() {
        LinearLayout root=new LinearLayout(this);root.setOrientation(LinearLayout.VERTICAL);root.setPadding(20,20,20,20);root.setBackgroundColor(Color.rgb(241,247,242));
        root.setOnApplyWindowInsetsListener((view,insets)->{view.setPadding(insets.getSystemWindowInsetLeft()+20,insets.getSystemWindowInsetTop()+20,insets.getSystemWindowInsetRight()+20,insets.getSystemWindowInsetBottom()+20);return insets;});
        TextView title=new TextView(this);title.setText("Profile picture\nDrag to position · adjust zoom");title.setTextSize(20);title.setTextColor(Color.rgb(16,50,59));root.addView(title);
        crop=new Crop();root.addView(crop,new LinearLayout.LayoutParams(-1,0,1));
        SeekBar zoom=new SeekBar(this);zoom.setMax(200);zoom.setContentDescription("Photo zoom");
        zoom.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener(){
            public void onProgressChanged(SeekBar s,int value,boolean user){crop.zoom=1+value/100f;crop.invalidate();}
            public void onStartTrackingTouch(SeekBar s){}public void onStopTrackingTouch(SeekBar s){}
        });root.addView(zoom);
        LinearLayout actions=new LinearLayout(this);
        Button cancel=new Button(this);cancel.setText("Cancel");cancel.setOnClickListener(v->finishWith("cancelled",""));actions.addView(cancel,new LinearLayout.LayoutParams(0,-2,1));
        Button use=new Button(this);use.setText("Use picture");use.setOnClickListener(v->save());actions.addView(use,new LinearLayout.LayoutParams(0,-2,1));
        root.addView(actions);setContentView(root);root.requestApplyInsets();
    }
    private void save() {
        if(crop==null||bitmap==null)return;
        Bitmap small=Bitmap.createBitmap(128,128,Bitmap.Config.ARGB_8888);Canvas canvas=new Canvas(small);canvas.drawColor(Color.WHITE);
        canvas.drawBitmap(bitmap,crop.source(),new RectF(0,0,128,128),new Paint(Paint.FILTER_BITMAP_FLAG));
        int[] pixels=new int[16384];small.getPixels(pixels,0,128,0,0,128,128);small.recycle();
        byte[] rgb=new byte[49152];for(int i=0;i<pixels.length;i++){rgb[i*3]=(byte)(pixels[i]>>16);rgb[i*3+1]=(byte)(pixels[i]>>8);rgb[i*3+2]=(byte)pixels[i];}
        finishWith("",Base64.encodeToString(rgb,Base64.NO_WRAP));
    }
    private void finishWith(String error,String photo){if(id!=null)PhotoBridge.complete(id,error,photo);finish();}
    @Override public void onBackPressed(){finishWith("cancelled","");}
    @Override protected void onDestroy(){if(bitmap!=null)bitmap.recycle();if(capture!=null)capture.delete();if(isFinishing()&&!choosing)PhotoBridge.complete(id==null?"":id,"cancelled","");super.onDestroy();}
    private final class Crop extends View {
        float zoom=1,cx=.5f,cy=.5f,lastX,lastY; final Paint paint=new Paint(Paint.FILTER_BITMAP_FLAG);
        Crop(){super(PhotoActivity.this);setContentDescription("Drag photo to crop");}
        Rect source(){float side=Math.min(bitmap.getWidth(),bitmap.getHeight())/zoom;cx=Math.max(side/2/bitmap.getWidth(),Math.min(1-side/2/bitmap.getWidth(),cx));cy=Math.max(side/2/bitmap.getHeight(),Math.min(1-side/2/bitmap.getHeight(),cy));int left=Math.round(cx*bitmap.getWidth()-side/2),top=Math.round(cy*bitmap.getHeight()-side/2);return new Rect(left,top,left+Math.max(1,(int)side),top+Math.max(1,(int)side));}
        protected void onDraw(Canvas canvas){super.onDraw(canvas);float side=Math.min(getWidth(),getHeight());canvas.drawBitmap(bitmap,source(),new RectF((getWidth()-side)/2,(getHeight()-side)/2,(getWidth()+side)/2,(getHeight()+side)/2),paint);}
        public boolean onTouchEvent(MotionEvent e){if(e.getAction()==MotionEvent.ACTION_DOWN){lastX=e.getX();lastY=e.getY();return true;}if(e.getAction()==MotionEvent.ACTION_MOVE){float side=Math.max(1,Math.min(getWidth(),getHeight()));float src=Math.min(bitmap.getWidth(),bitmap.getHeight())/zoom;cx-=(e.getX()-lastX)/side*src/bitmap.getWidth();cy-=(e.getY()-lastY)/side*src/bitmap.getHeight();lastX=e.getX();lastY=e.getY();invalidate();return true;}return true;}
    }
}
