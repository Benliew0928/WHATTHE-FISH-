// One production packing path for painted gameplay art and editable utility SVGs.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..');
const painted={ball:'basketball',shoot:'basketball',block:'block',guard:'guard',jump:'jump',pass:'pass',steal:'steal','soccer-ball':'football','sneaker-move':'dash','shield-chevron':'pressure',sneaker:'tackle'};
const groups={
 basketball:{names:['ball','block','book','close','court','exit','eye','follow','gear','guard','jump','move','pass','points','shoot','steal'],height:512,output:'BasketballUI',record:'Docs/BasketballUI/atlas.json'},
 football:{names:['soccer-ball','sneaker-move','shield-chevron','sneaker','hand-swipe-left','target','flag-checkered','users-three'],height:256,output:'FootballUI',record:'Docs/FootballUI/atlas.json'},
 props:{names:['lock-on','turbo','portal','check'],height:128,output:'SportsUI',record:'Docs/SportsUI/props-atlas.json'}
};
async function build(group){
 const spec=groups[group],layers=[],records=[];
 for(const [cell,name] of spec.names.entries()){
  const relative=painted[name]?`ArtSource/SportsUI/Painted/${painted[name]}.png`:`Docs/SportsUI/color-icons/${name}.svg`;
  const source=fs.readFileSync(path.join(root,relative));
  // Preserve the generated alpha, keep the illustration's complete silhouette,
  // and pack in the existing cells. Full-quality authoring masters stay outside Unity.
  const input=await sharp(source,{density:192}).resize(120,120,{fit:'contain',background:'#00000000'}).png().toBuffer();
  layers.push({input,left:cell%4*128+4,top:Math.floor(cell/4)*128+4});
  records.push({name,source:relative,sha256:crypto.createHash('sha256').update(source).digest('hex'),cell});
 }
 const output=path.join(root,`Game/Assets/_Game/Resources/${spec.output}`);fs.mkdirSync(output,{recursive:true});
 await sharp({create:{width:512,height:spec.height,channels:4,background:'#00000000'}}).composite(layers).png().toFile(path.join(output,group==='props'?'Props.png':'Icons.png'));
 fs.writeFileSync(path.join(root,spec.record),JSON.stringify({base:'repository root',generator:'Tools/Build/Generate-SportsUI.cjs',width:512,height:spec.height,tile:128,padding:4,style:'painted gameplay art and coloured utility illustrations',icons:records},null,2)+'\n');
 console.log(`SPORTS_ATLAS_OK: ${group}, ${spec.names.length} icons, 512 x ${spec.height}`);
}
module.exports={build};
