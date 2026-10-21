-- Photo-only changes do not rebuild the name-search indexes.
DROP TRIGGER profile_search_update;
CREATE TRIGGER profile_search_update AFTER UPDATE OF normalized ON profiles
WHEN NEW.normalized<>OLD.normalized BEGIN
  DELETE FROM search_terms WHERE player_id=NEW.id;
  DELETE FROM search_grams WHERE player_id=NEW.id;
  DELETE FROM search_deletes WHERE player_id=NEW.id;
  INSERT INTO search_terms SELECT value,NEW.id FROM json_each(NEW.terms_json);
  INSERT INTO search_grams SELECT value,NEW.id FROM json_each(NEW.grams_json);
  INSERT INTO search_deletes SELECT value,NEW.id FROM json_each(NEW.deletes_json);
END;
CREATE INDEX blocks_target ON blocks(target);
CREATE INDEX reports_target ON reports(target);
