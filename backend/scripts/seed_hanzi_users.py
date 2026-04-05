#!/usr/bin/env python3
import argparse
import base64
import datetime as dt
import json
import random
import ssl
import sys
import urllib.error
import urllib.request
import uuid


def build_auth_header(username: str, password: str) -> str:
    token = f"{username}:{password}".encode("ascii")
    return "Basic " + base64.b64encode(token).decode("ascii")


def http_request(method: str, url: str, headers: dict[str, str], body: bytes | None, insecure: bool):
    req = urllib.request.Request(url=url, method=method, headers=headers, data=body)
    context = ssl._create_unverified_context() if insecure else None
    with urllib.request.urlopen(req, context=context, timeout=30) as resp:
        return resp.getcode(), resp.read()


def index_exists(elastic_url: str, index_name: str, headers: dict[str, str], insecure: bool) -> bool:
    url = f"{elastic_url.rstrip('/')}/{index_name}"
    try:
        status, _ = http_request("HEAD", url, headers, None, insecure)
        return status == 200
    except urllib.error.HTTPError as ex:
        if ex.code == 404:
            return False
        raise


def create_index(elastic_url: str, index_name: str, headers: dict[str, str], insecure: bool) -> None:
    url = f"{elastic_url.rstrip('/')}/{index_name}"
    mapping = {
        "mappings": {
            "properties": {
                "id": {"type": "keyword"},
                "email": {"type": "keyword"},
                "username": {"type": "text"},
                "phoneNumber": {"type": "keyword", "null_value": ""},
                "isActive": {"type": "boolean"},
                "createdAt": {"type": "date"},
                "updatedAt": {"type": "date"},
                "currentLevel": {"type": "integer"},
                "lastLogin": {"type": "date"},
                "lastActivityAt": {"type": "date"},
                "avatarUrl": {"type": "keyword", "null_value": ""},
            }
        }
    }
    body = json.dumps(mapping).encode("utf-8")
    req_headers = dict(headers)
    req_headers["Content-Type"] = "application/json"
    http_request("PUT", url, req_headers, body, insecure)


def iso(dt_value: dt.datetime) -> str:
    return dt_value.replace(microsecond=0).isoformat() + "Z"


def fake_user(number: int) -> dict:
    user_id = str(uuid.uuid4())
    level = random.randint(0, 10)
    created_at = dt.datetime.utcnow() - dt.timedelta(
        days=random.randint(0, 120),
        minutes=random.randint(0, 1440),
    )
    updated_at = created_at + dt.timedelta(days=random.randint(0, 20))
    last_activity_at = updated_at - dt.timedelta(hours=random.randint(0, 72))
    is_active = random.choice([True, False])

    phone = None
    if random.randint(1, 100) > 35:
        phone = "09" + str(random.randint(10_000_000, 99_999_999))

    return {
        "id": user_id,
        "email": f"user{number}.{user_id[:8]}@example.com",
        "username": f"hanzi_user_{number}",
        "phoneNumber": phone,
        "isActive": is_active,
        "createdAt": iso(created_at),
        "updatedAt": iso(updated_at),
        "currentLevel": level,
        "lastLogin": iso(updated_at),
        "lastActivityAt": iso(last_activity_at),
        "avatarUrl": None,
    }


def bulk_seed(elastic_url: str, index_name: str, count: int, headers: dict[str, str], insecure: bool) -> tuple[bool, str]:
    lines: list[str] = []
    for i in range(1, count + 1):
        doc = fake_user(i)
        action = {"index": {"_index": index_name, "_id": doc["id"]}}
        lines.append(json.dumps(action, separators=(",", ":")))
        lines.append(json.dumps(doc, separators=(",", ":")))

    payload = ("\n".join(lines) + "\n").encode("utf-8")
    url = f"{elastic_url.rstrip('/')}/_bulk?refresh=true"

    req_headers = dict(headers)
    req_headers["Content-Type"] = "application/x-ndjson"

    _, body = http_request("POST", url, req_headers, payload, insecure)
    response = json.loads(body.decode("utf-8"))

    if response.get("errors"):
        return False, json.dumps(response.get("items", [])[:5], ensure_ascii=False)

    return True, ""


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Seed fake users into Elasticsearch hanzi_users index.")
    parser.add_argument("--count", type=int, default=100, help="Number of fake users to create.")
    parser.add_argument("--elastic-url", default="http://localhost:9200", help="Elasticsearch base URL.")
    parser.add_argument("--index-name", default="hanzi_users", help="Target index name.")
    parser.add_argument("--username", default="elastic", help="Basic auth username.")
    parser.add_argument("--password", required=True, help="Basic auth password.")
    parser.add_argument("--insecure", action="store_true", help="Skip TLS cert validation (local/self-signed only).")
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    if args.count <= 0:
        print("Count must be greater than 0.", file=sys.stderr)
        return 2

    headers = {
        "Authorization": build_auth_header(args.username, args.password),
    }

    try:
        if not index_exists(args.elastic_url, args.index_name, headers, args.insecure):
            create_index(args.elastic_url, args.index_name, headers, args.insecure)
            print(f"Created index '{args.index_name}'.")
        else:
            print(f"Index '{args.index_name}' already exists.")

        ok, details = bulk_seed(args.elastic_url, args.index_name, args.count, headers, args.insecure)
        if not ok:
            print("Seed completed with errors. Sample failed items:", file=sys.stderr)
            print(details, file=sys.stderr)
            return 1

        print(f"Seed successful. Indexed {args.count} users into '{args.index_name}'.")
        return 0
    except urllib.error.HTTPError as ex:
        body = ex.read().decode("utf-8", errors="ignore") if hasattr(ex, "read") else ""
        print(f"HTTP error {ex.code}: {body}", file=sys.stderr)
        return 1
    except Exception as ex:
        print(f"Unexpected error: {ex}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
