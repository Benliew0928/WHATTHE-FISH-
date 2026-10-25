// Local-only integration fixture. Fake identities are accepted only on loopback.
import { createServer } from 'node:http';
import { DatabaseSync } from 'node:sqlite';
import { readFileSync,readdirSync } from 'node:fs';
import { Economy } from './economy.mjs';
import { Profiles } from './profiles.mjs';
import { profileApi } from './profile-api.mjs';
const sql=new DatabaseSync(':memory:');sql.exec('PRAGMA foreign_keys=ON');
for(const f of readdirSync(new URL('./migrations/',import.meta.url)).filter(f=>f.endsWith('.sql')).sort())sql.exec(readFileSync(new URL('./migrations/'+f,import.meta.url),'utf8'));
const db={prepare(text){return {bind(...values){const s=sql.prepare(text);return {first:async()=>s.get(...values)||null,all:async()=>({results:s.all(...values)}),run:async()=>({meta:s.run(...values)}),_run:()=>s.run(...values)};}};},async batch(statements){sql.exec('BEGIN');try{const r=statements.map(s=>s._run());sql.exec('COMMIT');return r;}catch(e){sql.exec('ROLLBACK');throw e;}}};
const store=new Profiles(db),env={PROFILES_DB:db,PROFILE_READ_LIMITER:{limit:async()=>({success:true})},PROFILE_WRITE_LIMITER:{limit:async()=>({success:true})}};
const server=createServer(async(req,res)=>{
 try{
  if(req.url==='/health'){res.end('ready');return;}
  const id=req.headers['x-economy-player'];if(typeof id!=='string'||!/^economy_review_[a-z0-9_]+$/.test(id)){res.writeHead(403);res.end();return;}
  if(!await store.mine(id))await store.save(id,{name:id.slice(-16),avatar:0,revision:0});
  const chunks=[];let size=0;for await(const chunk of req){size+=chunk.length;if(size>65536)throw new Error('size');chunks.push(chunk);}
  const body=Buffer.concat(chunks),request=new Request('http://127.0.0.1'+req.url,{method:req.method,headers:{'Content-Type':'application/json'},...(body.length?{body}: {})});
  if(req.url==='/v1/economy/review-fund'&&req.method==='POST'){await new Economy(store).ensure(id);await store.statement("INSERT OR IGNORE INTO coin_transactions(player_id,reward_key,amount,reason,created_at) VALUES(?,'review-fund',30000,'fixture',0)",id).run();res.setHeader('Content-Type','application/json');res.end(JSON.stringify(await new Economy(store).wallet(id)));return;}
  const result=await profileApi(request,env,async()=>id);res.writeHead(result.status,{'Content-Type':'application/json'});res.end(await result.text());
 }catch{res.writeHead(500);res.end('{}');}
});
server.listen(8857,'127.0.0.1',()=>console.log('ECONOMY_FIXTURE_READY'));
