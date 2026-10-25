CREATE TABLE wallets (
  player_id TEXT PRIMARY KEY REFERENCES profiles(id) ON DELETE CASCADE,
  balance INTEGER NOT NULL DEFAULT 0 CHECK(balance>=0 AND balance<=9000000000000),
  revision INTEGER NOT NULL DEFAULT 0,
  last_day INTEGER NOT NULL DEFAULT -1,
  streak INTEGER NOT NULL DEFAULT 0 CHECK(streak BETWEEN 0 AND 7)
);
CREATE TABLE coin_transactions (
  player_id TEXT NOT NULL REFERENCES wallets(player_id) ON DELETE CASCADE,
  reward_key TEXT NOT NULL,
  amount INTEGER NOT NULL,
  reason TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  reward_day INTEGER NOT NULL DEFAULT -1,
  cycle_day INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY(player_id,reward_key)
);
CREATE INDEX coin_history ON coin_transactions(player_id,created_at DESC);
CREATE TRIGGER coin_credit AFTER INSERT ON coin_transactions BEGIN
  UPDATE wallets SET balance=balance+NEW.amount,revision=revision+1,
    last_day=CASE WHEN NEW.reason='daily' THEN NEW.reward_day ELSE last_day END,
    streak=CASE WHEN NEW.reason='daily' THEN NEW.cycle_day ELSE streak END
    WHERE player_id=NEW.player_id;
END;
CREATE TABLE cosmetic_inventory (
  player_id TEXT NOT NULL REFERENCES wallets(player_id) ON DELETE CASCADE,
  item_id TEXT NOT NULL,
  PRIMARY KEY(player_id,item_id)
);
CREATE TABLE cosmetic_equipment (
  player_id TEXT NOT NULL REFERENCES wallets(player_id) ON DELETE CASCADE,
  slot TEXT NOT NULL,
  item_id TEXT NOT NULL,
  PRIMARY KEY(player_id,slot),
  FOREIGN KEY(player_id,item_id) REFERENCES cosmetic_inventory(player_id,item_id) ON DELETE CASCADE
);
CREATE TABLE economy_matches (
  id TEXT PRIMARY KEY,
  host TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  request_key TEXT NOT NULL,
  room TEXT NOT NULL,
  sport TEXT NOT NULL,
  mode TEXT NOT NULL,
  roster TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  expires_at INTEGER NOT NULL,
  status TEXT NOT NULL DEFAULT 'active' CHECK(status IN ('active','finished','cancelled')),
  result_hash TEXT NOT NULL DEFAULT '',
  receipt TEXT NOT NULL DEFAULT '',
  UNIQUE(host,request_key)
);
CREATE UNIQUE INDEX economy_active_room ON economy_matches(room) WHERE status='active';
CREATE UNIQUE INDEX economy_active_host ON economy_matches(host) WHERE status='active';
CREATE TABLE economy_participants (
  match_id TEXT NOT NULL REFERENCES economy_matches(id) ON DELETE CASCADE,
  player_id TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  joined INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY(match_id,player_id)
);
CREATE INDEX economy_player_matches ON economy_participants(player_id,match_id);
CREATE TRIGGER economy_single_match BEFORE UPDATE OF joined ON economy_participants
WHEN NEW.joined=1 AND EXISTS(
  SELECT 1 FROM economy_participants p JOIN economy_matches m ON p.match_id=m.id
  WHERE p.player_id=NEW.player_id AND p.match_id<>NEW.match_id AND p.joined=1 AND m.status='active'
) BEGIN SELECT RAISE(ABORT,'economy_player_busy'); END;
