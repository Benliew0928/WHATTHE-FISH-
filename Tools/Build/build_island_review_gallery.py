"""Create a static review gallery from actual EXE evidence (never concept images)."""
from pathlib import Path
import html,re
root=Path(__file__).resolve().parents[2]
folder=root/'Docs/VisualDirection/GolfFishingRefinement/GameReview';folder.mkdir(parents=True,exist_ok=True)
captions={
'Overview':'Island layout, routes and shoreline from the Windows player.',
'Rear Cove':'Limestone coast and the course silhouette.',
'Rear Inlet':'Lagoon inlet, bridge and outer coastline.',
'Fairway':'Player-height fairway, turf and long-distance vegetation.',
'Palm Garden':'Close bark relief, planting and layered vegetation.',
'Turf And Bunker':'Fairway-to-sand transition and bunker lip.',
'Pavilion':'Open welcome pavilion, stone bases, timber and approach.',
'Pavilion Joinery':'Close roof supports, hardware and material joints.',
'Practice Green':'Practice surface, cup, flag and route connection.',
'Cascade':'Spring terraces, grounded foundations and moving waterfall.',
'Shoreline':'Grass, sand, stone and shallow-water transitions.',
'Rock Detail':'Limestone geometry and tiling surface detail.',
'Raised Green':'Raised green, flag and open sky.',
'Station Entrance':'Clear approach from the island circuit to a fishing station.',
'Deck Joinery':'Planks, bolts, supports, coloured caps and rope detail.',
'Lagoon Path':'The continuous path around the fishing lagoon.',
'Shelter':'Open shelter, seating, storage and welcome signage.',
'Bridge':'Inlet crossing, rope railings and retaining masonry.',
'Limestone Terraces':'Layered limestone and planted elevations.',
'Water Detail':'Calmer lagoon water, depth colour and shallow effects.',
'Lagoon Sky':'Player-height lagoon view with sky and sculpted clouds.'}
captions.update({'Ground Material Close':'Downward inspection of the tiled surface approximately 1–2 metres from the camera.','Spring Terraces':'Elevated EXE view of the connected spring pools, spillways and grounded rock foundations.'})
css='''*{box-sizing:border-box}html{scroll-behavior:smooth}body{margin:0;background:#f4f0e7;color:#183e42;font:16px/1.6 system-ui,sans-serif}header{padding:54px max(4vw,24px);background:#173e42;color:#fff9e9}header p{max-width:850px;color:#cce0da}h1{font-size:clamp(32px,4vw,54px);line-height:1.1;margin:8px 0 20px}h2{font-size:30px;margin:0}nav{display:flex;gap:12px;flex-wrap:wrap;margin-top:24px}nav a,.button{border:1px solid #729e96;padding:10px 20px;border-radius:30px;color:inherit;text-decoration:none}main{max-width:1600px;margin:auto;padding:40px 24px}section{margin-bottom:70px}.tag{font-size:12px;font-weight:700;letter-spacing:2px;text-transform:uppercase;color:#aad4c8}.intro{max-width:900px}.grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:20px;margin-top:25px}.card{border:1px solid #d9dfd5;border-radius:14px;overflow:hidden;background:#fffdf7}img{width:100%;display:block;aspect-ratio:16/9;object-fit:cover}.caption{padding:17px 21px}.caption h3{margin:0 0 4px;font-size:19px}.caption p{margin:0;color:#55706d;font-size:14px}video{width:100%;aspect-ratio:16/9;background:#142e30;border-radius:14px}.video{max-width:1100px;margin-top:25px}.evidence{padding:24px;background:#e3ece4;border-radius:14px}a{color:#17666b}pre{white-space:pre-wrap;overflow-wrap:anywhere;font-size:13px;line-height:1.65;padding:20px;background:#173e42;color:#e0ece4;border-radius:10px}footer{border-top:1px solid #ccd7cd;padding:20px 0;color:#55706d}@media(max-width:850px){.grid{grid-template-columns:1fr}}'''
parts=[f'<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>G2 + L2 — Windows game review</title><style>{css}</style><header><div class="tag">Actual Windows EXE captures</div><h1>Two islands, ready for a closer look.</h1><p>G2 Limestone Cove Links and L2 Limestone Garden Lagoon. These are game captures, separate from the earlier imagegen concepts. Open any image to inspect the original 1920 × 1080 frame.</p><nav><a href="#golf">G2 Golf</a><a href="#fishing">L2 Fishing</a><a href="#validation">Validation & performance</a><a href="../index.html">Concept references</a></nav></header><main>']
for sport,code,name,description in [('Golf','G2','Limestone Cove Links','A coastal course with planted limestone, clear fairways, a welcome pavilion, practice green and a spring garden.'),('Fishing','L2','Limestone Garden Lagoon','A sheltered lagoon with five independent coloured stations, a continuous circuit, inlet bridge and an open gathering shelter.')]:
    images=sorted(folder.glob(sport+'_??_*.png'));assert len(images)>=12,(sport,len(images))
    parts.append(f'<section id="{sport.lower()}"><h2>{code} · {name}</h2><p class="intro">{description}</p><div class="grid">')
    for p in images:
        label=p.stem.split('_',2)[2].replace('_',' ')
        parts.append(f'<article class="card"><a href="{p.name}" target="_blank"><img src="{p.name}" loading="lazy" alt="{html.escape(sport+": "+label)}"></a><div class="caption"><h3>{html.escape(label)}</h3><p>{captions.get(label,"Actual Windows player view.")}</p></div></article>')
    parts.append('</div>')
    movie=sport+'_Walkthrough.mp4'
    if (folder/movie).exists():parts.append(f'<div class="video"><h3>Continuous route walkthrough</h3><p>The existing player controller follows connected waypoints; no teleports between the recorded route points.</p><video controls preload="metadata" poster="{sport}_01_Overview.png"><source src="{movie}" type="video/mp4"></video></div>')
    parts.append('</section>')
video_link=' · <a href="video-metadata.json">Video verification</a>' if (folder/'video-metadata.json').exists() else ''
parts.append('<section id="validation"><h2>Validation & performance</h2><div class="evidence"><p><a href="VALIDATION.md">Read the validation report</a> · <a href="review.txt">Offline route results</a>'+video_link+' · <a href="../../../../ArtSource/RefinedIslands/README.md">Editable sources and rebuild guide</a></p><p>Windows controls: WASD to move, Shift to run, right mouse drag to look, C to change camera. Golf and fishing retain their existing capacities and menu names. This pass adds no sport mechanics, swimming or NPCs.</p></div>')
if (folder/'performance.txt').exists():parts.append('<pre>'+html.escape((folder/'performance.txt').read_text())+'</pre>')
parts.append('</section><footer>WHATTHE FISH? · G2/L2 environment refinement · Game evidence, not concept renders.</footer></main></html>')
(folder/'index.html').write_text('\n'.join(parts),encoding='utf-8')
print(folder/'index.html')
