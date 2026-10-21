CREATE TABLE profiles (
  id TEXT PRIMARY KEY,
  code TEXT NOT NULL UNIQUE,
  name TEXT NOT NULL,
  normalized TEXT NOT NULL,
  revision INTEGER NOT NULL DEFAULT 1,
  avatar_version INTEGER NOT NULL DEFAULT 0,
  avatar INTEGER NOT NULL DEFAULT 0,
  has_photo INTEGER NOT NULL DEFAULT 0,
  pixels BLOB CHECK(pixels IS NULL OR length(pixels)=49152),
  terms_json TEXT NOT NULL DEFAULT '[]',
  grams_json TEXT NOT NULL DEFAULT '[]',
  deletes_json TEXT NOT NULL DEFAULT '[]',
  hidden INTEGER NOT NULL DEFAULT 0,
  created_at INTEGER NOT NULL,
  updated_at INTEGER NOT NULL
);
CREATE INDEX profile_name ON profiles(normalized, id);
CREATE TABLE search_terms (
  term TEXT NOT NULL,
  player_id TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  PRIMARY KEY(term, player_id)
);
CREATE TABLE search_grams (
  gram TEXT NOT NULL,
  player_id TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  PRIMARY KEY(gram, player_id)
);
CREATE TABLE search_deletes (
  term TEXT NOT NULL,
  player_id TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  PRIMARY KEY(term,player_id)
);
CREATE TRIGGER profile_search_insert AFTER INSERT ON profiles BEGIN
  INSERT INTO search_terms SELECT value,NEW.id FROM json_each(NEW.terms_json);
  INSERT INTO search_grams SELECT value,NEW.id FROM json_each(NEW.grams_json);
  INSERT INTO search_deletes SELECT value,NEW.id FROM json_each(NEW.deletes_json);
END;
CREATE TRIGGER profile_search_update AFTER UPDATE OF normalized ON profiles BEGIN
  DELETE FROM search_terms WHERE player_id=NEW.id;
  DELETE FROM search_grams WHERE player_id=NEW.id;
  DELETE FROM search_deletes WHERE player_id=NEW.id;
  INSERT INTO search_terms SELECT value,NEW.id FROM json_each(NEW.terms_json);
  INSERT INTO search_grams SELECT value,NEW.id FROM json_each(NEW.grams_json);
  INSERT INTO search_deletes SELECT value,NEW.id FROM json_each(NEW.deletes_json);
END;
CREATE INDEX term_owner ON search_terms(player_id);
CREATE INDEX gram_owner ON search_grams(player_id);
CREATE INDEX delete_owner ON search_deletes(player_id);
CREATE TABLE relationships (
  low_id TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  high_id TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  sender TEXT NOT NULL,
  state TEXT NOT NULL CHECK(state IN ('pending','accepted','declined')),
  updated_at INTEGER NOT NULL,
  PRIMARY KEY(low_id, high_id),
  CHECK(low_id < high_id),
  CHECK(sender=low_id OR sender=high_id)
);
CREATE INDEX relationships_high ON relationships(high_id, state);
CREATE INDEX relationships_low ON relationships(low_id, state);
CREATE TABLE blocks (
  owner TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  target TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  PRIMARY KEY(owner,target),
  CHECK(owner<>target)
);
CREATE TRIGGER relationship_block_insert BEFORE INSERT ON relationships
WHEN EXISTS(SELECT 1 FROM blocks WHERE (owner=NEW.low_id AND target=NEW.high_id) OR (owner=NEW.high_id AND target=NEW.low_id))
BEGIN SELECT RAISE(ABORT,'player_unavailable'); END;
CREATE TRIGGER relationship_block_update BEFORE UPDATE ON relationships
WHEN EXISTS(SELECT 1 FROM blocks WHERE (owner=NEW.low_id AND target=NEW.high_id) OR (owner=NEW.high_id AND target=NEW.low_id))
BEGIN SELECT RAISE(ABORT,'player_unavailable'); END;
CREATE TABLE reports (
  reporter TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  target TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  reason TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  PRIMARY KEY(reporter,target)
);
CREATE TABLE room_proofs (
  digest TEXT PRIMARY KEY,
  player_id TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  room TEXT NOT NULL,
  challenge TEXT NOT NULL,
  expires_at INTEGER NOT NULL
);
CREATE INDEX proof_expiry ON room_proofs(expires_at);
CREATE INDEX proof_player ON room_proofs(player_id);
