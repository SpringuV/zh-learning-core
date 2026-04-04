# Migration Checklist (Module-First)

Muc tieu: tao migration theo thu tu giam xung dot, uu tien module nen va module co FK noi bo truoc.

## 1. Pre-check

- [ ] Chot schema trong `schema.dbml`.
- [ ] Dam bao cac cot soft-ref den `users.id` deu la `uuid`.
- [ ] Xac nhan module active trong solution: Auth, Users, Lesson, Classroom, Notification, Payment, Search.
- [ ] Search dung Elasticsearch, khong tao bang relational cho Search.

## 2. Baseline migration order

- [ ] Auth
  - users/roles/user_roles/refresh_tokens
  - outbox_messages (Auth outbox)
- [ ] Users
  - user_ai_usage
- [ ] Lesson
  - course, topic, exercise, exercise_attempt, user_topic_progress
- [ ] Classroom
  - live_class, live_class_enrollment
  - classroom, classroom_student
  - assignment, assignment_exercise, assignment_recipient
  - assignment_submission, submission_answer
- [ ] Payment
  - subscription_plan, user_subscription, payment, payment_item
  - course_access, payment_webhook_log
- [ ] Flashcard
  - flashcard
- [ ] AI
  - ai_job, ai_usage_log
- [ ] Notification
  - announcement, announcement_recipient, notification

## 3. Migration execution checklist

- [ ] Tao migration theo tung module (khong gop 1 migration qua lon).
- [ ] Review generated SQL: chi FK noi bo module, soft-ref khong tao FK.
- [ ] Apply migration tren dev database.
- [ ] Test startup + seed + API smoke test cho module vua migrate.
- [ ] Chay tiep migration module ke tiep.

## 4. Validation commands

- [ ] `dotnet restore backend/hanzi-anhvu.slnx`
- [ ] `dotnet build backend/hanzi-anhvu.slnx`
- [ ] `dotnet test backend/hanzi-anhvu.slnx`

## 5. Rollout safety

- [ ] Backup schema truoc khi apply tren staging/prod.
- [ ] Verify idempotency va outbox processing sau migration.
- [ ] Theo doi log loi migration/runtime 24h dau sau deploy.
