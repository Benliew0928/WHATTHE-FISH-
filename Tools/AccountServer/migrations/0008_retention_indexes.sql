CREATE INDEX economy_match_retention ON economy_matches(expires_at);
CREATE INDEX report_incidents_retention ON report_incidents(status,reviewed_at);
