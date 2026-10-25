import { randomUUID, createHash } from 'node:crypto';
import { AuthError } from './auth.mjs';
import catalog from '../../Game/Assets/_Game/Resources/EconomyCosmetics.json' with {type:'json'};
export const DAILY = [100,200,300,400,500,600,1000];
export const COSMETICS = catalog.items;
const fail=(status,code)=>{throw new AuthError(status,code);};
const integer=(value,max)=>Number.isSafeInteger(value)&&value>=0&&value<=max;
const validId=value=>typeof value==='string'&&/^[A-Za-z0-9_-]{16,128}$/.test(value);
const key=value=>typeof value==='string'&&/^[A-Za-z0-9_-]{16,64}$/.test(value);
export const rewardDate=seconds=>Math.floor((seconds+8*3600)/86400);
export function payouts(match,body) {
  const roster=JSON.parse(match.roster),team=match.sport==='football'||match.sport==='basketball';
  if(!Array.isArray(body.players)||body.players.length!==roster.length)fail(400,'invalid_results');
  const values=new Map();
  for(const p of body.players){
    if(!p||!validId(p.id)||values.has(p.id)||typeof p.finished!=='boolean')fail(400,'invalid_results');
    if(!integer(p.score,1000000)||typeof p.secondary!=='number'||!Number.isFinite(p.secondary)||p.secondary<0||p.secondary>100000000)fail(400,'invalid_results');
    values.set(p.id,p);
  }
  if(roster.some(p=>!values.has(p.id)))fail(400,'invalid_results');
  if(team&&(!integer(body.scoreA,10000)||!integer(body.scoreB,10000)))fail(400,'invalid_results');
  const ordered=roster.map(p=>({...p,...values.get(p.id)}));
  if(!team)ordered.sort((a,b)=>Number(b.finished)-Number(a.finished)||(match.sport==='golf'?a.score-b.score||a.secondary-b.secondary:b.score-a.score||b.secondary-a.secondary)||a.id.localeCompare(b.id));
  let rank=0;
  return ordered.map((p,i)=>{
    if(team){const winner=body.scoreA===body.scoreB?0:body.scoreA>body.scoreB?1:2;return {id:p.id,rank:0,amount:p.finished?(winner===p.team?100:30):0};}
    if(i===0||p.score!==ordered[i-1].score||p.secondary!==ordered[i-1].secondary)rank=i+1;
    return {id:p.id,rank:p.finished?rank:0,amount:p.finished?([200,100,50][rank-1]||0):0};
  });
}
export class Economy {
  constructor(profiles,clock=()=>Math.floor(Date.now()/1000)){this.s=profiles;this.clock=clock;}
  async ensure(id){
    await this.s.require(id);
    await this.s.db.batch([
      this.s.statement('INSERT OR IGNORE INTO wallets(player_id) VALUES(?)',id),
      ...COSMETICS.filter(c=>c.price===0).map(c=>this.s.statement('INSERT OR IGNORE INTO cosmetic_inventory(player_id,item_id) VALUES(?,?)',id,c.id)),
      // The previous release auto-granted every trail. Only actual purchases
      // retain paid items; the canonical receipt also makes this retry-safe.
      this.s.statement(`DELETE FROM cosmetic_inventory WHERE player_id=? AND item_id IN (${COSMETICS.filter(c=>c.price>0).map(()=>'?').join(',')}) AND NOT EXISTS(SELECT 1 FROM coin_transactions t WHERE t.player_id=cosmetic_inventory.player_id AND t.reward_key='purchase:'||cosmetic_inventory.item_id AND t.reason='purchase')`,id,...COSMETICS.filter(c=>c.price>0).map(c=>c.id))
    ]);
  }
  async wallet(id){
    await this.ensure(id);const now=this.clock(),day=rewardDate(now),w=await this.s.one('SELECT balance,revision,last_day AS lastDay,streak FROM wallets WHERE player_id=?',id);
    return {storeVersion:2,playerId:id,...w,serverTime:now,nextReset:(day+1)*86400-8*3600,todayClaimed:w.lastDay===day,
      nextDay:w.lastDay===day?w.streak:w.lastDay===day-1?w.streak%7+1:1,dailyRewards:DAILY,
      history:await this.s.rows('SELECT reward_key AS key,amount,reason,created_at AS time FROM coin_transactions WHERE player_id=? ORDER BY created_at DESC,reward_key DESC LIMIT 30',id),
      owned:(await this.s.rows('SELECT item_id AS id FROM cosmetic_inventory WHERE player_id=?',id)).map(x=>x.id),
      equipped:await this.s.rows('SELECT slot,item_id AS id FROM cosmetic_equipment WHERE player_id=?',id)};
  }
  async daily(id){
    await this.ensure(id);const now=this.clock(),day=rewardDate(now);
    // One SQL insertion reads the previous streak; the trigger credits and advances it atomically.
    await this.s.statement(`INSERT OR IGNORE INTO coin_transactions(player_id,reward_key,amount,reason,created_at,reward_day,cycle_day)
      SELECT player_id,?,CASE WHEN last_day=? AND streak=6 THEN 1000 WHEN last_day=? THEN (streak%7+1)*100 ELSE 100 END,
      'daily',?,?,CASE WHEN last_day=? THEN streak%7+1 ELSE 1 END FROM wallets WHERE player_id=? AND last_day<?`,
      `daily:${day}`,day-1,day-1,now,day,day-1,id,day).run();
    return this.wallet(id);
  }
  async equip(id,body){
    await this.ensure(id);const item=COSMETICS.find(c=>c.id===body.id&&c.slot===body.slot);
    if(!item)fail(400,'invalid_cosmetic');
    if(!await this.s.one('SELECT 1 FROM cosmetic_inventory WHERE player_id=? AND item_id=?',id,item.id))fail(403,'cosmetic_not_owned');
    await this.s.statement('INSERT INTO cosmetic_equipment(player_id,slot,item_id) VALUES(?,?,?) ON CONFLICT(player_id,slot) DO UPDATE SET item_id=excluded.item_id',id,item.slot,item.id).run();
    return this.wallet(id);
  }
  async purchase(id,body){
    const item=COSMETICS.find(c=>c.id===body.id);if(!item)fail(400,'invalid_cosmetic');
    await this.ensure(id);
    if(item.price>0)await this.s.statement(`INSERT OR IGNORE INTO coin_transactions(player_id,reward_key,amount,reason,created_at)
      SELECT player_id,?,?,'purchase',? FROM wallets WHERE player_id=? AND balance>=?
      AND NOT EXISTS(SELECT 1 FROM cosmetic_inventory WHERE player_id=? AND item_id=?)`,
      'purchase:'+item.id,-item.price,this.clock(),id,item.price,id,item.id).run();
    // The SQL trigger debits and grants within the same statement. A retry for
    // the same permanent item returns the current wallet without charging again.
    if(!await this.s.one('SELECT 1 FROM cosmetic_inventory WHERE player_id=? AND item_id=?',id,item.id))fail(409,'insufficient_coins');
    return this.wallet(id);
  }
  async expire(){await this.s.statement("UPDATE economy_matches SET status='cancelled' WHERE status='active' AND expires_at<=?",this.clock()).run();}
  async start(id,body){
    await this.ensure(id);await this.expire();
    if(!key(body.requestKey)||typeof body.room!=='string'||!/^[A-Za-z0-9_-]{1,128}$/.test(body.room))fail(400,'invalid_match');
    if(!['football','basketball','golf','fishing'].includes(body.sport)||body.mode!==(body.sport==='fishing'&&body.mode==='cup'?'cup':body.sport==='fishing'?'round':'match'))fail(400,'invalid_match');
    const team=body.sport==='football'||body.sport==='basketball',max=body.sport==='fishing'?5:10;
    if(!Array.isArray(body.players)||body.players.length<2||body.players.length>max)fail(400,'invalid_roster');
    const players=body.players.map(p=>({id:p?.id,team:p?.team})).sort((a,b)=>String(a.id).localeCompare(String(b.id)));
    if(players.some(p=>!validId(p.id)||!integer(p.team,2)||team&&p.team===0||!team&&p.team!==0)||new Set(players.map(p=>p.id)).size!==players.length)fail(400,'invalid_roster');
    if(team&&(players.filter(p=>p.team===1).length!==players.filter(p=>p.team===2).length||body.sport==='basketball'&&players.length<4))fail(400,'invalid_roster');
    const roster=JSON.stringify(players),old=await this.s.one('SELECT * FROM economy_matches WHERE host=? AND request_key=?',id,body.requestKey);
    if(old){if(old.roster!==roster||old.sport!==body.sport||old.mode!==body.mode||old.room!==body.room)fail(409,'match_changed');return {id:old.id,status:old.status};}
    for(const p of players)await this.ensure(p.id);
    const matchId=randomUUID(),now=this.clock();
    try{await this.s.db.batch([
      this.s.statement('INSERT INTO economy_matches(id,host,request_key,room,sport,mode,roster,created_at,expires_at) VALUES(?,?,?,?,?,?,?,?,?)',matchId,id,body.requestKey,body.room,body.sport,body.mode,roster,now,now+14400),
      ...players.map(p=>this.s.statement('INSERT INTO economy_participants(match_id,player_id) VALUES(?,?)',matchId,p.id))
    ]);}catch(e){
      const concurrent=await this.s.one('SELECT * FROM economy_matches WHERE host=? AND request_key=?',id,body.requestKey);
      if(concurrent&&concurrent.roster===roster&&concurrent.room===body.room&&concurrent.sport===body.sport&&concurrent.mode===body.mode)return {id:concurrent.id,status:concurrent.status};
      if(await this.s.one("SELECT 1 FROM economy_matches WHERE status='active' AND (host=? OR room=?)",id,body.room))fail(409,'match_busy');throw e;
    }
    return {id:matchId,status:'active'};
  }
  async match(id,body){
    if(!key(body.id))fail(400,'invalid_match');
    const m=await this.s.one('SELECT * FROM economy_matches WHERE id=?',body.id);
    if(!m||m.host!==id&&!await this.s.one('SELECT 1 FROM economy_participants WHERE match_id=? AND player_id=?',m.id,id))fail(404,'match_not_found');
    return m;
  }
  async lookup(id,body){
    if(!key(body.requestKey))fail(400,'invalid_match');
    const m=await this.s.one('SELECT id,status FROM economy_matches WHERE host=? AND request_key=?',id,body.requestKey);
    if(!m)fail(404,'match_not_found');return m;
  }
  async join(id,body){
    await this.expire();const m=await this.match(id,body);
    if(m.room!==body.room)fail(403,'wrong_room');
    if(m.status!=='active')return {id:m.id,status:m.status};
    try{await this.s.statement('UPDATE economy_participants SET joined=1 WHERE match_id=? AND player_id=?',m.id,id).run();}
    catch(e){if(String(e).includes('economy_player_busy'))fail(409,'player_busy');throw e;}
    return {id:m.id,status:m.status};
  }
  async finish(id,body){
    await this.expire();const m=await this.match(id,body);if(m.host!==id)fail(403,'host_required');
    if(body.cancelled===true){
      await this.s.statement("UPDATE economy_matches SET status='cancelled' WHERE id=? AND status='active'",m.id).run();
      return {id:m.id,status:(await this.s.one('SELECT status FROM economy_matches WHERE id=?',m.id)).status};
    }
    const awards=payouts(m,body).sort((a,b)=>a.id.localeCompare(b.id));
    const canonical=JSON.stringify({players:[...body.players].sort((a,b)=>a.id.localeCompare(b.id)),scoreA:body.scoreA,scoreB:body.scoreB});
    const hash=createHash('sha256').update(canonical).digest('hex');
    if(m.status==='finished'){if(hash!==m.result_hash)fail(409,'match_changed');return JSON.parse(m.receipt);}
    if(m.status!=='active')fail(409,'match_cancelled');
    if(this.clock()-m.created_at<10)fail(409,'match_too_short');
    const joined=await this.s.rows('SELECT player_id FROM economy_participants WHERE match_id=? AND joined=1',m.id);
    if(JSON.parse(m.roster).some(p=>!joined.some(j=>j.player_id===p.id)))fail(409,'participants_pending');
    const receipt={id:m.id,status:'finished',awards},now=this.clock();
    await this.s.db.batch([
      this.s.statement("UPDATE economy_matches SET status='finished',result_hash=?,receipt=? WHERE id=? AND status='active'",hash,JSON.stringify(receipt),m.id),
      ...awards.map(p=>this.s.statement(`INSERT OR IGNORE INTO coin_transactions(player_id,reward_key,amount,reason,created_at)
        SELECT ?,?,?,?,? WHERE EXISTS(SELECT 1 FROM economy_matches WHERE id=? AND status='finished' AND result_hash=?)`,p.id,`match:${m.id}`,p.amount,m.sport,now,m.id,hash))
    ]);
    const saved=await this.s.one('SELECT * FROM economy_matches WHERE id=?',m.id);
    if(saved.result_hash!==hash)fail(409,'match_changed');
    return JSON.parse(saved.receipt);
  }
}
