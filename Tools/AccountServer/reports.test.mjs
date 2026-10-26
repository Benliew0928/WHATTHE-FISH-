import test from 'node:test';
import assert from 'node:assert/strict';
import {DatabaseSync} from 'node:sqlite';
import {readFileSync,readdirSync} from 'node:fs';
import {Profiles} from './profiles.mjs';
import {submitReport} from './reports.mjs';
import {profileApi} from './profile-api.mjs';
import {blockedText} from './text-safety.mjs';
import {validateName} from './profile-search.mjs';
const A='report_test_player_a',B='report_test_player_b';
async function fixture(){
 const sql=new DatabaseSync(':memory:');sql.exec('PRAGMA foreign_keys=ON');
 for(const file of readdirSync(new URL('./migrations/',import.meta.url)).filter(n=>n.endsWith('.sql')).sort())sql.exec(readFileSync(new URL('./migrations/'+file,import.meta.url),'utf8'));
 const db={prepare(query){return {bind(...args){const s=sql.prepare(query),v=args.map(a=>Array.isArray(a)?new Uint8Array(a):a);return {first:async()=>s.get(...v)||null,all:async()=>({results:s.all(...v)}),run:async()=>({meta:s.run(...v)}),_run:()=>s.run(...v)};}};},async batch(items){sql.exec('BEGIN');try{const result=items.map(i=>i._run());sql.exec('COMMIT');return result;}catch(e){sql.exec('ROLLBACK');throw e;}}};
 const store=new Profiles(db);await store.save(A,{name:'Amber',avatar:0,revision:0});await store.save(B,{name:'Blue',avatar:0,revision:0,photo:Buffer.alloc(49152,100).toString('base64')});
 return {store,sql,db};
}
const body=(requestId='a'.repeat(32),reason='abuse')=>({version:2,requestId,target:B,reason,details:'Repeated insults',context:'Football',evidence:'Reported message: fuck you'});
test('multilingual filter catches listed terms, common evasions and mixed-language input',()=>{
 for(const s of ['fuck','F.U.C.K','f u c k','ＦＵＣＫ','f\u200bu\u200bc\u200bk','fuuuck','sh1t','bull$hit','fuck123','pukimak','P U K I M A K','cibai','你好 傻逼 lah','你是傻 逼','草泥马','約炮','send nudes'])assert.equal(blockedText(s),true,s);
 for(const s of ['class','Scunthorpe','Dick','assistant','Shitake','Mishit','sex education','sexual health','gay','transgender','babi','anjing','Terima kasih','Selamat bermain','你好朋友','Nice pass!','福克斯','Sialkot'])assert.equal(blockedText(s),false,s);
 assert.throws(()=>validateName('fuck123'),e=>e.code==='offensive_name');assert.equal(validateName('Dick'),'Dick');
});
test('retry is idempotent, other incidents survive, and quoted evidence is preserved privately',async()=>{
 const {store,sql}=await fixture(),first=await submitReport(store,A,body());
 assert.deepEqual(await submitReport(store,A,body()),first);assert.equal(sql.prepare('SELECT COUNT(*) n FROM report_incidents').get().n,1);
 await assert.rejects(submitReport(store,A,{...body(),details:'changed'}),e=>e.code==='report_changed');
 await assert.rejects(submitReport(store,A,body('b'.repeat(32))),e=>e.code==='report_limit');
 sql.prepare('UPDATE report_incidents SET created_at=created_at-61').run();
 const second=await submitReport(store,A,body('b'.repeat(32),'behaviour'));assert.notEqual(second.id,first.id);
 assert.equal(sql.prepare('SELECT COUNT(*) n FROM report_incidents').get().n,2);
 assert.equal(sql.prepare('SELECT evidence FROM report_incidents WHERE id=?').get(first.id).evidence,body().evidence);
 assert.equal('details' in await store.get(A,B),false);
});
test('report keeps server profile and photo snapshot after target edits',async()=>{
 const {store,sql}=await fixture(),r=await submitReport(store,A,body('c'.repeat(32),'photo'));
 await store.save(B,{name:'Renamed',avatar:0,revision:1,photo:''});
 const row=sql.prepare('SELECT * FROM report_incidents WHERE id=?').get(r.id);assert.equal(row.target_name,'Blue');assert.equal(row.target_revision,1);assert.equal(row.target_pixels.length,49152);
});
test('invalid or self reports and empty Other cannot be saved; details have bounded size',async()=>{
 const {store,sql}=await fixture();
 for(const patch of [{target:A},{target:'missing'},{reason:'bogus'},{reason:'other',details:' '},{details:'x'.repeat(501)},{evidence:'x'.repeat(2401)},{requestId:'bad'},{details:23},{version:99}])await assert.rejects(submitReport(store,A,{...body(),...patch}),e=>e.status===400);
 assert.equal(sql.prepare('SELECT COUNT(*) n FROM report_incidents').get().n,0);
});
test('parallel identical submission returns one receipt; concurrent new submissions obey cooldown',async()=>{
 const {store,sql}=await fixture();const same=await Promise.all(Array.from({length:5},()=>submitReport(store,A,body())));assert.ok(same.every(r=>r.id===same[0].id));
 sql.prepare('UPDATE report_incidents SET created_at=created_at-61').run();
 const results=await Promise.allSettled(['b','c','d'].map(c=>submitReport(store,A,body(c.repeat(32)))));assert.equal(results.filter(r=>r.status==='fulfilled').length,1);
 assert.equal(sql.prepare('SELECT COUNT(*) n FROM report_incidents').get().n,2);
});
test('API derives reporter from authenticated session, advertises contract and rejects bad names',async()=>{
 const {store,sql,db}=await fixture(),env={PROFILES_DB:db,PROFILE_READ_LIMITER:{limit:async()=>({success:true})},PROFILE_WRITE_LIMITER:{limit:async()=>({success:true})}};
 const call=(path,method='GET',data)=>profileApi(new Request('https://fixture'+path,{method,headers:{'Content-Type':'application/json'},...(data?{body:JSON.stringify(data)}:{})}),env,async()=>A);
 assert.equal((await (await call('/v1/reports')).json()).version,2);
 assert.equal((await call('/v1/reports','POST',{...body(),reporter:B})).status,200);assert.equal(sql.prepare('SELECT reporter FROM report_incidents').get().reporter,A);
 assert.equal((await call('/v1/me','PUT',{name:'傻逼',avatar:0,revision:1})).status,400);assert.equal((await store.mine(A)).name,'Amber');
 await store.block(A,B);sql.prepare('UPDATE report_incidents SET created_at=created_at-61').run();assert.equal((await call('/v1/reports','POST',body('e'.repeat(32)))).status,200);
 await store.statement('DELETE FROM profiles WHERE id=?',A).run();assert.equal(sql.prepare('SELECT COUNT(*) n FROM report_incidents').get().n,0);
});
