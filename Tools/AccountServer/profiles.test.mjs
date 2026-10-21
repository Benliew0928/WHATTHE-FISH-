import test from 'node:test';
import assert from 'node:assert/strict';
import { DatabaseSync } from 'node:sqlite';
import { readFileSync } from 'node:fs';
import { generateKeyPairSync, sign } from 'node:crypto';
import { Profiles } from './profiles.mjs';
import { normalize, rank, distance, validateName } from './profile-search.mjs';
import { verifyPlayerToken } from './player-auth.mjs';
import { profileApi } from './profile-api.mjs';
function fixture(){
  const sql=new DatabaseSync(':memory:');sql.exec('PRAGMA foreign_keys=ON');
  sql.exec(readFileSync(new URL('./migrations/0001_profiles.sql',import.meta.url),'utf8'));
  sql.exec(readFileSync(new URL('./migrations/0002_profile_limits.sql',import.meta.url),'utf8'));
  const db={prepare(text){return {bind(...args){
    const statement=sql.prepare(text),values=args.map(a=>Array.isArray(a)?new Uint8Array(a):a);
    return {async first(){return statement.get(...values)||null;},async all(){return {results:statement.all(...values)};},
      async run(){return {meta:statement.run(...values)};},_run:()=>statement.run(...values)};
  }};},async batch(statements){sql.exec('BEGIN');try{const r=statements.map(s=>s._run());sql.exec('COMMIT');return r;}catch(e){sql.exec('ROLLBACK');throw e;}}};
  return {sql,db,store:new Profiles(db)};
}
const A='player_A_123456789',B='player_B_123456789',C='player_C_123456789';
const create=(s,id,name)=>s.save(id,{name,revision:0,avatar:0});
test('all ten character avatars persist; invalid indices cannot replace a saved profile',async()=>{
  const {store:s}=fixture();let p=await create(s,A,'Character');
  for(let avatar=0;avatar<10;avatar++){
    p=await s.save(A,{name:p.name,revision:p.revision,avatar});
    assert.equal((await s.get(B,A)).avatar,avatar);
  }
  for(const avatar of [-1,10,1.5,'9',null]){
    await assert.rejects(s.save(A,{name:p.name,revision:p.revision,avatar}),e=>e.code==='invalid_avatar');
  }
  assert.equal((await s.mine(A)).avatar,9);
});
test('normalization, exact precedence, word prefixes, swapped letters and short-query precision',()=>{
  assert.equal(normalize('  JOSÉ_Wave-2 '),'jose wave 2');
  assert.equal(validateName('José'), 'José');
  assert.throws(()=>validateName('<color=red>x'));
  assert.equal(rank('jose','José'),0);
  assert.ok(rank('jo','John')<rank('jo','Captain John'));
  assert.equal(distance('jonh','john'),1);
  assert.ok(Number.isFinite(rank('jonh','John')));
  assert.equal(rank('jo','Bo'),Infinity);
});
test('profile CAS, image bounds and search-index rename are atomic',async()=>{
  const {store:s,sql}=fixture();
  const a=await create(s,A,'José Wave');assert.equal(a.revision,1);
  await assert.rejects(s.save(A,{name:'Oops',revision:0,avatar:0}),e=>e.code==='profile_changed');
  await assert.rejects(s.save(A,{name:'Oops',revision:1,avatar:0,photo:'bad'}),e=>e.code==='invalid_photo');
  const pixels=Buffer.alloc(49152,95).toString('base64');
  const b=await s.save(A,{name:'Luna Tide',revision:1,avatar:1,photo:pixels});
  assert.equal(b.avatarVersion,1);assert.equal((await s.photo(B,A,1)).length,49152);
  assert.equal(sql.prepare("SELECT COUNT(*) AS n FROM search_terms WHERE term='jose'").get().n,0);
  assert.equal((await s.search(B,'luna')).players[0].id,A);
  await assert.rejects(s.save(A,{name:'Stale',revision:1,avatar:0}),e=>e.code==='profile_changed');
});
test('duplicate names retain distinct public codes; indexed exact/prefix pagination',async()=>{
  const {store:s}=fixture();
  await create(s,A,'Sam');await create(s,B,'Sam');
  assert.notEqual((await s.mine(A)).code,(await s.mine(B)).code);
  assert.equal((await s.search(C,'SAM')).players.length,2);
  assert.equal((await s.search(C,(await s.mine(B)).code)).players[0].id,B);
  for(let i=0;i<23;i++)await create(s,'fixture_player_'+String(i).padStart(4,'0'),'Wave '+i);
  const first=await s.search(C,'wave'),second=await s.search(C,'wave',first.offset);
  assert.equal(first.players.length,20);assert.equal(first.more,true);assert.equal(second.players.length,3);
  assert.equal(new Set([...first.players,...second.players].map(p=>p.id)).size,23);
  await create(s,'fixture_player_john','John');
  assert.equal((await s.search(C,'Jonh')).players[0].name,'John');
});
test('friend request ownership, crossing, acceptance, decline, remove and cooldown',async()=>{
  const {store:s}=fixture();await create(s,A,'Amber');await create(s,B,'Blue');
  assert.equal((await s.friend(A,B,'request')).relationship,'outgoing');
  assert.equal((await s.friend(A,B,'request')).relationship,'outgoing');
  assert.equal((await s.friend(B,A,'request')).relationship,'incoming');
  assert.equal((await s.friend(A,B,'accept')).relationship,'outgoing');
  assert.equal((await s.friend(B,A,'accept')).relationship,'friends');
  assert.equal((await s.relationships(A)).friends.length,1);
  assert.equal((await s.friend(A,B,'remove')).relationship,'none');
  await assert.rejects(s.friend(A,B,'request'),e=>e.code==='request_cooldown');
});
test('decline only by recipient; blocking removes friendship and excludes profiles/search/photos',async()=>{
  const {store:s}=fixture();await create(s,A,'Amber');await create(s,B,'Blue');
  await s.friend(A,B,'request');
  assert.equal((await s.friend(A,B,'decline')).relationship,'outgoing');
  assert.equal((await s.friend(B,A,'decline')).relationship,'none');
  await s.block(A,B);
  assert.equal((await s.search(A,'blue')).players.length,0);
  await assert.rejects(s.get(B,A),e=>e.code==='profile_not_found');
  await assert.rejects(s.friend(B,A,'request'),e=>e.code==='player_unavailable');
});
test('one-use room proof is bound to room and challenge and does not reveal access tokens',async()=>{
  const {store:s}=fixture();await create(s,A,'Amber');
  const challenge='a'.repeat(32),p=await s.proof(A,'room-a',challenge);
  await assert.rejects(s.verifyProof('room-b',challenge,p.proof),e=>e.code==='invalid_proof');
  await assert.rejects(s.verifyProof('room-a','b'.repeat(32),p.proof),e=>e.code==='invalid_proof');
  assert.equal((await s.verifyProof('room-a',challenge,p.proof)).id,A);
  await assert.rejects(s.verifyProof('room-a',challenge,p.proof),e=>e.code==='invalid_proof');
});
test('concurrent requests cannot exceed the 150 combined relationship limit',async()=>{
  const {store:s,sql}=fixture();await create(s,A,'Captain');
  for(let i=0;i<151;i++){
    const id='capacity_player_'+String(i).padStart(3,'0');await create(s,id,'Crew '+i);
    if(i<149)sql.prepare("INSERT INTO relationships VALUES(?,?,?,'accepted',0)").run(...[A,id].sort(),A);
  }
  const results=await Promise.allSettled([s.friend(A,'capacity_player_149','request'),s.friend(A,'capacity_player_150','request')]);
  assert.equal(results.filter(r=>r.status==='fulfilled').length,1);
  assert.equal((await s.relationships(A)).friends.length+(await s.relationships(A)).outgoing.length,150);
});
test('photo-only updates preserve search indexes and malformed/expired proofs fail',async()=>{
  const {store:s,sql}=fixture();const p=await create(s,A,'Profile');
  const before=sql.prepare('SELECT rowid,* FROM search_deletes ORDER BY rowid').all();
  await s.save(A,{name:p.name,revision:p.revision,avatar:2,photo:Buffer.alloc(49152).toString('base64')});
  assert.deepEqual(sql.prepare('SELECT rowid,* FROM search_deletes ORDER BY rowid').all(),before);
  const proof=await s.proof(A,'room','f'.repeat(32));sql.exec('UPDATE room_proofs SET expires_at=0');
  await assert.rejects(s.verifyProof('room','f'.repeat(32),proof.proof),e=>e.code==='invalid_proof');
});
test('API obtains owner from verified session, rejects invalid bodies, and scopes deletion',async()=>{
  const {store:s,db}=fixture();await create(s,A,'Amber');await create(s,B,'Blue');
  const env={PROFILES_DB:db,PROFILE_READ_LIMITER:{limit:async()=>({success:true})},PROFILE_WRITE_LIMITER:{limit:async()=>({success:true})}};
  const result=await profileApi(new Request('https://fixture/v1/me',{method:'PUT',headers:{'Content-Type':'application/json'},
    body:JSON.stringify({id:B,name:'Owner only',avatar:0,revision:1})}),env,async()=>A);
  assert.equal(result.status,200);assert.equal((await s.mine(B)).name,'Blue');assert.equal((await s.mine(A)).name,'Owner only');
  const removed=await profileApi(new Request('https://fixture/v1/me',{method:'DELETE',headers:{'Content-Type':'application/json'},body:'{"confirm":"delete-profile"}'}),env,async()=>A);
  assert.equal(removed.status,200);assert.equal(await s.mine(A),null);assert.ok(await s.mine(B));
});
test('UGS signature/project/environment/time validation cannot be replaced by decoded client claims',()=>{
  const {privateKey,publicKey}=generateKeyPairSync('rsa',{modulusLength:2048});
  const key={...publicKey.export({format:'jwk'}),kid:'fixture'};
  const now=Math.floor(Date.now()/1000);
  const claims={iss:'https://player-auth.services.api.unity.com',sub:A,project_id:'project',aud:['upid:project','envId:env'],iat:now,nbf:now,exp:now+60};
  const token=c=>{const unsigned=[{alg:'RS256',kid:'fixture'},c].map(v=>Buffer.from(JSON.stringify(v)).toString('base64url')).join('.');return unsigned+'.'+sign('RSA-SHA256',Buffer.from(unsigned),privateKey).toString('base64url');};
  assert.equal(verifyPlayerToken(token(claims),[key],'project','env'),A);
  for(const changed of [{exp:now-1},{project_id:'other'},{aud:['upid:project','envId:other']},{iss:'fake'}])
    assert.throws(()=>verifyPlayerToken(token({...claims,...changed}),[key],'project','env'));
  assert.throws(()=>verifyPlayerToken(token(claims).slice(0,-10)+'invalid',[key],'project','env'));
});
