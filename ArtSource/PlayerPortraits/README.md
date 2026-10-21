# Player character portraits

These are the ten original character illustrations recovered from the local image-generation output saved on 15 October 2026. They are the approved colourful blue-uniform roster, numbered 01–10. No replacement artwork was generated for the player hub redesign.

The 1024 × 1536 RGBA master PNGs retain their original bytes. The game copies live in `../../Game/Assets/_Game/Resources/PlayerPortraits/`, with matching filenames without `-master`. `integrity.json` records repository-relative paths and SHA-256 hashes. Keep both masters and delivery copies in Git LFS.

Avatar indices 0–9 correspond to characters 01–10. Existing indices 0–5 now display the first six characters; custom profile photos remain unchanged. The blue uniform numbers are part of the original artwork, not player IDs.

The player card shows the entire illustration. Square tiles, room seats and gameplay nameplates use a square face crop via Unity UV coordinates, preserving the source image and the original hair silhouettes. Camera/gallery photos use the whole square thumbnail.

Unity imports these textures without mipmaps, with clamp wrapping and Android ASTC 4×4 compression. Full-resolution masters are never compressed or overwritten. To restore the build copies, run `python Tools/Art/Sync-PlayerPortraits.py` from any directory using the script's actual checkout location.
