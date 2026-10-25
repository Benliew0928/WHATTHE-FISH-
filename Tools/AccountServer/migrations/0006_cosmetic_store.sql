-- Every paid cosmetic has one permanent, canonical purchase receipt.
-- The debit trigger from 0005 and this grant trigger share one SQL statement.
CREATE TRIGGER cosmetic_purchase AFTER INSERT ON coin_transactions
WHEN NEW.reason='purchase' BEGIN
  INSERT INTO cosmetic_inventory(player_id,item_id)
    VALUES(NEW.player_id,substr(NEW.reward_key,10));
END;
-- Legacy automatic grants are reconciled by Economy.ensure against receipts.
UPDATE profiles SET avatar=0,avatar_version=avatar_version+1,revision=revision+1 WHERE avatar>=2;
