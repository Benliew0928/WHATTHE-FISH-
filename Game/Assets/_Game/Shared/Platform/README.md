# Platform adapters
Implement IPlatformGameServices for the selected Huawei SDK after project registration. The current placeholder reports Available=false and submits no achievements or results.

Guest identity is implemented by `Shared/Core/PlayerIdentity.cs`. Huawei authentication and guest-to-account linking remain pending AppGallery Connect setup and verification of the UGS integration route. See [player login requirements](../../../../../Docs/PLAYER-LOGIN.md). Authentication should have its own adapter; the existing result-reporting interface does not implement sign-in.
