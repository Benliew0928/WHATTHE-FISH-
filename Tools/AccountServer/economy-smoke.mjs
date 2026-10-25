// Opt-in against the configured game service. Synthetic profiles are removed in finally.
// Real UGS tokens stay in memory and are never printed or saved.
import assert from 'node:assert/strict';
import { randomUUID } from 'node:crypto';
const origin='https://whatthefish-huawei-auth.benliew28262826.workers.dev';
const clients=[];let assertions=0;
async function guest(){
 const r=await fetch('https://player-auth.services.api.unity.com/v1/authentication/anonymous',{method:'POST',headers:{ProjectId:'633e314c-46d1-4b48-bbbc-2dc5f6fb653c',UnityEnvironment:'production','Content-Type':'application/json'},body:'{}'});
 assert.equal(r.status,200);const data=await r.json(),token=data.idToken||data.accessToken;
 const c={token,id:JSON.parse(Buffer.from(token.split('.')[1],'base64url')).sub};clients.push(c);return c;
}
async function request(c,path,method='GET',body,expected=200){
 const r=await fetch(origin+'/v1/'+path,{method,headers:{Authorization:'Bearer '+c.token,...(body?{'Content-Type':'application/json'}:{})},body:body?JSON.stringify(body):undefined});
 const value=await r.json();assert.equal(r.status,expected,path+': '+JSON.stringify(value));return value;
}
function check(value,label){assert.ok(value,label);console.log('PASS '+label);assertions++;}
try{
 const a=await guest(),b=await guest(),nonce=Date.now().toString(36).slice(-5);
 for(const [i,c] of clients.entries())await request(c,'me','PUT',{name:'Coin QA '+nonce+i,avatar:0,revision:0});
 const daily=await request(a,'economy/daily','POST',{});check(daily.balance===100&&daily.streak===1,'real account receives first daily reward');
 await request(a,'economy/daily','POST',{});check((await request(a,'economy/wallet')).balance===100,'repeated daily claim credits once');
 check((await request(b,'economy/wallet')).balance===0,'wallets are isolated by authenticated account');
 const start={requestKey:randomUUID(),room:'coin-smoke-'+nonce,sport:'football',mode:'match',players:[{id:a.id,team:1},{id:b.id,team:2}]};
 const ticket=await request(a,'economy/matches/start','POST',start);
 check((await request(a,'economy/matches/start','POST',start)).id===ticket.id,'start retry returns same match');
 for(const c of clients)await request(c,'economy/matches/join','POST',{id:ticket.id,room:start.room});
 const equipped=await request(a,'economy/equip','POST',{slot:'footballTrail',id:'football.trail.ocean'});
 check(equipped.owned.length===30&&equipped.equipped.some(e=>e.id==='football.trail.ocean'),'two free choices per category and free trail selection saved');
 await new Promise(resolve=>setTimeout(resolve,11000));
 const result={id:ticket.id,scoreA:2,scoreB:1,players:clients.map(c=>({id:c.id,finished:true,score:0,secondary:0}))};
 const receipt=await request(a,'economy/matches/finish','POST',result);
 check(receipt.awards.find(p=>p.id===a.id).amount===100&&receipt.awards.find(p=>p.id===b.id).amount===30,'server credits winner 100 and loser 30');
 await request(a,'economy/matches/finish','POST',result);
 const wa=await request(a,'economy/wallet'),wb=await request(b,'economy/wallet');
 check(wa.balance===200&&wb.balance===30&&wa.history.length===2,'retried finish preserves exact persisted balances');
 check(wa.equipped.some(e=>e.id==='football.trail.ocean'),'equipment survives subsequent wallet reads');
 const insufficient=await request(a,'economy/purchase','POST',{id:'football.trail.solar',price:0},409);
 check(insufficient.code==='insufficient_coins','purchase rejects insufficient funds and client supplied price');
 for(let round=0;round<4;round++){
  const match={requestKey:randomUUID(),room:'store-smoke-'+nonce+'-'+round,sport:'golf',mode:'match',players:clients.map(c=>({id:c.id,team:0}))};
  const t=await request(a,'economy/matches/start','POST',match);
  for(const c of clients)await request(c,'economy/matches/join','POST',{id:t.id,room:match.room});
  await new Promise(resolve=>setTimeout(resolve,11000));
  await request(a,'economy/matches/finish','POST',{id:t.id,scoreA:0,scoreB:0,players:clients.map((c,i)=>({id:c.id,finished:true,score:i+1,secondary:20}))});
  await new Promise(resolve=>setTimeout(resolve,9000));
 }
 const earned=await request(a,'economy/wallet');check(earned.balance===1000,'synthetic account earns the purchase price through real reward routes');
 const bought=await request(a,'economy/purchase','POST',{id:'football.trail.solar',price:1,playerId:b.id});
 check(bought.balance===0&&bought.owned.includes('football.trail.solar'),'live purchase debits exactly 1000 and grants item');
 const repeated=await request(a,'economy/purchase','POST',{id:'football.trail.solar'});
 check(repeated.balance===0&&repeated.history.filter(t=>t.key==='purchase:football.trail.solar').length===1,'live purchase retry is idempotent');
 check(!(await request(b,'economy/wallet')).owned.includes('football.trail.solar'),'purchase remains isolated to its authenticated account');
 const paid=await request(a,'economy/equip','POST',{slot:'footballTrail',id:'football.trail.solar'});
 check(paid.equipped.some(e=>e.id==='football.trail.solar'),'purchased trail can be equipped and recovered');
 console.log('ECONOMY_CLOUD_SMOKE_PASS assertions='+assertions);
}finally{
 for(const c of clients)await request(c,'me','DELETE',{confirm:'delete-profile'});
 console.log('Synthetic cloud profiles removed; credentials were not retained.');
}
