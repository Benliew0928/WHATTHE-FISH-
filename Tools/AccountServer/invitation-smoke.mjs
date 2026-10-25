// Opt-in deployed-service test. Only newly created disposable test identities are used.
// Tokens stay in memory. No invitation is sent to an existing player's account.
import assert from 'node:assert/strict';
const origin='https://whatthefish-huawei-auth.benliew28262826.workers.dev';
const project='633e314c-46d1-4b48-bbbc-2dc5f6fb653c';
const clients=[];let checks=0;
const check=(value,label)=>{assert.ok(value,label);checks++;console.log('PASS '+label);};
async function guest(){
 const response=await fetch('https://player-auth.services.api.unity.com/v1/authentication/anonymous',{method:'POST',headers:{ProjectId:project,UnityEnvironment:'production','Content-Type':'application/json'},body:'{}',signal:AbortSignal.timeout(30000)});
 assert.equal(response.status,200);const data=await response.json();const token=data.idToken||data.accessToken;
 const c={token,id:JSON.parse(Buffer.from(token.split('.')[1],'base64url')).sub};clients.push(c);return c;
}
async function request(c,path,method='GET',body){
 const r=await fetch(origin+path,{method,headers:{Authorization:'Bearer '+c.token,...(body?{'Content-Type':'application/json'}:{})},body:body?JSON.stringify(body):undefined,signal:AbortSignal.timeout(30000)});
 return {status:r.status,data:await r.json()};
}
try{
 const a=await guest(),b=await guest(),c=await guest();const nonce=Date.now().toString(36).slice(-5);
 for(const [i,p] of clients.entries())check((await request(p,'/v1/me','PUT',{name:'Invite '+nonce+i,avatar:i,revision:0})).status===200,'temporary profile '+i);
 check((await request(c,'/v1/invites','POST',{target:b.id,roomCode:'ABC123',sport:0,protocol:83})).status===403,'non-friend invitation rejected');
 await request(a,'/v1/friends','POST',{target:b.id,action:'request'});await request(b,'/v1/friends','POST',{target:a.id,action:'accept'});
 const sent=await request(a,'/v1/invites','POST',{target:b.id,roomCode:'ABC123',sport:2,protocol:83});check(sent.status===200,'friend invitation delivered');
 const inbox=await request(b,'/v1/invites');check(inbox.status===200&&inbox.data.invites.length===1&&inbox.data.invites[0].sender===a.id,'recipient sees actual sender');
 check(!('roomCode' in inbox.data.invites[0]),'inbox does not expose room code');
 check(inbox.data.invites[0].expiresAt-inbox.data.serverTime<=120,'expiry bounded by service clock');
 check((await request(c,'/v1/invites/resolve','POST',{id:sent.data.id})).status===410,'unrelated account cannot resolve invitation');
 const resolved=await request(b,'/v1/invites/resolve','POST',{id:sent.data.id});check(resolved.status===200&&resolved.data.protocol===83&&resolved.data.roomCode==='ABC123','recipient resolves version and room');
 check((await request(a,'/v1/invites','POST',{target:b.id,roomCode:'ABC123',sport:2,protocol:83})).status===429,'rapid resend rejected');
 await request(b,'/v1/blocks','POST',{target:a.id});check((await request(b,'/v1/invites')).data.invites.length===0,'blocking revokes invitation');
 check((await request(b,'/v1/invites/resolve','POST',{id:sent.data.id})).status===410,'blocked invitation cannot be reused');
 console.log('INVITATION_CLOUD_SMOKE_PASS assertions='+checks);
}finally{
 for(const c of clients){const r=await request(c,'/v1/me','DELETE',{confirm:'delete-profile'});assert.equal(r.status,200,'temporary profile cleanup');}
 console.log('Temporary cloud profiles removed. No credentials saved.');
}
