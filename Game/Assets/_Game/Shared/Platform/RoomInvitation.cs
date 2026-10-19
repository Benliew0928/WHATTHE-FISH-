using System;

namespace WhatTheFish {
 // A room invitation contains only a shareable session code. Admission remains server-authoritative.
 public static class RoomInvitation {
  public const string Prefix="WTFROOM:1:";
  public static string Encode(string code) {
   code=StorybookMenu.NormalizeCode(code);
   if(!StorybookMenu.ValidCode(code))throw new ArgumentException("A live room code is required.",nameof(code));
   return Prefix+code;
  }
  public static bool TryDecode(string payload,out string code,out string error) {
   code=null;error="That QR isn't a WHATTHE FISH? room invitation.";
   if(string.IsNullOrEmpty(payload)||payload.Length>64)return false;
   if(!payload.StartsWith(Prefix,StringComparison.Ordinal)) {
    if(payload.StartsWith("WTFROOM:",StringComparison.Ordinal))error="This invitation needs a different game version. Ask your host to update.";
    return false;
   }
   var value=payload.Substring(Prefix.Length);
   if(!StorybookMenu.ValidCode(value)||value!=StorybookMenu.NormalizeCode(value)){error="This room invitation is damaged. Try scanning again or enter the code.";return false;}
   code=value;error=null;return true;
  }
 }
}
