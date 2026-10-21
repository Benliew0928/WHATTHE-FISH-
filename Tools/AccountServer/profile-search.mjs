import { AuthError } from './auth.mjs';
export function validateName(value) {
  if(typeof value!=='string')throw new AuthError(400,'invalid_name');
  const name=value.normalize('NFC').trim();
  if(!name || name.length>16 || !/^[\p{L}\p{N}\p{M} _-]+$/u.test(name) || !/[\p{L}\p{N}]/u.test(name))
    throw new AuthError(400,'invalid_name');
  return name;
}
export function normalize(value) {
  return String(value).normalize('NFKD').toLowerCase().replace(/\p{M}/gu,'')
    .replace(/[_\-\s]+/gu,' ').trim();
}
export function grams(value) {
  const chars=Array.from(value.replace(/ /g,''));
  const result=new Set();
  for(let i=0;i+2<chars.length;i++)result.add(chars.slice(i,i+3).join(''));
  return [...result].slice(0,32);
}
export function deletions(value) {
  const chars=Array.from(value),result=new Set([value]);
  for(let i=0;i<chars.length;i++)result.add([...chars.slice(0,i),...chars.slice(i+1)].join(''));
  return [...result];
}
export function distance(a,b) {
  // Bounded optimal-string-alignment distance also catches adjacent swapped letters.
  const x=Array.from(a),y=Array.from(b),d=Array.from({length:x.length+1},()=>Array(y.length+1).fill(0));
  for(let i=0;i<=x.length;i++)d[i][0]=i;
  for(let j=0;j<=y.length;j++)d[0][j]=j;
  for(let i=1;i<=x.length;i++)for(let j=1;j<=y.length;j++){
    d[i][j]=Math.min(d[i-1][j]+1,d[i][j-1]+1,d[i-1][j-1]+(x[i-1]===y[j-1]?0:1));
    if(i>1&&j>1&&x[i-1]===y[j-2]&&x[i-2]===y[j-1])d[i][j]=Math.min(d[i][j],d[i-2][j-2]+1);
  }
  return d[x.length][y.length];
}
export function rank(query,name) {
  const q=normalize(query),n=normalize(name);
  if(n===q)return 0;
  if(n.startsWith(q))return 100+n.length-q.length;
  if(n.split(' ').some(t=>t.startsWith(q)))return 200+n.length-q.length;
  if(q.length<4)return Infinity;
  const edit=distance(q,n),limit=q.length>=8?2:1;
  return edit<=limit?300+edit*10+Math.abs(q.length-n.length):Infinity;
}
