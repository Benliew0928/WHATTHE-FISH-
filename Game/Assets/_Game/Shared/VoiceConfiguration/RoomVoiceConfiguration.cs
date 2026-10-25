using Unity.Services.Core;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Internal;

namespace WhatTheFish {
 // The pinned UGS SDK keeps project configuration in a separately referenced
 // assembly. Read only the public connection fields, never token-signing keys.
 public static class RoomVoiceConfiguration {
  public static bool Configured {
   get {
    if(UnityServices.State!=ServicesInitializationState.Initialized||CoreRegistry.Instance==null||!CoreRegistry.Instance.TryGetServiceComponent<IProjectConfiguration>(out var config))return false;
    return !string.IsNullOrWhiteSpace(config.GetString("com.unity.services.vivox.server"))&&!string.IsNullOrWhiteSpace(config.GetString("com.unity.services.vivox.domain"))&&!string.IsNullOrWhiteSpace(config.GetString("com.unity.services.vivox.issuer"));
   }
  }
 }
}
