import policy from '../../Game/Assets/_Game/Resources/TextSafety.json' with {type:'json'};
const escape=c=>c.replace(/[.*+?^${}()|[\]\\]/g,'\\$&');
const pattern=(term,boundary)=>new RegExp((boundary?'(?<![\\p{L}\\p{M}])':'')+[...term].filter(c=>c!==' ').map(c=>escape(c)+'+').join('[^\\p{L}\\p{N}\\p{M}]*')+(boundary?'(?![\\p{L}\\p{M}])':''),'u');
const patterns=[...policy.latin.map(t=>pattern(t,true)),...policy.phrases.map(t=>pattern(t,false))];
export function blockedText(value){
 if(typeof value!=='string'||!value.trim())return false;
 if(value.length>4096)return true;
 const plain=value.normalize('NFKC').toLowerCase().replace(/\p{Cf}/gu,''),leet=plain.replace(/[013457@$]/g,c=>({'0':'o','1':'i','3':'e','4':'a','5':'s','7':'t','@':'a','$':'s'}[c]));
 return patterns.some(p=>p.test(plain)||p.test(leet));
}
