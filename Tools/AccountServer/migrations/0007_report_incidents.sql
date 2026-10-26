-- Keep legacy reports recoverable; new submissions are incident-based.
CREATE TABLE report_incidents (
 id TEXT PRIMARY KEY,
 reporter TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
 target TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
 request_id TEXT NOT NULL,
 fingerprint TEXT NOT NULL,
 reason TEXT NOT NULL,
 details TEXT NOT NULL DEFAULT '',
 context TEXT NOT NULL DEFAULT '',
 evidence TEXT NOT NULL DEFAULT '',
 target_name TEXT NOT NULL,
 target_code TEXT NOT NULL,
 target_revision INTEGER NOT NULL,
 target_avatar_version INTEGER NOT NULL,
 target_pixels BLOB,
 created_at INTEGER NOT NULL,
 status TEXT NOT NULL DEFAULT 'pending' CHECK(status IN ('pending','reviewed','dismissed')),
 review_note TEXT NOT NULL DEFAULT '',
 reviewed_at INTEGER,
 UNIQUE(reporter,request_id)
);
INSERT INTO report_incidents(id,reporter,target,request_id,fingerprint,reason,target_name,target_code,target_revision,target_avatar_version,created_at)
 SELECT 'legacy-'||hex(randomblob(16)),r.reporter,r.target,'legacy-'||r.target,'legacy',r.reason,p.name,p.code,p.revision,p.avatar_version,r.created_at
 FROM reports r JOIN profiles p ON p.id=r.target;
CREATE INDEX report_incidents_queue ON report_incidents(status,created_at);
CREATE INDEX report_incidents_rate ON report_incidents(reporter,created_at);
CREATE INDEX report_incidents_target ON report_incidents(target,created_at);
