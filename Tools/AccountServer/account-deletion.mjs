import { createHash } from 'node:crypto';
export const deletionHash = id => createHash('sha256').update('whatthefish/account-deletion/v1/' + id).digest('hex');
export async function deletionPending(store,id) {
  return !!await store.one('SELECT 1 FROM account_deletions WHERE identity_hash=?',deletionHash(id));
}
export async function deleteProfileForAccount(store,id) {
  // Keep the other host's request key to prevent replay, but remove this player's
  // embedded IDs as well as normalized FK rows. A changed active roster cancels
  // rewards; it must never be finalized against a partially deleted roster.
  await store.db.batch([
    store.statement('INSERT OR IGNORE INTO account_deletions(identity_hash,requested_at) VALUES(?,?)',deletionHash(id),Math.floor(Date.now()/1000)),
    store.statement(`UPDATE economy_matches AS m SET
      roster=json((SELECT json_group_array(json(value)) FROM json_each(m.roster) WHERE json_extract(value,'$.id') IS NOT ?)),
      receipt=CASE WHEN m.receipt='' THEN '' ELSE json_set(m.receipt,'$.awards',json((SELECT json_group_array(json(value)) FROM json_each(m.receipt,'$.awards') WHERE json_extract(value,'$.id') IS NOT ?))) END,
      result_hash='',status=CASE WHEN m.status='active' THEN 'cancelled' ELSE m.status END
      WHERE m.host<>? AND (
        EXISTS(SELECT 1 FROM json_each(m.roster) WHERE json_extract(value,'$.id')=?) OR
        EXISTS(SELECT 1 FROM json_each(CASE WHEN m.receipt='' THEN '{}' ELSE m.receipt END,'$.awards') WHERE json_extract(value,'$.id')=?) OR
        EXISTS(SELECT 1 FROM economy_participants p WHERE p.match_id=m.id AND p.player_id=?))`,id,id,id,id,id,id),
    store.statement('DELETE FROM profiles WHERE id=?',id)
  ]);
  return {version:1,profileDeleted:true};
}
