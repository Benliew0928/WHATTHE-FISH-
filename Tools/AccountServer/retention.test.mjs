import test from 'node:test';
import assert from 'node:assert/strict';
import {DatabaseSync} from 'node:sqlite';
import {readFileSync,readdirSync} from 'node:fs';
import {Profiles} from './profiles.mjs';
import {deleteProfileForAccount,deletionHash} from './account-deletion.mjs';
import {runRetentionCleanup,retentionSeconds} from './retention.mjs';
import {createWorker} from './worker.mjs';

const now=1800000000,A='retention_player_a',B='retention_player_b',C='retention_player_c';
async function fixture(){
  const sql=new DatabaseSync(':memory:');sql.exec('PRAGMA foreign_keys=ON');
  for(const name of readdirSync(new URL('./migrations/',import.meta.url)).filter(f=>f.endsWith('.sql')).sort())
    sql.exec(readFileSync(new URL('./migrations/'+name,import.meta.url),'utf8'));
  const db={prepare(query){return {bind(...v){const s=sql.prepare(query);return {
    first:async()=>s.get(...v)||null,all:async()=>({results:s.all(...v)}),
    run:async()=>({meta:s.run(...v)}),_run:()=>({meta:s.run(...v)})};}};},async batch(items){
    sql.exec('BEGIN');try{const result=items.map(i=>i._run());sql.exec('COMMIT');return result;}
    catch(e){sql.exec('ROLLBACK');throw e;}
  }};
  const store=new Profiles(db);
  for(const [id,name] of [[A,'Amber'],[B,'Blue'],[C,'Cove']])await store.save(id,{name,avatar:0,revision:0});
  const match=(id,expiry,status='finished',players=[B,A])=>{
    const roster=players.map(id=>({id,team:0})),receipt=status==='finished'?JSON.stringify({id,status,awards:players.map(id=>({id,amount:30}))}):'';
    sql.prepare('INSERT INTO economy_matches(id,host,request_key,room,sport,mode,roster,created_at,expires_at,status,result_hash,receipt) VALUES(?,?,?,?,?,?,?,?,?,?,?,?)')
      .run(id,B,id,'room_'+id,'golf','match',JSON.stringify(roster),expiry-14400,expiry,status,'saved-result-hash',receipt);
    for(const idPlayer of players)sql.prepare('INSERT INTO economy_participants(match_id,player_id) VALUES(?,?)').run(id,idPlayer);
  };
  const report=(id,age,status='reviewed',reviewed=true)=>sql.prepare("INSERT INTO report_incidents(id,reporter,target,request_id,fingerprint,reason,details,context,evidence,target_name,target_code,target_revision,target_avatar_version,target_pixels,created_at,status,review_note,reviewed_at) VALUES(?,?,?,?,?,'abuse','private detail','private context','private excerpt','Blue','123456789',1,1,?, ?,?,'private review',?)")
    .run(id,A,B,id,'fingerprint',Buffer.from([1,2,3]),now-age-100,status,reviewed?now-age:null);
  return {sql,store,db,match,report};
}

test('retention boundaries, cascades and repeat runs preserve wallets and revocations',async()=>{
  const f=await fixture();f.match('expired',now-retentionSeconds.matches);f.match('within',now-retentionSeconds.matches+1);
  f.sql.prepare('INSERT INTO wallets(player_id,balance) VALUES(?,123)').run(B);
  f.sql.prepare('INSERT INTO account_deletions VALUES(?,?)').run(deletionHash('deleted-old-player'),1);
  const result=await runRetentionCleanup(f.store,now);assert.equal(result.matchesDeleted,1);
  assert.equal(f.sql.prepare("SELECT COUNT(*) n FROM economy_participants WHERE match_id='expired'").get().n,0);
  assert.ok(f.sql.prepare("SELECT 1 FROM economy_matches WHERE id='within'").get());
  assert.equal(f.sql.prepare('SELECT balance FROM wallets').get().balance,123);
  assert.equal(f.sql.prepare('SELECT COUNT(*) n FROM account_deletions').get().n,1);
  assert.equal((await runRetentionCleanup(f.store,now)).matchesDeleted,0);
});

test('closed-case evidence expires at 30 days, metadata at 90; open/undated cases survive',async()=>{
  const f=await fixture();f.report('evidence-edge',retentionSeconds.evidence);f.report('young',retentionSeconds.evidence-1);
  f.report('metadata-edge',retentionSeconds.reports,'dismissed');f.report('open',retentionSeconds.reports+10,'pending');
  f.report('missing-close-date',retentionSeconds.reports+10,'reviewed',false);
  const result=await runRetentionCleanup(f.store,now);assert.equal(result.reportsDeleted,1);
  const expired=f.sql.prepare("SELECT * FROM report_incidents WHERE id='evidence-edge'").get();
  for(const key of ['details','context','evidence','review_note'])assert.equal(expired[key],'');
  assert.equal(expired.target_pixels,null);assert.equal(expired.reason,'abuse');
  for(const id of ['young','open','missing-close-date'])assert.equal(f.sql.prepare('SELECT evidence FROM report_incidents WHERE id=?').get(id).evidence,'private excerpt');
  assert.equal((await runRetentionCleanup(f.store,now)).evidenceCleared,0);
});

test('cleanup is transactional and refuses an invalid clock',async()=>{
  const f=await fixture();f.report('old',retentionSeconds.reports);
  f.sql.exec("CREATE TRIGGER fail_cleanup BEFORE DELETE ON report_incidents BEGIN SELECT RAISE(ABORT,'fixture failure'); END;");
  await assert.rejects(runRetentionCleanup(f.store,now));
  assert.equal(f.sql.prepare('SELECT evidence FROM report_incidents').get().evidence,'private excerpt');
  await assert.rejects(runRetentionCleanup(f.store,NaN));
});

test('legacy report markers expire only with their matching closed case',async()=>{
  const f=await fixture();f.report('legacy-closed',retentionSeconds.reports);f.report('legacy-open',retentionSeconds.reports+100,'pending');
  f.sql.exec("UPDATE report_incidents SET fingerprint='legacy';");
  f.sql.prepare("UPDATE report_incidents SET target=? WHERE id='legacy-open'").run(C);
  for(const target of [B,C])f.sql.prepare("INSERT INTO reports(reporter,target,reason,created_at) VALUES(?,?,'abuse',1)").run(A,target);
  assert.equal((await runRetentionCleanup(f.store,now)).legacyReportsDeleted,1);
  assert.equal(f.sql.prepare('SELECT target FROM reports').get().target,C);
});

test('deleting a participant scrubs another host’s embedded IDs without erasing host replay keys or wallet',async()=>{
  const f=await fixture();f.match('finished',now);f.match('active',now,'active');f.match('unrelated',now,'finished',[B,C]);
  f.sql.prepare('INSERT INTO wallets(player_id,balance) VALUES(?,345)').run(B);
  const unrelated=f.sql.prepare("SELECT * FROM economy_matches WHERE id='unrelated'").get();
  await deleteProfileForAccount(f.store,A);
  for(const id of ['finished','active']){
    const m=f.sql.prepare('SELECT * FROM economy_matches WHERE id=?').get(id);
    assert.ok(!m.roster.includes(A)&&!m.receipt.includes(A));assert.equal(m.request_key,id);assert.equal(m.result_hash,'');
  }
  assert.equal(f.sql.prepare("SELECT status FROM economy_matches WHERE id='active'").get().status,'cancelled');
  assert.deepEqual(JSON.parse(f.sql.prepare("SELECT receipt FROM economy_matches WHERE id='finished'").get().receipt).awards,[{id:B,amount:30}]);
  assert.deepEqual(f.sql.prepare("SELECT * FROM economy_matches WHERE id='unrelated'").get(),unrelated);
  assert.equal(f.sql.prepare('SELECT balance FROM wallets').get().balance,345);
  assert.equal(f.sql.prepare('SELECT COUNT(*) n FROM economy_participants WHERE player_id=?').get(A).n,0);
  await deleteProfileForAccount(f.store,A);assert.equal(f.sql.prepare('SELECT COUNT(*) n FROM account_deletions').get().n,1);
});

test('profile deletion failure rolls back shared-match scrubbing and its revocation marker',async()=>{
  const f=await fixture();f.match('finished',now);
  f.sql.exec("CREATE TRIGGER fail_delete BEFORE DELETE ON profiles BEGIN SELECT RAISE(ABORT,'fixture failure'); END;");
  await assert.rejects(deleteProfileForAccount(f.store,A));
  assert.ok(await f.store.mine(A));assert.ok(f.sql.prepare('SELECT roster FROM economy_matches').get().roster.includes(A));
  assert.equal(f.sql.prepare('SELECT COUNT(*) n FROM account_deletions').get().n,0);
});

test('a controlled restore keeps revocations and reapplies deletion before reopening service',async()=>{
  const f=await fixture();f.match('restored',now);
  const profile=f.sql.prepare('SELECT * FROM profiles WHERE id=?').get(A),old=f.sql.prepare("SELECT roster,receipt FROM economy_matches WHERE id='restored'").get();
  await deleteProfileForAccount(f.store,A);
  // Simulate restoring old rows while retaining the separately preserved revocations.
  const keys=Object.keys(profile);f.sql.prepare(`INSERT INTO profiles(${keys.join(',')}) VALUES(${keys.map(()=>'?').join(',')})`).run(...Object.values(profile));
  f.sql.prepare("UPDATE economy_matches SET roster=?,receipt=? WHERE id='restored'").run(old.roster,old.receipt);
  await assert.rejects(f.store.save(A,{name:'Stale client',avatar:0,revision:0}),e=>e.code==='profile_changed');
  await deleteProfileForAccount(f.store,A);
  assert.equal(await f.store.mine(A),null);
  const scrubbed=f.sql.prepare("SELECT roster,receipt FROM economy_matches WHERE id='restored'").get();
  assert.ok(!scrubbed.roster.includes(A)&&!scrubbed.receipt.includes(A));
  assert.equal(f.sql.prepare('SELECT COUNT(*) n FROM account_deletions').get().n,1);
});

test('scheduled cleanup is opt-in and exposes no public HTTP trigger',async()=>{
  const worker=createWorker();assert.equal(await worker.scheduled({},{}),undefined);
  await assert.rejects(worker.scheduled({},{ENABLE_RETENTION_CLEANUP:'true'}));
  const f=await fixture();assert.equal((await worker.scheduled({},{ENABLE_RETENTION_CLEANUP:'true',PROFILES_DB:f.db})).matchesDeleted,0);
  assert.equal((await worker.fetch(new Request('https://fixture.invalid/retention'),{})).status,404);
});
