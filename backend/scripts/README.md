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

## Create Course via Backend API

Script: `create_course_via_api.py`

Run from repository root:

```powershell
python .\backend\scripts\create_course_via_api.py --base-url https://localhost:1917 --title "HSK 3 - Grammar" --description "Course created by script" --hsk-level 3 --identity-cookie "HanziAnhVu.Identity=<your-jwt-cookie-value>" --insecure
```

You can also pass a full Set-Cookie line copied from browser devtools:

```powershell
python .\backend\scripts\create_course_via_api.py --title "HSK 2" --description "Cookie test" --hsk-level 2 --identity-cookie "HanziAnhVu.Identity=eyJ...; Path=/; Secure; HttpOnly" --insecure
```

Alternative cookie options:

- `--cookie "name=value"` (can be repeated)
- `--cookie-header "name1=value1; name2=value2"`
- `--refresh-cookie "HanziAnhVu.Refresh=..."`

## Create Topic via Backend API

Script: `create_topic_via_api.py`

Important: `--course-id` is required.

Run from repository root:

```powershell
python .\backend\scripts\create_topic_via_api.py --course-id "<course-guid>" --identity-cookie "HanziAnhVu.Identity=<your-jwt-cookie-value>" --insecure
```

Create multiple topics with auto-generated payload:

```powershell
python .\backend\scripts\create_topic_via_api.py --course-id "<course-guid>" --count 5 --identity-cookie "HanziAnhVu.Identity=<your-jwt-cookie-value>" --insecure
```

Create exam topics:

```powershell
python .\backend\scripts\create_topic_via_api.py --course-id "<course-guid>" --topic-type Exam --exam-year 2026 --exam-code "HSK-MOCK-01" --count 2 --identity-cookie "HanziAnhVu.Identity=<your-jwt-cookie-value>" --insecure
```

Preview payload only:

```powershell
python .\backend\scripts\create_topic_via_api.py --course-id "<course-guid>" --count 3 --dry-run
```
