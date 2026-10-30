// Owner-approved periods, 28 October 2026. Enabling the scheduled job is a separate rollout.
const day=86400;
export const retentionSeconds=Object.freeze({matches:30*day,evidence:30*day,reports:90*day});

export async function runRetentionCleanup(store,now=Math.floor(Date.now()/1000)) {
  if(!Number.isSafeInteger(now)||now<0)throw new TypeError('Invalid retention clock');
  const results=await store.db.batch([
    store.statement("UPDATE report_incidents SET details='',context='',evidence='',target_pixels=NULL,review_note='' WHERE status IN ('reviewed','dismissed') AND reviewed_at IS NOT NULL AND reviewed_at<=? AND (details<>'' OR context<>'' OR evidence<>'' OR target_pixels IS NOT NULL OR review_note<>'')",now-retentionSeconds.evidence),
    // Legacy markers have a corresponding migrated case; only closed cases expire.
    store.statement("DELETE FROM reports WHERE EXISTS (SELECT 1 FROM report_incidents i WHERE i.reporter=reports.reporter AND i.target=reports.target AND i.fingerprint='legacy' AND i.status IN ('reviewed','dismissed') AND i.reviewed_at IS NOT NULL AND i.reviewed_at<=?)",now-retentionSeconds.reports),
    store.statement("DELETE FROM report_incidents WHERE status IN ('reviewed','dismissed') AND reviewed_at IS NOT NULL AND reviewed_at<=?",now-retentionSeconds.reports),
    store.statement('DELETE FROM economy_matches WHERE expires_at<=?',now-retentionSeconds.matches)
  ]);
  const changes=i=>Number(results[i]?.meta?.changes||0);
  return {evidenceCleared:changes(0),legacyReportsDeleted:changes(1),reportsDeleted:changes(2),matchesDeleted:changes(3)};
}
