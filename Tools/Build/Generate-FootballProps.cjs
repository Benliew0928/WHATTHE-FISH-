// npm package: sharp. Resolve it through normal Node resolution or NODE_PATH.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..');
async function main(){
 const layers=[],records=[];
 for(const [cell,name] of ['Turbo','Portal','Wall'].entries()){
  const relative=`ArtSource/Football/Props/${name}.svg`,source=fs.readFileSync(path.join(root,relative));
  layers.push({input:await sharp(source,{density:192}).resize(120,120).png().toBuffer(),left:cell*128+4,top:4});
  records.push({name,cell,source:relative,sha256:crypto.createHash('sha256').update(source).digest('hex')});
 }
 const output=path.join(root,'Game/Assets/_Game/Resources/FootballProps');fs.mkdirSync(output,{recursive:true});
 await sharp({create:{width:512,height:128,channels:4,background:'#00000000'}}).composite(layers).png().toFile(path.join(output,'Icons.png'));
 fs.writeFileSync(path.join(root,'ArtSource/Football/Props/atlas.json'),JSON.stringify({base:'repository root',generator:'Tools/Build/Generate-FootballProps.cjs',width:512,height:128,tile:128,padding:4,icons:records},null,2)+'\n');
 console.log('FOOTBALL_PROP_ICONS_OK: three illustrated icons, 512 x 128');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
