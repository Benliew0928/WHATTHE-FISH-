// Requires Node.js and sharp (install outside the repository or use NODE_PATH).
// The approved, editable SVG masters are the only source of the shipped atlas.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const sharp = require('sharp');
const root = path.resolve(__dirname, '../..');
const names = ['ball','block','book','close','court','exit','eye','follow','gear','guard','jump','move','pass','points','shoot','steal'];
(async () => {
  const output = path.join(root, 'Game/Assets/_Game/Resources/BasketballUI');
  fs.mkdirSync(output, { recursive: true });
  const layers = [], records = [];
  for (const [i,name] of names.entries()) {
    const relative = `Docs/BasketballUI/icons/${name}.svg`;
    const source = fs.readFileSync(path.join(root, relative));
    // Four transparent pixels around each cell prevent neighbouring icon bleed.
    const input = await sharp(source, { density: 192 }).resize(120,120,{fit:'contain',background:'#00000000'}).png().toBuffer();
    layers.push({input,left:i%4*128+4,top:Math.floor(i/4)*128+4});
    records.push({name,source:relative,sha256:crypto.createHash('sha256').update(source).digest('hex'),cell:i});
  }
  await sharp({create:{width:512,height:512,channels:4,background:'#00000000'}}).composite(layers).png().toFile(path.join(output,'Icons.png'));
  fs.writeFileSync(path.join(root,'Docs/BasketballUI/atlas.json'),JSON.stringify({base:'repository root',generator:'Tools/Build/Generate-BasketballUI.cjs',tile:128,padding:4,icons:records},null,2)+'\n');
  console.log('BASKETBALL_UI_ATLAS_OK: 16 approved icons, 512 x 512');
})();
