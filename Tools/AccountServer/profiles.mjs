import { createHash, randomBytes } from 'node:crypto';
import { AuthError } from './auth.mjs';
import { validateName, normalize, grams, deletions, rank } from './profile-search.mjs';
import { deletionHash } from './account-deletion.mjs';
import { submitReport } from './reports.mjs';
export const columns='id,code,name,revision,avatar_version AS avatarVersion,avatar,has_photo AS hasPhoto,hidden';
const bad=(status,code)=>{throw new AuthError(status,code);};
const now=()=>Math.floor(Date.now()/1000);
const pair=(a,b)=>a<b?[a,b]:[b,a];
const validId=id=>typeof id==='string'&&/^[a-zA-Z0-9_-]{16,128}$/.test(id);
const digest=s=>createHash('sha256').update(s).digest('hex');
export class Profiles {
  constructor(db){this.db=db;}
  statement(sql,...values){return this.db.prepare(sql).bind(...values);}
  async one(sql,...values){return this.statement(sql,...values).first();}
  async rows(sql,...values){return (await this.statement(sql,...values).all()).results;}
  async mine(id) { return await this.one(`SELECT ${columns} FROM profiles WHERE id=?`,id); }
  async require(id) { const p=await this.mine(id);if(!p||p.hidden)bad(404,'profile_not_found');return p; }
  async blocked(a,b) {
    return !!await this.one('SELECT 1 FROM blocks WHERE (owner=? AND target=?) OR (owner=? AND target=?)',a,b,b,a);
  }
  async relation(a,b) {
    if(a===b)return 'self';
    if(await this.blocked(a,b))return 'blocked';
    const r=await this.one('SELECT sender,state FROM relationships WHERE low_id=? AND high_id=?',...pair(a,b));
    return !r||r.state==='declined'?'none':r.state==='accepted'?'friends':r.sender===a?'outgoing':'incoming';
  }
  async get(viewer,id) {
    if(!validId(id))bad(400,'invalid_player');
    if(await this.blocked(viewer,id))bad(404,'profile_not_found');
    return {...await this.require(id),relationship:await this.relation(viewer,id)};
  }
  async save(id,body) {
    const name=validateName(body.name),normalized=normalize(name);
    if(!Number.isSafeInteger(body.revision)||body.revision<0)bad(400,'invalid_revision');
    const avatar=body.avatar;
    if(!Number.isInteger(avatar)||avatar<0||avatar>9)bad(400,'invalid_avatar');
    let pixels=null,change=body.photo!==undefined;
    if(change&&body.photo!==null&&body.photo!=='') {
      // Fixed RGB24 pixels: no compressed decoder, EXIF, scripts, external URL or oversized dimensions.
      if(typeof body.photo!=='string'||body.photo.length!==65536||!/^[A-Za-z0-9+/]+$/.test(body.photo))bad(400,'invalid_photo');
      pixels=Buffer.from(body.photo,'base64');
      if(pixels.length!==49152)bad(400,'invalid_photo');
    }
    const terms=JSON.stringify([...new Set(normalized.split(' '))]),g=JSON.stringify(grams(normalized)),del=JSON.stringify(deletions(normalized)),time=now();
    let result;
    if(body.revision===0) {
      const code=randomBytes(8).toString('hex').toUpperCase();
      result=await this.one(`INSERT INTO profiles(id,code,name,normalized,avatar,pixels,has_photo,avatar_version,terms_json,grams_json,deletes_json,created_at,updated_at)
        SELECT ?,?,?,?,?,?,?,?,?,?,?,?,? WHERE NOT EXISTS(SELECT 1 FROM account_deletions WHERE identity_hash=?)
        ON CONFLICT(id) DO NOTHING RETURNING ${columns}`,
        id,code,name,normalized,avatar,pixels?Array.from(pixels):null,pixels?1:0,pixels?1:0,terms,g,del,time,time,deletionHash(id));
    } else {
      result=await this.one(`UPDATE profiles SET name=?,normalized=?,avatar=?,has_photo=CASE WHEN ? THEN ? ELSE has_photo END,pixels=CASE WHEN ? THEN ? ELSE pixels END,
        avatar_version=avatar_version+CASE WHEN ? OR avatar<>? THEN 1 ELSE 0 END,
        revision=revision+1,terms_json=?,grams_json=?,deletes_json=?,updated_at=? WHERE id=? AND revision=? AND hidden=0 RETURNING ${columns}`,
        name,normalized,avatar,change?1:0,pixels?1:0,change?1:0,pixels?Array.from(pixels):null,change?1:0,avatar,terms,g,del,time,id,body.revision);
    }
    if(!result)bad(409,'profile_changed');
    return result;
  }
  async photo(viewer,id,version) {
    if(await this.blocked(viewer,id))bad(404,'photo_not_found');
    const row=await this.one('SELECT pixels FROM profiles WHERE id=? AND hidden=0 AND avatar_version=?',id,version);
    if(!row?.pixels)bad(404,'photo_not_found');
    return new Uint8Array(row.pixels);
  }
  async search(viewer,query,offset=0) {
    if(typeof query!=='string'||query.length>64||!Number.isInteger(offset)||offset<0||offset>10000)bad(400,'invalid_search');
    const q=normalize(query);
    if(!q || q.length>32)return {players:[],more:false,offset};
    const code=query.replace(/^#/,'').trim().toUpperCase();
    const exactCode=await this.one(`SELECT ${columns} FROM profiles WHERE code=? AND hidden=0`,code);
    if(exactCode) {
      if(exactCode.id===viewer||await this.blocked(viewer,exactCode.id))return {players:[],more:false,offset:0};
      return {players:[{...exactCode,match:'code',relationship:await this.relation(viewer,exactCode.id)}],more:false,offset:0};
    }
    // Indexed ranges avoid LIKE wildcards and full-table substring scans.
    const exclusion=`p.id<>? AND p.hidden=0 AND NOT EXISTS(SELECT 1 FROM blocks b WHERE
      (b.owner=? AND b.target=p.id) OR (b.owner=p.id AND b.target=?))`;
    const direct=await this.rows(`SELECT ${columns.split(',').map(c=>'p.'+c).join(',')} FROM profiles p
      WHERE p.normalized>=? AND p.normalized<? AND ${exclusion}
      ORDER BY CASE WHEN p.normalized=? THEN 0 ELSE 1 END,p.normalized,p.code LIMIT 21 OFFSET ?`,
      q,q+'\uffff',viewer,viewer,viewer,q,offset);
    let more=direct.length>20,selected=direct.slice(0,20);
    if(!more&&offset===0&&selected.length<20) {
      const candidates=await this.rows(`SELECT DISTINCT ${columns.split(',').map(c=>'p.'+c).join(',')} FROM search_terms t
        JOIN profiles p ON p.id=t.player_id WHERE t.term>=? AND t.term<? AND ${exclusion}
        ORDER BY p.normalized,p.code LIMIT 80`,q,q+'\uffff',viewer,viewer,viewer);
      const terms=grams(q).slice(0,12);
      if(q.length>=4) {
        const keys=deletions(q).slice(0,34);
        candidates.push(...await this.rows(`SELECT DISTINCT ${columns.split(',').map(c=>'p.'+c).join(',')} FROM search_deletes d
          JOIN profiles p ON p.id=d.player_id WHERE d.term IN (${keys.map(()=>'?').join(',')}) AND ${exclusion}
          ORDER BY p.normalized,p.code LIMIT 80`,...keys,viewer,viewer,viewer));
      }
      if(q.length>=4&&terms.length){
        const near=await this.rows(`SELECT ${columns.split(',').map(c=>'p.'+c).join(',')},COUNT(*) AS hits FROM search_grams g
          JOIN profiles p ON p.id=g.player_id WHERE g.gram IN (${terms.map(()=>'?').join(',')}) AND ${exclusion}
          GROUP BY p.id ORDER BY hits DESC,p.code LIMIT 80`,...terms,viewer,viewer,viewer);
        candidates.push(...near);
      }
      const seen=new Set(selected.map(p=>p.id));
      selected.push(...candidates.filter(p=>{if(seen.has(p.id)||!Number.isFinite(rank(q,p.name)))return false;seen.add(p.id);return true;})
        .sort((a,b)=>rank(q,a.name)-rank(q,b.name)||a.code.localeCompare(b.code)).slice(0,20-selected.length));
    }
    const states=await this.relationships(viewer);
    const relations=new Map([...states.friends,...states.incoming,...states.outgoing].map(p=>[p.id,p.relationship]));
    return {players:selected.map(p=>({...p,hidden:undefined,hits:undefined,
      match:rank(q,p.name)<100?'exact':rank(q,p.name)<300?'prefix':'similar',
      relationship:relations.get(p.id)||'none'})),more,offset:offset+20};
  }
  async relationships(id) {
    const rows=await this.rows(`SELECT ${columns.split(',').map(c=>'p.'+c).join(',')},r.sender,r.state
      FROM relationships r JOIN profiles p ON p.id=CASE WHEN r.low_id=? THEN r.high_id ELSE r.low_id END
      WHERE (r.low_id=? OR r.high_id=?) AND r.state IN ('pending','accepted') AND p.hidden=0
      ORDER BY r.updated_at DESC LIMIT 251`,id,id,id);
    const result={friends:[],incoming:[],outgoing:[]};
    for(const p of rows) {
      const key=p.state==='accepted'?'friends':p.sender===id?'outgoing':'incoming';
      const {sender,state,hidden,...profile}=p;
      result[key].push({...profile,relationship:key==='friends'?'friends':key});
    }
    return result;
  }
  async friend(id,target,action) {
    if(!validId(target)||target===id)bad(400,'invalid_player');
    await this.require(id);await this.require(target);
    if(await this.blocked(id,target))bad(403,'player_unavailable');
    const [lo,hi]=pair(id,target),time=now();
    if(action==='request') {
      // One canonical pair, atomic guarded insert. Retrying never creates duplicates;
      // crossed requests remain a request until its recipient explicitly accepts.
      const existing=await this.one('SELECT * FROM relationships WHERE low_id=? AND high_id=?',lo,hi);
      if(existing?.state==='accepted'||existing?.state==='pending')return this.get(id,target);
      if(existing&&time-existing.updated_at<86400)bad(429,'request_cooldown');
      const count=await this.one(`SELECT COUNT(*) AS n FROM relationships WHERE
        (low_id=? OR high_id=?) AND state IN ('pending','accepted')`,id,id);
      const other=await this.one(`SELECT COUNT(*) AS n FROM relationships WHERE
        (low_id=? OR high_id=?) AND state IN ('pending','accepted')`,target,target);
      if(count.n>=150||other.n>=150)bad(409,'friend_limit');
      await this.statement(`INSERT INTO relationships(low_id,high_id,sender,state,updated_at)
        SELECT ?,?,?,'pending',? WHERE (SELECT COUNT(*) FROM relationships WHERE (low_id=? OR high_id=?) AND state IN ('pending','accepted'))<150
        AND (SELECT COUNT(*) FROM relationships WHERE (low_id=? OR high_id=?) AND state IN ('pending','accepted'))<150
        ON CONFLICT(low_id,high_id) DO UPDATE SET sender=excluded.sender,state='pending',updated_at=excluded.updated_at
        WHERE relationships.state='declined' AND relationships.updated_at<=?`,lo,hi,id,time,id,id,target,target,time-86400).run();
      if(await this.relation(id,target)==='none')bad(409,'friend_limit');
    } else if(action==='accept') {
      await this.statement(`UPDATE relationships SET state='accepted',updated_at=? WHERE low_id=? AND high_id=?
        AND state='pending' AND sender<>?`,time,lo,hi,id).run();
    } else if(action==='decline') {
      await this.statement(`UPDATE relationships SET state='declined',updated_at=? WHERE low_id=? AND high_id=?
        AND state='pending' AND sender<>?`,time,lo,hi,id).run();
    } else if(action==='cancel') {
      await this.statement(`UPDATE relationships SET state='declined',updated_at=? WHERE low_id=? AND high_id=?
        AND state='pending' AND sender=?`,time,lo,hi,id).run();
    } else if(action==='remove') {
      await this.statement(`UPDATE relationships SET state='declined',updated_at=? WHERE low_id=? AND high_id=? AND state='accepted'`,time,lo,hi).run();
    } else bad(400,'invalid_action');
    return this.get(id,target);
  }
  async block(id,target) {
    if(!validId(target)||id===target)bad(400,'invalid_player');
    await this.require(target);
    const count=await this.one('SELECT COUNT(*) AS n FROM blocks WHERE owner=?',id);
    if(count.n>=200)bad(409,'block_limit');
    await this.db.batch([
      this.statement('INSERT OR IGNORE INTO blocks(owner,target) SELECT ?,? WHERE (SELECT COUNT(*) FROM blocks WHERE owner=?)<200',id,target,id),
      this.statement('DELETE FROM relationships WHERE low_id=? AND high_id=? AND EXISTS(SELECT 1 FROM blocks WHERE owner=? AND target=?)',...pair(id,target),id,target)
    ]);
    if(!await this.one('SELECT 1 FROM blocks WHERE owner=? AND target=?',id,target))bad(409,'block_limit');
    return {ok:true};
  }
  async report(id,target,reason) {return submitReport(this,id,{target,reason});}
  async proof(id,room,challenge) {
    await this.require(id);
    if(typeof room!=='string'||room.length>128||!room||!/^[a-f0-9]{32}$/.test(challenge||''))bad(400,'invalid_proof');
    const token=randomBytes(32).toString('hex');
    await this.db.batch([
      this.statement('DELETE FROM room_proofs WHERE player_id=? OR expires_at<?',id,now()),
      this.statement('INSERT INTO room_proofs VALUES(?,?,?,?,?)',digest(token),id,room,challenge,now()+90)
    ]);
    return {proof:token};
  }
  async verifyProof(room,challenge,proof) {
    if(!/^[a-f0-9]{64}$/.test(proof||''))bad(401,'invalid_proof');
    const row=await this.one('DELETE FROM room_proofs WHERE digest=? AND room=? AND challenge=? AND expires_at>? RETURNING player_id',digest(proof),room,challenge,now());
    if(!row)bad(401,'invalid_proof');
    return this.require(row.player_id);
  }
}
