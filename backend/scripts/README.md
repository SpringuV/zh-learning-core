# Backend Scripts

## Seed Elasticsearch users

Recommended script (Python): `seed_hanzi_users.py`

Run from repository root:

```powershell
python .\backend\scripts\seed_hanzi_users.py --count 200 --elastic-url http://localhost:9200 --username elastic --password "<elastic-password>"
```

If you run Elasticsearch via Aspire/Docker, host port is often dynamic (not always 9200).
Check actual port first:

```powershell
docker ps --format "table {{.Names}}\t{{.Ports}}"
```

Then pass the mapped port, for example `http://localhost:60351`.

If your Elasticsearch endpoint is HTTPS with self-signed certificate:

```powershell
python .\backend\scripts\seed_hanzi_users.py --count 200 --elastic-url https://localhost:9200 --username elastic --password "<elastic-password>" --insecure
```

Notes:

- Index default is `hanzi_users`.
- Script auto-creates the index if it does not exist.
- Script inserts documents with fields compatible with `UserSearch`.
