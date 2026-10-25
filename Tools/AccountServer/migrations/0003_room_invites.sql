CREATE TABLE room_invites (
  id TEXT PRIMARY KEY,
  sender TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  recipient TEXT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  room_code TEXT NOT NULL,
  sport INTEGER NOT NULL CHECK(sport BETWEEN 0 AND 3),
  protocol INTEGER NOT NULL,
  created_at INTEGER NOT NULL,
  expires_at INTEGER NOT NULL,
  state TEXT NOT NULL DEFAULT 'pending' CHECK(state IN ('pending','declined','joined','cancelled')),
  UNIQUE(sender,recipient)
);
CREATE INDEX room_invites_inbox ON room_invites(recipient,state,expires_at);
CREATE INDEX room_invites_outbox ON room_invites(sender,state,expires_at);
-- Removing/blocking a friend must revoke old invitations even if they become friends again.
CREATE TRIGGER room_invites_friend_removed AFTER DELETE ON relationships BEGIN
 DELETE FROM room_invites WHERE (sender=old.low_id AND recipient=old.high_id) OR (sender=old.high_id AND recipient=old.low_id);
END;
CREATE TRIGGER room_invites_friend_changed AFTER UPDATE OF state ON relationships WHEN new.state<>'accepted' BEGIN
 DELETE FROM room_invites WHERE (sender=new.low_id AND recipient=new.high_id) OR (sender=new.high_id AND recipient=new.low_id);
END;
