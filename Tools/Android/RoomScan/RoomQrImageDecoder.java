package com.umpsa.whatthefish.scan;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import com.huawei.hms.hmsscankit.ScanUtil;
import com.huawei.hms.ml.scan.HmsScan;
import com.huawei.hms.ml.scan.HmsScanAnalyzerOptions;
import java.io.FilterInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.HashSet;
import java.util.Set;

final class RoomQrImageDecoder {
    static final class ImageException extends Exception {
        ImageException(String message){super(message);}
    }
    // Bound both compressed input and decoded bitmap memory; never retain the user's image.
    private static final long MAX_BYTES=24L*1024*1024;
    static String decode(Context context,Uri uri) throws Exception {
        BitmapFactory.Options bounds=new BitmapFactory.Options();bounds.inJustDecodeBounds=true;
        try(InputStream input=open(context,uri)){BitmapFactory.decodeStream(input,null,bounds);}
        if(bounds.outWidth<=0||bounds.outHeight<=0)throw new ImageException("That file isn't a readable image. Choose a QR screenshot or photo.");
        BitmapFactory.Options options=new BitmapFactory.Options();options.inSampleSize=sampleSize(bounds.outWidth,bounds.outHeight);
        options.inPreferredConfig=Bitmap.Config.ARGB_8888;
        Bitmap bitmap;
        try(InputStream input=open(context,uri)){bitmap=BitmapFactory.decodeStream(input,null,options);}
        if(bitmap==null)throw new ImageException("That image couldn't be read. Try a smaller QR screenshot.");
        try {
            ScanUtil.setOperationTrackingEnabled(false);
            HmsScanAnalyzerOptions scanOptions=new HmsScanAnalyzerOptions.Creator()
                .setHmsScanTypes(HmsScan.QRCODE_SCAN_TYPE).setPhotoMode(true).create();
            HmsScan[] results=ScanUtil.decodeWithBitmap(context,bitmap,scanOptions);
            String[] values=new String[results==null?0:results.length];
            for(int i=0;i<values.length;i++)if(results[i]!=null)values[i]=results[i].getOriginalValue();
            return selectInvitation(values);
        } finally {bitmap.recycle();}
    }
    static int sampleSize(int width,int height) throws ImageException {
        if(width<=0||height<=0)throw new ImageException("That file isn't a readable image. Choose a QR screenshot or photo.");
        int sample=1;
        while((long)width/sample>2048||(long)height/sample>2048)sample*=2;
        return sample;
    }
    static String selectInvitation(String[] values) throws ImageException {
        Set<String> invitations=new HashSet<>();
        if(values!=null)for(String value:values)
            if(value!=null&&value.length()<=22&&value.matches("WTFROOM:1:[A-Z0-9]{4,12}"))invitations.add(value);
        if(invitations.size()>1)throw new ImageException("This image has several room invitations. Crop it to just the room you want.");
        if(invitations.isEmpty())throw new ImageException("No valid room QR found. Choose a clear image of your host's room QR code.");
        return invitations.iterator().next(); // Unity validates the invitation again before filling the field.
    }
    private static InputStream open(Context context,Uri uri) throws IOException {
        InputStream source=context.getContentResolver().openInputStream(uri);
        if(source==null)throw new IOException("Image unavailable");
        return new FilterInputStream(source) {
            long consumed;
            private void count(long amount) throws IOException {if(amount>0&&(consumed+=amount)>MAX_BYTES)throw new IOException("Image too large");}
            @Override public int read() throws IOException {int value=in.read();if(value>=0)count(1);return value;}
            @Override public int read(byte[] buffer,int offset,int length) throws IOException {int count=in.read(buffer,offset,length);count(count);return count;}
            @Override public long skip(long amount) throws IOException {long skipped=in.skip(Math.min(amount,MAX_BYTES-consumed+1));count(skipped);return skipped;}
        };
    }
}
