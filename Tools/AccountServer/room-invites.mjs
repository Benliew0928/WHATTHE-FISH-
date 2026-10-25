import { randomBytes } from 'node:crypto';
import { AuthError } from './auth.mjs';
const fail=(status,code)=>{throw new AuthError(status,code);};
export class RoomInvites {
  constructor(profiles,clock=()=>Math.floor(Date.now()/1000)){this.p=profiles;this.clock=clock;}
  async list(owner){
    const time=this.clock();
    const invites=await this.p.rows(`SELECT i.id,i.sender,i.recipient,i.sport,i.protocol,i.expires_at AS expiresAt,
      p.name,p.code,p.avatar,p.avatar_version AS avatarVersion,p.has_photo AS hasPhoto
      FROM room_invites i JOIN profiles p ON p.id=i.sender
      JOIN relationships r ON r.low_id=min(i.sender,i.recipient) AND r.high_id=max(i.sender,i.recipient) AND r.state='accepted'
      WHERE i.recipient=? AND i.state='pending' AND i.expires_at>? AND p.hidden=0
      AND NOT EXISTS(SELECT 1 FROM blocks b WHERE (b.owner=i.sender AND b.target=i.recipient) OR (b.owner=i.recipient AND b.target=i.sender))
      ORDER BY i.created_at DESC LIMIT 20`,owner,time);
    return {invites,serverTime:time};
  }
  async send(owner,b){
    if(typeof b.target!=='string'||b.target===owner||!/^[A-Za-z0-9_-]{16,128}$/.test(b.target))fail(400,'invalid_player');
    if(typeof b.roomCode!=='string'||!/^[A-Z0-9]{4,12}$/.test(b.roomCode)||!Number.isInteger(b.sport)||b.sport<0||b.sport>3||!Number.isInteger(b.protocol)||b.protocol<1||b.protocol>65535)fail(400,'invalid_invitation');
    await this.p.require(owner);await this.p.require(b.target);
    if(await this.p.relation(owner,b.target)!=='friends')fail(403,'invite_friends_only');
    const time=this.clock(),previous=await this.p.one('SELECT created_at FROM room_invites WHERE sender=? AND recipient=?',owner,b.target);
    if(previous&&previous.created_at>time-30)fail(429,'invite_cooldown');
    const result=await this.p.one(`INSERT INTO room_invites(id,sender,recipient,room_code,sport,protocol,created_at,expires_at)
      SELECT ?,?,?,?,?,?,?,? WHERE
      (SELECT COUNT(*) FROM room_invites WHERE recipient=? AND state='pending' AND expires_at>?)<20 AND
      (SELECT COUNT(*) FROM room_invites WHERE sender=? AND state='pending' AND expires_at>?)<20
      ON CONFLICT(sender,recipient) DO UPDATE SET id=excluded.id,room_code=excluded.room_code,sport=excluded.sport,protocol=excluded.protocol,
      created_at=excluded.created_at,expires_at=excluded.expires_at,state='pending' WHERE room_invites.created_at<=?
      RETURNING id,expires_at AS expiresAt`,randomBytes(16).toString('hex'),owner,b.target,b.roomCode,b.sport,b.protocol,time,time+120,b.target,time,owner,time,time-30);
    if(!result)fail(429,'invite_limit');
    return result;
  }
  async resolve(owner,id){
    const invite=await this.p.one("SELECT * FROM room_invites WHERE id=? AND recipient=? AND state='pending' AND expires_at>?",id,owner,this.clock());
    if(!invite||await this.p.relation(owner,invite.sender)!=='friends')fail(410,'invite_expired');
    // A room invitation is advisory. Unity's actual connection admission checks capacity,
    // room locks and protocol. Never accept arbitrary URLs or bypass room admission.
    return {roomCode:invite.room_code,protocol:invite.protocol,sport:invite.sport};
  }
  async respond(owner,b){
    if(!['declined','joined','cancelled'].includes(b.action)||typeof b.id!=='string')fail(400,'invalid_action');
    const field=b.action==='cancelled'?'sender':'recipient';
    await this.p.statement(`UPDATE room_invites SET state=? WHERE id=? AND ${field}=? AND state='pending'`,b.action,b.id,owner).run();
    return {ok:true};
  }
}
