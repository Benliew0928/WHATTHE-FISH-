import { createHash } from 'node:crypto';
export const deletionHash = id => createHash('sha256').update('whatthefish/account-deletion/v1/' + id).digest('hex');
export async function deletionPending(store,id) {
  return !!await store.one('SELECT 1 FROM account_deletions WHERE identity_hash=?',deletionHash(id));
}
export async function deleteProfileForAccount(store,id) {
  // D1 batches are transactional. Mark first, then delete all FK-owned data.
  await store.db.batch([
    store.statement('INSERT OR IGNORE INTO account_deletions(identity_hash,requested_at) VALUES(?,?)',deletionHash(id),Math.floor(Date.now()/1000)),
    store.statement('DELETE FROM profiles WHERE id=?',id)
  ]);
  return {version:1,profileDeleted:true};
}
