using System;
using System.Collections;
using UnityEngine;

namespace WhatTheFish {
 public static class CoveClipboard {
  // The Windows clipboard can be briefly owned by another application. Retry
  // the user's explicit copy request, and never announce success without it.
  public static IEnumerator Copy(string value,Action<bool> finished) {
   for(int attempt=0;attempt<10;attempt++){
    GUIUtility.systemCopyBuffer=value;
    yield return new WaitForSecondsRealtime(.04f);
    if(GUIUtility.systemCopyBuffer==value){finished(true);yield break;}
   }
   finished(false);
  }
 }
}
