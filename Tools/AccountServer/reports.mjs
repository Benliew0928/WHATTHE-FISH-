import {createHash,randomUUID} from 'node:crypto';
import {AuthError} from './auth.mjs';
export const reportReasons=['name','photo','abuse','sexual','behaviour','cheating','spam','other'];
const bad=(status,code)=>{throw new AuthError(status,code);};
function field(value,max){
 if(value===undefined)return '';
 if(typeof value!=='string'||value.length>max||/[\u0000-\u0008\u000b\u000c\u000e-\u001f]/u.test(value))bad(400,'invalid_report');
 return value.normalize('NFC').trim();
}
export async function submitReport(store,reporter,body){
 const target=body.target,reason=body.reason;
 if(typeof target!=='string'||!/^[a-zA-Z0-9_-]{16,128}$/.test(target)||target===reporter||!reportReasons.includes(reason))bad(400,'invalid_report');
 const details=field(body.details,500),context=field(body.context,160),evidence=field(body.evidence,2400);
 if(reason==='other'&&!details)bad(400,'report_details_required');
 if(body.version!==undefined&&body.version!==2)bad(400,'invalid_report');
 const requestId=body.requestId??randomUUID().replaceAll('-','');
 if(typeof requestId!=='string'||!/^[a-f0-9]{32}$/.test(requestId))bad(400,'invalid_report');
 const fingerprint=createHash('sha256').update(JSON.stringify({target,reason,details,context,evidence})).digest('hex');
 await store.require(reporter);
 const old=await store.one('SELECT id,fingerprint FROM report_incidents WHERE reporter=? AND request_id=?',reporter,requestId);
 if(old){if(old.fingerprint!==fingerprint)bad(409,'report_changed');return {ok:true,id:old.id,version:2};}
 await store.require(target);
 const time=Math.floor(Date.now()/1000),id=randomUUID();
 // Counts and insertion share one SQL statement, including under concurrent calls.
 const inserted=await store.one(`INSERT INTO report_incidents(id,reporter,target,request_id,fingerprint,reason,details,context,evidence,target_name,target_code,target_revision,target_avatar_version,target_pixels,created_at)
  SELECT ?,?,?,?,?,?,?,?,?,p.name,p.code,p.revision,p.avatar_version,CASE WHEN ?='photo' THEN p.pixels ELSE NULL END,?
  FROM profiles p WHERE p.id=? AND p.hidden=0
  AND (SELECT COUNT(*) FROM report_incidents WHERE reporter=? AND created_at>?)<20
  AND (SELECT COUNT(*) FROM report_incidents WHERE reporter=? AND status='pending')<200
  AND NOT EXISTS(SELECT 1 FROM report_incidents WHERE reporter=? AND target=? AND created_at>?)
  ON CONFLICT(reporter,request_id) DO NOTHING RETURNING id`,
 id,reporter,target,requestId,fingerprint,reason,details,context,evidence,reason,time,target,reporter,time-86400,reporter,reporter,target,time-60);
 if(!inserted){
  const raced=await store.one('SELECT id,fingerprint FROM report_incidents WHERE reporter=? AND request_id=?',reporter,requestId);
  if(raced){if(raced.fingerprint!==fingerprint)bad(409,'report_changed');return {ok:true,id:raced.id,version:2};}
  bad(429,'report_limit');
 }
 return {ok:true,id,version:2};
}
