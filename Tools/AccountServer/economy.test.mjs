import test from 'node:test';
import assert from 'node:assert/strict';
import { DatabaseSync } from 'node:sqlite';
import { readFileSync,readdirSync } from 'node:fs';
import { Profiles } from './profiles.mjs';
import { Economy,DAILY,rewardDate,COSMETICS } from './economy.mjs';
import { profileApi } from './profile-api.mjs';
const A='economy_player_A123',B='economy_player_B123',C='economy_player_C123',D='economy_player_D123';
async function fixture(){
 const sql=new DatabaseSync(':memory:');sql.exec('PRAGMA foreign_keys=ON');
 for(const f of readdirSync(new URL('./migrations/',import.meta.url)).filter(f=>f.endsWith('.sql')).sort())sql.exec(readFileSync(new URL('./migrations/'+f,import.meta.url),'utf8'));
 const db={prepare(text){return {bind(...args){const s=sql.prepare(text);return {async first(){return s.get(...args)||null;},async all(){return {results:s.all(...args)};},async run(){return {meta:s.run(...args)};},_run:()=>s.run(...args)};}};},async batch(statements){sql.exec('BEGIN');try{const result=statements.map(s=>s._run());sql.exec('COMMIT');return result;}catch(e){sql.exec('ROLLBACK');throw e;}}};
 const profiles=new Profiles(db);for(const [id,name] of [[A,'Amber'],[B,'Blue'],[C,'Coral'],[D,'Dune']])await profiles.save(id,{name,avatar:0,revision:0});
 let now=1792944000;const economy=new Economy(profiles,()=>now);
 return {sql,db,profiles,economy,advance:n=>now+=n};
}
const roster=(team=false)=>[A,B,C,D].map((id,i)=>({id,team:team?(i<2?1:2):0}));
const start=(sport='football',mode='match')=>({requestKey:'request_1234567890',room:'room-123',sport,mode,players:roster(['football','basketball'].includes(sport))});
const results=(id)=>({id,scoreA:2,scoreB:1,players:[A,B,C,D].map((id,i)=>({id,finished:true,score:4-i,secondary:0}))});
async function joined(f,sport='football',mode='match'){
 const m=await f.economy.start(A,start(sport,mode));for(const id of [A,B,C,D])await f.economy.join(id,{id:m.id,room:'room-123'});f.advance(180);return m;
}
test('Malaysia day boundary and seven-day rollover are independent of client time',async()=>{
 assert.equal(rewardDate(16*3600-1),0);assert.equal(rewardDate(16*3600),1);
 const f=await fixture();let balance=0;
 for(let day=0;day<9;day++){const w=await f.economy.daily(A);balance+=DAILY[day%7];assert.equal(w.balance,balance);assert.equal(w.streak,day%7+1);assert.equal(w.todayClaimed,true);assert.ok(w.nextReset>w.serverTime);f.advance(86400);}
 assert.equal(balance,3400);
});
test('repeat and concurrent daily requests credit once; missed day resets only streak',async()=>{
 const f=await fixture();await Promise.all(Array.from({length:12},()=>f.economy.daily(A)));
 assert.equal((await f.economy.wallet(A)).balance,100);f.advance(86400);await f.economy.daily(A);f.advance(2*86400);
 const w=await f.economy.daily(A);assert.equal(w.balance,400);assert.equal(w.streak,1);assert.equal(w.history.length,3);assert.equal((await f.economy.wallet(B)).balance,0);
});
test('wallet and free inventory persist across service instances; ownership enforced',async()=>{
 const f=await fixture();await f.economy.daily(A);await f.economy.equip(A,{slot:'footballTrail',id:'football.trail.ocean'});
 const restored=new Economy(f.profiles);const w=await restored.wallet(A);assert.equal(w.balance,100);assert.equal(w.owned.length,30);assert.equal(w.equipped[0].id,'football.trail.ocean');
 await assert.rejects(restored.equip(A,{slot:'footballTrail',id:'basketball.trail.solar'}),e=>e.code==='invalid_cosmetic');
});
for(const sport of ['football','basketball'])test(`${sport} credits every eligible teammate once and rejects conflicting retries`,async()=>{
 const f=await fixture(),m=await joined(f,sport),body=results(m.id);
 const receipts=await Promise.all([f.economy.finish(A,body),f.economy.finish(A,body)]);assert.deepEqual(receipts[0],receipts[1]);
 for(const [id,coins] of [[A,100],[B,100],[C,30],[D,30]])assert.equal((await f.economy.wallet(id)).balance,coins);
 await assert.rejects(f.economy.finish(A,{...body,scoreA:0}),e=>e.code==='match_changed');
});
test('draw pays 30 and an ineligible player receives zero',async()=>{
 const f=await fixture(),m=await joined(f),body=results(m.id);body.scoreA=1;body.players[1].finished=false;await f.economy.finish(A,body);
 assert.equal((await f.economy.wallet(A)).balance,30);assert.equal((await f.economy.wallet(B)).balance,0);
});
for(const [sport,mode] of [['golf','match'],['fishing','round'],['fishing','cup']])test(`${sport} ${mode} ranks on score and secondary and pays top three only`,async()=>{
 const f=await fixture(),m=await joined(f,sport,mode);await f.economy.finish(A,results(m.id));
 const coins=await Promise.all([A,B,C,D].map(async id=>(await f.economy.wallet(id)).balance));assert.deepEqual(coins,sport==='golf'?[0,50,100,200]:[200,100,50,0]);
});
test('ties use competition ranks and golf DNF is excluded',async()=>{
 const f=await fixture(),m=await joined(f,'golf'),body=results(m.id);
 body.players.forEach((p,i)=>{p.score=i<2?1:i;p.secondary=10;});body.players[3].finished=false;
 const r=await f.economy.finish(A,body);assert.deepEqual(r.awards.map(p=>p.amount),[200,200,50,0]);assert.deepEqual(r.awards.map(p=>p.rank),[1,1,3,0]);
});
test('registered roster, two humans, balanced teams, supported modes and host permissions are required',async()=>{
 const f=await fixture();await assert.rejects(f.economy.start(A,{...start(),players:roster(true).slice(0,1)}));
 await assert.rejects(f.economy.start(A,{...start(),players:roster(true).map(p=>({...p,team:1}))}));
 await assert.rejects(f.economy.start(A,start('fishing','crew')));
 const m=await f.economy.start(A,start());await assert.rejects(f.economy.finish(B,results(m.id)),e=>e.code==='host_required');
 await assert.rejects(f.economy.join(B,{id:m.id,room:'other'}),e=>e.code==='wrong_room');
 await assert.rejects(f.economy.finish(A,results(m.id)),e=>e.code==='match_too_short');f.advance(180);
 await assert.rejects(f.economy.finish(A,results(m.id)),e=>e.code==='participants_pending');
});
test('golf supports the existing ten-player island capacity while fishing remains five',async()=>{
 const f=await fixture(),players=roster();
 for(let i=0;i<6;i++){const id='economy_extra_player_'+i;await f.profiles.save(id,{name:'Extra '+i,avatar:0,revision:0});players.push({id,team:0});}
 await assert.rejects(f.economy.start(A,{...start('fishing','round'),players:players.slice(0,6)}),e=>e.code==='invalid_roster');
 await assert.rejects(f.economy.start(A,{...start('golf'),players:[...players,{id:'economy_extra_player_6',team:0}]}),e=>e.code==='invalid_roster');
 const ticket=await f.economy.start(A,{...start('golf'),players});assert.equal(ticket.status,'active');
});
test('start retries return same ticket; concurrent rooms and duplicate accounts cannot double-register',async()=>{
 const f=await fixture();const [a,b]=await Promise.all([f.economy.start(A,start()),f.economy.start(A,start())]);assert.equal(a.id,b.id);
 await assert.rejects(f.economy.start(A,{...start(),requestKey:'different_12345678'}),e=>e.code==='match_busy');
 await assert.rejects(f.economy.start(A,{...start(),players:[roster(true)[0],roster(true)[0]]}),e=>e.code==='invalid_roster');
});
test('cancel and expiry never pay; cancelled requests cannot revive a match',async()=>{
 const f=await fixture(),m=await joined(f);await f.economy.finish(A,{id:m.id,cancelled:true});await assert.rejects(f.economy.finish(A,results(m.id)),e=>e.code==='match_cancelled');
 assert.equal((await f.economy.wallet(A)).balance,0);
 const n=await f.economy.start(A,{...start(),requestKey:'next_request_123456'});f.advance(14401);await assert.rejects(f.economy.finish(A,results(n.id)),e=>e.code==='match_cancelled');
});
test('failed settlement rolls back status and every wallet, then succeeds on retry',async()=>{
 const f=await fixture(),m=await joined(f);f.sql.exec(`CREATE TRIGGER fail_credit BEFORE INSERT ON coin_transactions WHEN NEW.player_id='${B}' BEGIN SELECT RAISE(ABORT,'test failure'); END;`);
 await assert.rejects(f.economy.finish(A,results(m.id)));assert.equal((await f.economy.wallet(A)).balance,0);assert.equal(f.sql.prepare('SELECT status FROM economy_matches WHERE id=?').get(m.id).status,'active');
 f.sql.exec('DROP TRIGGER fail_credit');await f.economy.finish(A,results(m.id));assert.equal((await f.economy.wallet(A)).balance,100);
});
test('HTTP derives wallet owner from auth; arbitrary credits and invalid purchase IDs are rejected',async()=>{
 const f=await fixture(),env={PROFILES_DB:f.db,PROFILE_READ_LIMITER:{limit:async()=>({success:true})},PROFILE_WRITE_LIMITER:{limit:async()=>({success:true})}};
 const call=(path,body={})=>profileApi(new Request('https://fixture/v1/economy/'+path,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(body)}),env,async()=>A);
 const r=await call('daily',{id:B,amount:999999,date:'2099-01-01'});assert.equal(r.status,200);assert.equal((await r.json()).balance,100);assert.equal((await f.economy.wallet(B)).balance,0);
 assert.equal((await call('credit')).status,404);assert.equal((await call('purchase')).status,400);
});
test('deleting an account removes economy data without removing other wallets',async()=>{
 const f=await fixture(),m=await joined(f);await f.economy.finish(A,results(m.id));await f.profiles.statement('DELETE FROM profiles WHERE id=?',A).run();
 assert.equal(f.sql.prepare('SELECT COUNT(*) AS n FROM wallets WHERE player_id=?').get(A).n,0);assert.equal((await f.economy.wallet(B)).balance,100);
});

test('catalog has exactly two starters per category; all other permanent items cost 1000',()=>{
 assert.equal(new Set(COSMETICS.map(c=>c.id)).size,COSMETICS.length);
 for(const slot of new Set(COSMETICS.map(c=>c.slot))){const items=COSMETICS.filter(c=>c.slot===slot);assert.equal(items.filter(c=>c.price===0).length,2);for(const c of items)assert.equal(c.price,c.index<2?0:1000);}
});
async function fund(f,id,amount){await f.economy.ensure(id);await f.profiles.statement("INSERT INTO coin_transactions(player_id,reward_key,amount,reason,created_at) VALUES(?,'fixture-credit',?,'fixture',0)",id,amount).run();}
test('purchase enforces balance and ownership; price and account are server controlled',async()=>{
 const f=await fixture(),id='football.trail.rose';await fund(f,A,999);
 await assert.rejects(f.economy.purchase(A,{id,price:0,amount:100000,playerId:B}),e=>e.code==='insufficient_coins');
 assert.equal((await f.economy.wallet(A)).balance,999);assert.ok(!(await f.economy.wallet(A)).owned.includes(id));
 await assert.rejects(f.economy.equip(A,{slot:'footballTrail',id}),e=>e.code==='cosmetic_not_owned');
 await f.economy.daily(A);const w=await f.economy.purchase(A,{id,price:0,playerId:B});
 assert.equal(w.balance,99);assert.ok(w.owned.includes(id));assert.equal(w.history.find(t=>t.reason==='purchase').amount,-1000);assert.equal((await f.economy.wallet(B)).balance,0);
 const restored=new Economy(f.profiles);assert.ok((await restored.wallet(A)).owned.includes(id));
 await restored.equip(A,{id,slot:'footballTrail'});assert.equal((await restored.wallet(A)).equipped[0].id,id);
});
test('simultaneous duplicate purchases debit once; competing purchases cannot overdraw',async()=>{
 const f=await fixture();await fund(f,A,1000);
 const same=await Promise.all(Array.from({length:12},()=>f.economy.purchase(A,{id:'basketball.trail.glacier'})));
 assert.ok(same.every(w=>w.balance===0));assert.equal(f.sql.prepare("SELECT COUNT(*) n FROM coin_transactions WHERE reason='purchase'").get().n,1);
 await fund(f,B,1000);const different=await Promise.allSettled(['golf.finish.2','fishing.finish.2'].map(id=>f.economy.purchase(B,{id})));
 assert.equal(different.filter(x=>x.status==='fulfilled').length,1);assert.equal((await f.economy.wallet(B)).balance,0);
});
test('inventory failure rolls the debit back and retry succeeds',async()=>{
 const f=await fixture();await fund(f,A,1000);
 f.sql.exec("CREATE TRIGGER fail_purchase BEFORE INSERT ON cosmetic_inventory WHEN NEW.item_id='golf.finish.2' BEGIN SELECT RAISE(ABORT,'fixture'); END;");
 await assert.rejects(f.economy.purchase(A,{id:'golf.finish.2'}));assert.equal((await f.economy.wallet(A)).balance,1000);
 f.sql.exec('DROP TRIGGER fail_purchase');assert.equal((await f.economy.purchase(A,{id:'golf.finish.2'})).balance,0);
});
test('old automatic grants are removed, real purchases retained, and stale equipment cleared',async()=>{
 const f=await fixture();await fund(f,A,1000);await f.economy.purchase(A,{id:'football.trail.solar'});
 f.sql.exec(`INSERT INTO cosmetic_inventory VALUES('${A}','football.trail.aurora');INSERT INTO cosmetic_equipment VALUES('${A}','footballTrail','football.trail.aurora');`);
 const w=await f.economy.wallet(A);assert.equal(w.balance,0);assert.ok(w.owned.includes('football.trail.solar'));assert.ok(!w.owned.includes('football.trail.aurora'));assert.equal(w.equipped.length,0);
});
test('HTTP profile avatar and purchases enforce authenticated ownership',async()=>{
 const f=await fixture();await fund(f,A,1000);
 const env={PROFILES_DB:f.db,PROFILE_WRITE_LIMITER:{limit:async()=>({success:true})}};
 const call=(path,method,body)=>profileApi(new Request('https://fixture/v1/'+path,{method,headers:{'Content-Type':'application/json'},body:JSON.stringify(body)}),env,async()=>A);
 let body={name:'Amber',avatar:3,revision:1};assert.equal((await call('me','PUT',body)).status,403);
 assert.equal((await call('economy/purchase','POST',{id:'profile.avatar.3',price:0,playerId:B})).status,200);
 assert.equal((await call('me','PUT',body)).status,200);assert.ok(!(await f.economy.wallet(B)).owned.includes('profile.avatar.3'));
});
