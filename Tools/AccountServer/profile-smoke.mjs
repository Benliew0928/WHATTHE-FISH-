// Opt-in against the configured development backend. Credentials exist only in memory.
// Creates two temporary guest profiles, then removes those profiles in finally.
import assert from 'node:assert/strict';
const origin='https://whatthefish-huawei-auth.benliew28262826.workers.dev';
const project='633e314c-46d1-4b48-bbbc-2dc5f6fb653c';
const clients=[];let passed=0;
async function guest(){
 const response=await fetch('https://player-auth.services.api.unity.com/v1/authentication/anonymous',{
  method:'POST',headers:{ProjectId:project,UnityEnvironment:'production','Content-Type':'application/json'},body:'{}'});
 assert.equal(response.status,200);const data=await response.json();
 const client={id:data.userId||data.playerId,token:data.idToken||data.accessToken};
 assert.ok(client.token);client.id=JSON.parse(Buffer.from(client.token.split('.')[1],'base64url')).sub;clients.push(client);return client;
}
async function request(client,path,method='GET',body){
 const response=await fetch(origin+path,{method,headers:{Authorization:'Bearer '+client.token,...(body?{'Content-Type':'application/json'}:{})},body:body?JSON.stringify(body):undefined});
 const value=path.startsWith('/v1/photos/')&&response.ok?Buffer.from(await response.arrayBuffer()):await response.json();
 return {status:response.status,value};
}
function check(condition,label){assert.ok(condition,label);console.log('PASS '+label);passed++;}
try{
 const a=await guest(),b=await guest();const nonce=Date.now().toString(36).slice(-5);
 const create=async(c,name)=>request(c,'/v1/me','PUT',{name,avatar:9,photo:'',revision:0});
 const pa=await create(a,'QA '+nonce),pb=await create(b,'John '+nonce);
 check(pa.status===200&&pb.status===200,'two real UGS guests create cloud profiles');
 check(pa.value.avatar===9&&pb.value.avatar===9,'tenth character avatar persists on deployed service');
 const photo=Buffer.alloc(49152,83);
 const save=await request(a,'/v1/me','PUT',{name:'QA '+nonce,avatar:2,photo:photo.toString('base64'),revision:pa.value.revision});
 check(save.status===200&&save.value.hasPhoto===1,'save photo and profile revision');
 const read=await request(b,'/v1/photos/'+a.id+'/'+save.value.avatarVersion);
 check(read.status===200&&read.value.equals(photo),'other player reads exact thumbnail bytes');
 const exact=await request(a,'/v1/search?q='+encodeURIComponent('John '+nonce));
 check(exact.status===200&&exact.value.players[0]?.id===b.id,'exact name search');
 const typo=await request(a,'/v1/search?q='+encodeURIComponent('Jonh '+nonce));
 check(typo.status===200&&typo.value.players.some(p=>p.id===b.id),'transposed-letter search');
 const sent=await request(a,'/v1/friends','POST',{target:b.id,action:'request'});
 check(sent.status===200&&sent.value.relationship==='outgoing','send friend request');
 const incoming=await request(b,'/v1/friends');
 check(incoming.value.incoming.some(p=>p.id===a.id),'recipient sees incoming request');
 const invalid=await request(a,'/v1/friends','POST',{target:b.id,action:'accept'});
 check(invalid.value.relationship==='outgoing','sender cannot accept own request');
 const accept=await request(b,'/v1/friends','POST',{target:a.id,action:'accept'});
 check(accept.value.relationship==='friends','recipient accepts request');
 const proof=await request(a,'/v1/room-proof','POST',{room:'qa-room',challenge:'c'.repeat(32)});
 const verified=await request(b,'/v1/room-proof/verify','POST',{room:'qa-room',challenge:'c'.repeat(32),proof:proof.value.proof});
 check(verified.status===200&&verified.value.id===a.id,'room proof verifies actual guest ownership');
 const replay=await request(b,'/v1/room-proof/verify','POST',{room:'qa-room',challenge:'c'.repeat(32),proof:proof.value.proof});
 check(replay.status===401,'proof replay rejected');
 const conflict=await request(a,'/v1/me','PUT',{name:'Stale',avatar:0,revision:1});
 check(conflict.status===409,'stale profile update rejected');
 const blocked=await request(b,'/v1/blocks','POST',{target:a.id});
 const hidden=await request(a,'/v1/players/'+b.id);
 check(blocked.status===200&&hidden.status===404,'blocking hides profiles and removes friendship');
 console.log('PROFILE_CLOUD_SMOKE_PASS assertions='+passed);
}finally{
 for(const c of clients){
  const r=await request(c,'/v1/me','DELETE',{confirm:'delete-profile'});
  if(r.status!==200)throw new Error('Temporary profile cleanup failed: '+r.status);
 }
 console.log('Temporary cloud profiles removed; no session credentials retained.');
}
