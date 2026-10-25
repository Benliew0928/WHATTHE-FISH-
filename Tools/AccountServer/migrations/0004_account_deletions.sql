-- Minimal pseudonymous revocation markers prevent an unexpired old token or
-- concurrent profile write from recreating a deleted profile. No name/photo/token.
CREATE TABLE account_deletions (
  identity_hash TEXT PRIMARY KEY,
  requested_at INTEGER NOT NULL
);
