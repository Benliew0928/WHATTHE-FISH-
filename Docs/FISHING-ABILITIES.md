# Fishing skills, shore bags and pier treasure

Choose **two favourites from six skills** before pressing Ready. Open
**CHOOSE 2 SKILLS**, select two cards and save. Unready to change your kit.
The kit stays with you across rematches during this island visit. Carry
**one item** at a time; an active effect must finish before taking another.

Walk near a shore supply and tap its **bag icon**, then pick one of your two
skills. The host accepts the first valid request. That bag has a shared
**22-second refill**, visible on its button and in the world. Items are never
collected automatically or swapped over another held item.

Alternatively, tap the **treasure chest** at a pier. Treasure first becomes
available 12 seconds into play. Choose one of your two skills, then spend
**six seconds** recovering it with the rod. Walking and jumping pause during
recovery. Cancel gives up the item. All piers share one treasure opportunity;
beginning recovery starts a **28-second cooldown**. Treasure gives gear, not
points, and costs time you could spend fishing.

| Skill | Effect | When to tap its icon |
| --- | --- | --- |
| Sonar | Available Wanted fish gain gold target rings for you and friends within 12 metres, for 8 seconds | Any phase except treasure recovery |
| Cool Spool | 45% less positive strain buildup for 4 seconds | While fighting a fish |
| Flippers | Move 30% faster for 5 seconds | While idle |
| Power Reel | Wind 28% faster, with 35% more positive strain for 4 seconds | While fighting a fish |
| Bubble Guard | Reduce strain by 0.22 at the next strong surge within 12 seconds | While fighting a fish |
| Quick Lure | One cast within 12 seconds gets its bite 0.8 seconds sooner | While idle |

Tap the held **skill icon** to activate it. An unavailable icon dims until
its required phase. The duration bar and seconds show the remaining effect;
Quick Lure and Bubble Guard can end early when consumed. Sonar recipients
come from the host's nearby-player query. Refreshing sonar extends expiry;
it does not multiply the effect. Reduced motion retains the cues while
suppressing bobs and button pops. Skills never automatically catch a fish,
award points, or steal another player's claimed fish.

The host owns kits, inventory, cooldowns, treasure reservations, recipients
and expiry. Both peers need the current **protocol 70**; the three removed abilities
are rejected rather than silently mapped to another skill. Match endings,
disconnects and island travel clear the active fishing state.

## First visit and How to Play

The first visit opens a four-page tutorial: **Choose 2 Skills → Cast & Catch →
Use Your Gear → Play to Win**. Next and Back change pages; Skip/Done closes
the card so you can walk. The **?** on Fishing's leaderboard reopens it while
idle. Fishing's **How to Play** uses the exact same component, pictures and
copy in [FishingTutorialUI.cs](../Game/Assets/_Game/Shared/UI/FishingTutorialUI.cs).

The tutorial uses the live skill icons, mirrored Left/Right circles, timing
circle, Catch/Tension bars and gear duration meter. Its supporting
illustrations are small compressed atlases. Sonar shares the sports radar;
Power Reel shares Fishing's rod/reel image. Source art and reproducible
delivery hashes are in [the learning art folder](../ArtSource/Fishing/UI/Learning/).
Football/Basketball meshes, materials and fonts remain shared.

Round is highest score after three minutes. Cup combines three rounds.
Crew needs a **shared basket worth 20 × player count**, all three fish sizes
and at least one catch from every player, within the same three minutes.
Small/medium/large score 2/5/10; Wanted adds +3 once per minute per player.
See [Fishing rules](FISHING-MINIGAME.md) and
[verification limits](FISHING-VERIFICATION.md).

The current [Hold the Glow controls](FISHING-COUNTERSTEER.md) restore continuous holding;
gear choices and acquisition rules above remain. The shared tutorial uses the
live fish/gold-band meter and the two Left/Right hold controls without keyboard captions.
