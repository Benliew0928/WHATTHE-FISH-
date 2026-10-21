package com.umpsa.whatthefish.scan;

// JVM checks for ambiguous invitations and bitmap memory bounds, not camera/SDK runtime tests.
public final class RoomQrImageDecoderTest {
    static int checks;
    static void check(boolean condition,String name){if(!condition)throw new AssertionError(name);checks++;System.out.println("PASS "+name);}
    static void rejects(String[] values,String message) throws Exception {
        try {RoomQrImageDecoder.selectInvitation(values);throw new AssertionError("Accepted invalid/ambiguous invitation");}
        catch(RoomQrImageDecoder.ImageException e){check(e.getMessage().contains(message),message);}
    }
    public static void main(String[] args) throws Exception {
        String room="WTFROOM:1:WAVE24";
        check(room.equals(RoomQrImageDecoder.selectInvitation(new String[]{room})),"valid room screenshot");
        check(room.equals(RoomQrImageDecoder.selectInvitation(new String[]{null,"https://example.com",room,room})),"ignore foreign codes and deduplicate same room");
        check("WTFROOM:1:123456789012".equals(RoomQrImageDecoder.selectInvitation(new String[]{"WTFROOM:1:123456789012"})),"maximum room code length");
        rejects(new String[]{room,"WTFROOM:1:ABCD"},"several room");
        rejects(null,"No valid");rejects(new String[0],"No valid");
        for(String invalid:new String[]{"","WTFROOM:2:ABCD","WTFROOM:1:abcD","WTFROOM:1:ABC","WTFROOM:1:1234567890123","WTFROOM:1:ABCD\n","WTFROOM:1:ＡＢＣＤ","WTFROOM:1:ABCD:SECRET","ABCD"})
            rejects(new String[]{invalid},"No valid");
        check(RoomQrImageDecoder.sampleSize(512,512)==1,"keep QR screenshot detail");
        check(RoomQrImageDecoder.sampleSize(8000,6000)==4,"bound large landscape photo");
        check(RoomQrImageDecoder.sampleSize(6000,8000)==4,"bound large portrait photo");
        int sample=RoomQrImageDecoder.sampleSize(Integer.MAX_VALUE,Integer.MAX_VALUE);
        check((long)Integer.MAX_VALUE/sample<=2048&&sample>0,"malicious dimensions do not overflow");
        try {RoomQrImageDecoder.sampleSize(-1,0);throw new AssertionError("Accepted invalid dimensions");}
        catch(RoomQrImageDecoder.ImageException e){check(true,"reject invalid image dimensions");}
        System.out.println("ROOM_QR_IMAGE_POLICY_PASS checks="+checks);
    }
}
