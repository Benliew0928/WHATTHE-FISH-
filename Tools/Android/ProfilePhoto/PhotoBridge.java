package com.umpsa.whatthefish.profiles;

import android.app.Activity;
import android.content.Intent;
import org.json.JSONObject;

public final class PhotoBridge {
    private static String active="", result="";
    private PhotoBridge() {}
    public static synchronized void start(Activity activity, String id, boolean camera) {
        if (!active.isEmpty()) throw new IllegalStateException("Photo picker already open");
        active=id;result="";
        activity.runOnUiThread(() -> {
            try { activity.startActivity(new Intent(activity, PhotoActivity.class).putExtra("id",id).putExtra("camera",camera)); }
            catch (Exception e) { complete(id,"unavailable",""); }
        });
    }
    static synchronized boolean valid(String id) { return !id.isEmpty() && id.equals(active); }
    static synchronized void complete(String id,String error,String photo) {
        if (!valid(id)) return;
        try { result=new JSONObject().put("error",error).put("photo",photo).toString(); }
        catch(Exception e) { result="{\"error\":\"unavailable\"}"; }
        active="";
    }
    public static synchronized String poll() { String value=result;result="";return value; }
    public static synchronized void cancel(String id) { if(valid(id)){active="";result="";} }
}
