// Opt-in deployed-service check. Only these synthetic profiles are removed.
// Real UGS session tokens remain in memory and are never printed or saved.
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
const origin='https://whatthefish-huawei-auth.benliew28262826.workers.dev';
const clients=[];let assertions=0;
async function guest(){
 const r=await fetch('https://player-auth.services.api.unity.com/v1/authentication/anonymous',{method:'POST',headers:{ProjectId:'633e314c-46d1-4b48-bbbc-2dc5f6fb653c',UnityEnvironment:'production','Content-Type':'application/json'},body:'{}'});
 assert.equal(r.status,200);const d=await r.json(),token=d.idToken||d.accessToken,c={token};clients.push(c);return c;
}
async function request(c,path,method='GET',body,expected=200){
 const r=await fetch(origin+'/v1/'+path,{method,headers:{Authorization:'Bearer '+c.token,...(body?{'Content-Type':'application/json'}:{})},body:body?JSON.stringify(body):undefined});
 const data=await r.json();assert.equal(r.status,expected,path+': '+JSON.stringify(data));return data;
}
function check(value,label){assert.ok(value,label);console.log('PASS '+label);assertions++;}
try{
 const a=await guest(),b=await guest(),nonce=Date.now().toString(36).slice(-6);
 const pa=await request(a,'me','PUT',{name:'Report QA '+nonce,avatar:0,revision:0});a.created=true;
 const pb=await request(b,'me','PUT',{name:'Target QA '+nonce,avatar:0,revision:0});b.created=true;
 check((await request(a,'reports')).version===2,'deployed incident contract is version 2');
 const rejected=await request(a,'me','PUT',{name:'sh1t',avatar:0,revision:pa.revision},400);
 check(rejected.code==='offensive_name'&&(await request(a,'me')).name===pa.name,'offensive profile update rejected without changing name');
 const report={version:2,target:pb.id,reason:'other',details:'Synthetic QA report; no actual misconduct.',context:'moderation-smoke',evidence:'',requestId:randomUUID().replaceAll('-','')};
 const receipt=await request(a,'reports','POST',report),retry=await request(a,'reports','POST',report);
 check(receipt.ok&&receipt.version===2&&receipt.id===retry.id,'report accepted and retry returns same saved incident');
 check((await request(a,'reports','POST',{...report,details:'Changed'},409)).code==='report_changed','changed retry rejected');
 check((await request(a,'reports','POST',{...report,target:pa.id},400)).code==='invalid_report','self report rejected');
 check((await request(a,'reports','POST',{...report,details:''},400)).code==='report_details_required','Other requires explanation');
 check((await request(a,'reports','POST',{...report,requestId:randomUUID().replaceAll('-','')},429)).code==='report_limit','rapid duplicate incident rate limited');
 console.log('MODERATION_CLOUD_SMOKE_PASS assertions='+assertions);
}finally{
 for(const c of clients)if(c.created)await request(c,'me','DELETE',{confirm:'delete-profile'});
 console.log('Synthetic profiles and their reports removed; no tokens retained.');
}
