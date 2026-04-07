# Database Schema Documentation

> Last updated: April 2026  
> This document describes the relational database schema for the Chinese learning platform.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Module: Auth](#module-auth)
3. [Module: User](#module-user)
4. [Module: Lesson](#module-lesson)
5. [Module: Classroom](#module-classroom)
6. [Module: Flashcard](#module-flashcard)
7. [Module: Payment](#module-payment)
8. [Module: AI](#module-ai)
9. [Module: Notification](#module-notification)
10. [Module: Search](#module-search)
11. [Cross-Module References](#cross-module-references)

---

## Architecture Overview

### Design Principles

- **Module Boundary Rule**:
  - ✅ Foreign keys **within same module** → Keep constraint
  - ❌ Foreign keys **across modules** → Use soft reference (ID only, no constraint)
  - Cross-module consistency handled via **Outbox Pattern** + Domain Events

- **Consistency Strategy**: Event-driven architecture ensures eventual consistency across module boundaries

### Modules

| Module           | Purpose                             | Key Aggregates                                 |
| ---------------- | ----------------------------------- | ---------------------------------------------- |
| **Auth**         | User authentication & authorization | User, Role, RefreshToken                       |
| **User**         | User profile & quota management     | UserAiUsage                                    |
| **Lesson**       | Course content & exercises          | Course, Topic, Exercise                        |
| **Classroom**    | Live classes & classroom management | LiveClass, Classroom, Assignment               |
| **Flashcard**    | Spaced Repetition System (SRS)      | Flashcard                                      |
| **Payment**      | Payments & subscriptions            | Payment, UserSubscription, CourseAccess        |
| **AI**           | Async AI jobs & usage tracking      | AiJob, AiUsageLog                              |
| **Notification** | Announcements & notifications       | Announcement, Notification                     |
| **Search**       | Full-text search (Elasticsearch)    | _(Stored in Elasticsearch, not relational DB)_ |

---

## Module: Auth

**Purpose**: ASP.NET Identity integration + session management

### Table: `users`

Maps to physical table: `AspNetUsers` (ASP.NET Identity)

| Column                      | Type       | Notes                                    |
| --------------------------- | ---------- | ---------------------------------------- |
| `id`                        | `uuid`     | Primary key, GUID from IdentityUser.Id   |
| `username`                  | `varchar`  | Unique username                          |
| `normalized_user_name`      | `varchar`  | Uppercase for search                     |
| `email`                     | `varchar`  | Primary email                            |
| `normalized_email`          | `varchar`  | Normalized email                         |
| `phone_number`              | `varchar`  | Optional                                 |
| `email_confirmed`           | `boolean`  | Default: false                           |
| `phone_number_confirmed`    | `boolean`  | Default: false                           |
| `is_active`                 | `boolean`  | Account activation flag (default: false) |
| `activate_code`             | `varchar`  | OTP/activation code                      |
| `expire_time_activate_code` | `datetime` | Code expiration                          |
| `last_login`                | `datetime` | Last login timestamp                     |
| `last_time_change_email`    | `datetime` | Email change audit                       |
| `last_time_change_password` | `datetime` | Password change audit                    |
| `created_at`                | `datetime` | Account creation time                    |
| `updated_at`                | `datetime` | Last update time                         |

### Table: `roles`

Maps to physical table: `AspNetRoles`

| Column            | Type      | Notes                                                 |
| ----------------- | --------- | ----------------------------------------------------- |
| `id`              | `varchar` | Role identifier (e.g., "Admin", "Teacher", "Student") |
| `name`            | `varchar` | Display name                                          |
| `normalized_name` | `varchar` | Uppercase for search                                  |

### Table: `user_roles`

Maps to physical table: `AspNetUserRoles` (Junction table)

| Column    | Type      | Notes         |
| --------- | --------- | ------------- |
| `user_id` | `uuid`    | FK → users.id |
| `role_id` | `varchar` | FK → roles.id |

**Indexes**:

- Primary Key: `(user_id, role_id)`
- Index on `user_id`
- Index on `role_id`

### Table: `refresh_tokens`

Manages JWT refresh token lifecycle

| Column       | Type       | Notes                            |
| ------------ | ---------- | -------------------------------- |
| `id`         | `int`      | Auto-increment primary key       |
| `user_id`    | `uuid`     | FK → users.id                    |
| `token`      | `varchar`  | Unique refresh token value       |
| `expires_at` | `datetime` | Token expiration time            |
| `is_revoked` | `boolean`  | Revocation flag (default: false) |
| `created_at` | `datetime` | Token issued at                  |

**Indexes**:

- Index on `token` (for lookup)
- Index on `user_id` (for user's token history)

### Table: `outbox_messages`

Outbox pattern for event publishing (Auth module)

| Column             | Type       | Notes                                                |
| ------------------ | ---------- | ---------------------------------------------------- |
| `id`               | `uuid`     | Primary key                                          |
| `type`             | `varchar`  | Event type (e.g., "UserCreated", "UserRoleAssigned") |
| `payload`          | `json`     | Event data                                           |
| `occurred_on_utc`  | `datetime` | Event timestamp                                      |
| `processed_on_utc` | `datetime` | Processing timestamp (NULL before processing)        |
| `error`            | `text`     | Error message if processing failed                   |
| `retry_count`      | `int`      | Retry attempt counter (default: 0)                   |

**Indexes**:

- Index on `occurred_on_utc` (for polling)
- Index on `processed_on_utc` (for unprocessed events)

---

## Module: User

**Purpose**: Extended user profiles and quota management

### Table: `user_ai_usage`

Tracks daily AI message quota consumption

| Column             | Type       | Notes                              |
| ------------------ | ---------- | ---------------------------------- |
| `id`               | `uuid`     | Primary key                        |
| `user_id`          | `uuid`     | Soft ref → users.id (cross-module) |
| `usage_date`       | `date`     | Date of usage                      |
| `ai_messages_used` | `int`      | Messages used today (default: 0)   |
| `reset_at`         | `datetime` | Next reset time                    |

**Indexes**:

- Unique index: `(user_id, usage_date)`

**Purpose**: Check quota in real-time (high read frequency)

---

## Module: Lesson

**Purpose**: Course content management (hierarchical: Course → Topic → Exercise)

### Design Notes

- **No static files**: All content stored in database
- **No theory sections**: Learning happens through exercises
- **No XP/score system**: Just tracking accuracy per topic
- **Topic types**:
  - `learning`: General topics (e.g., "Office Communication", "Travel", "Shopping")
  - `exam`: Exam papers (e.g., "HSK1-2019", "HSK1-2020")

### Table: `course`

Root aggregate for courses

| Column          | Type       | Notes                                                 |
| --------------- | ---------- | ----------------------------------------------------- |
| `id`            | `uuid`     | Primary key                                           |
| `created_by`    | `uuid`     | Soft ref → users.id (creator: admin or teacher)       |
| `title`         | `varchar`  | Course name                                           |
| `hsk_level`     | `int`      | 1-6 or NULL if not HSK-specific                       |
| `description`   | `text`     | Course description                                    |
| `thumbnail_url` | `text`     | Course cover image                                    |
| `course_type`   | `enum`     | `video` (self-paced) or `live_practice` (live drills) |
| `is_published`  | `boolean`  | Publication status (default: false)                   |
| `order_index`   | `int`      | Display order                                         |
| `created_at`    | `datetime` | Creation timestamp                                    |
| `updated_at`    | `datetime` | Last update                                           |

### Table: `topic`

Topics within a course (acts as catalog/category)

| Column              | Type       | Notes                                         |
| ------------------- | ---------- | --------------------------------------------- |
| `id`                | `uuid`     | Primary key                                   |
| `course_id`         | `uuid`     | FK → course.id                                |
| `title`             | `varchar`  | Topic name (e.g., "Food", "Family", "Travel") |
| `topic_type`        | `enum`     | `learning` or `exam` (default: learning)      |
| `description`       | `text`     | Topic description                             |
| `estimated_minutes` | `int`      | Estimated practice time                       |
| `exam_year`         | `int`      | For exam topics: year (e.g., 2019, 2020)      |
| `exam_code`         | `varchar`  | For exam topics: code (e.g., "HSK1-2020-01")  |
| `order_index`       | `int`      | Display order                                 |
| `is_published`      | `boolean`  | Publication status (default: false)           |
| `created_at`        | `datetime` | Creation timestamp                            |
| `updated_at`        | `datetime` | Last update                                   |

**Indexes**:

- Index on `course_id`
- Index on `exam_code`
- Index on `(course_id, order_index)`

### Table: `exercise`

Individual exercise questions

| Column           | Type       | Notes                                             |
| ---------------- | ---------- | ------------------------------------------------- |
| `id`             | `uuid`     | Primary key                                       |
| `topic_id`       | `uuid`     | FK → topic.id                                     |
| `created_by`     | `uuid`     | Soft ref → users.id (NULL = system, else teacher) |
| `title`          | `text`     | Exercise description                              |
| `type`           | `enum`     | Exercise format (see types below)                 |
| `skill`          | `enum`     | Target skill: `listen`, `read`, `write`           |
| `question`       | `text`     | Question text or prompt                           |
| `options`        | `json`     | For choice exercises: `[{id, text}, ...]`         |
| `audio_url`      | `varchar`  | For listening exercises                           |
| `correct_answer` | `text`     | Correct answer (NULL if AI-graded)                |
| `explanation`    | `text`     | Explanation shown after answering                 |
| `difficulty`     | `enum`     | `easy`, `medium`, `hard` (default: medium)        |
| `order_index`    | `int`      | Display order within topic                        |
| `is_published`   | `boolean`  | Publication status (default: false)               |
| `created_at`     | `datetime` | Creation timestamp                                |

**Exercise Types**:

| Type                     | Skill  | Input        | Description                               |
| ------------------------ | ------ | ------------ | ----------------------------------------- |
| `listen_dialogue_choice` | Listen | Audio + MCQ  | Listen to dialogue, choose correct answer |
| `listen_sentence_judge`  | Listen | Audio + T/F  | Listen to sentence, judge true/false      |
| `listen_fill_blank`      | Listen | Audio + Text | Listen and fill blanks                    |
| `read_fill_blank`        | Read   | Text         | Read and fill blanks                      |
| `read_comprehension`     | Read   | Passage      | Read passage, answer questions            |
| `read_sentence_order`    | Read   | Sentences    | Order sentences correctly                 |
| `read_match`             | Read   | Phrases      | Match phrases or definitions              |
| `write_hanzi`            | Write  | Character    | Write Chinese character (stroke order)    |
| `write_sentence`         | Write  | Prompt       | Write complete sentence (AI feedback)     |
| `write_pinyin`           | Write  | Hanzi        | Write pinyin for character                |

**Indexes**:

- Index on `topic_id`
- Index on `(topic_id, order_index)`

### Table: `user_exercise_session`

Practice session (phiên làm bài) — Group multiple exercises or start standalone

| Column               | Type       | Notes                                                          |
| -------------------- | ---------- | -------------------------------------------------------------- |
| `id`                 | `uuid`     | Primary key                                                    |
| `user_id`            | `uuid`     | Soft ref → users.id                                            |
| `topic_id`           | `uuid`     | FK → topic.id (optional, for session context)                  |
| `status`             | `enum`     | `in_progress`, `completed`, `abandoned` (default: in_progress) |
| `total_score`        | `float`    | Cumulative score for session (nullable, calculated)            |
| `started_at`         | `datetime` | Session start time                                             |
| `completed_at`       | `datetime` | Session completion time (nullable)                             |
| `time_spent_seconds` | `int`      | Total time for session                                         |

**Indexes**:

- Index on `user_id`
- Index on `topic_id`
- Index on `(user_id, status)` (for active sessions)
- Index on `started_at` (for timeline queries)

### Table: `exercise_attempt`

Individual exercise attempt within a session

| Column        | Type         | Notes                                    |
| ------------- | ------------ | ---------------------------------------- |
| `id`          | `uuid`       | Primary key                              |
| `session_id`  | `uuid`       | FK → user_exercise_session.id            |
| `exercise_id` | `uuid`       | Soft ref → exercise.id (cross-module)    |
| `answer`      | `text`       | User's answer                            |
| `score`       | `float(8,2)` | Score (0-100)                            |
| `is_correct`  | `boolean`    | Pass/fail flag                           |
| `ai_feedback` | `text`       | Feedback from AI (for writing exercises) |
| `created_at`  | `datetime`   | Attempt timestamp                        |

**Indexes**:

- FK on `session_id` (→ user_exercise_session.id)
- Index on `session_id`
- Index on `exercise_id`
- Index on `(session_id, exercise_id)` (for unique per session)

### Table: `user_topic_progress`

Aggregated practice statistics per user per topic

| Column              | Type         | Notes                                 |
| ------------------- | ------------ | ------------------------------------- |
| `id`                | `uuid`       | Primary key                           |
| `user_id`           | `uuid`       | Soft ref → users.id                   |
| `topic_id`          | `uuid`       | FK → topic.id                         |
| `total_attempts`    | `int`        | Total practice sessions (default: 0)  |
| `total_answered`    | `int`        | Total questions answered (default: 0) |
| `total_correct`     | `int`        | Total correct answers (default: 0)    |
| `total_wrong`       | `int`        | Total incorrect answers (default: 0)  |
| `total_score`       | `float`      | Cumulative score (default: 0)         |
| `accuracy_rate`     | `float(5,2)` | Percentage correct (nullable)         |
| `last_practiced_at` | `datetime`   | Last practice time                    |
| `created_at`        | `datetime`   | First practice                        |
| `updated_at`        | `datetime`   | Last update                           |

**Indexes**:

- Unique index: `(user_id, topic_id)`
- Index on `user_id`
- Index on `topic_id`

---

## Module: Classroom

**Purpose**: Live classes and classroom management

### Table: `live_class`

Enrollment-based live class offering

| Column                 | Type           | Notes                                                                                       |
| ---------------------- | -------------- | ------------------------------------------------------------------------------------------- |
| `id`                   | `uuid`         | Primary key                                                                                 |
| `teacher_id`           | `uuid`         | Soft ref → users.id                                                                         |
| `title`                | `varchar`      | Class name (e.g., "HSK1 Prep T3-T5 2025")                                                   |
| `description`          | `text`         | Class description                                                                           |
| `hsk_level`            | `int`          | 1-6 or NULL                                                                                 |
| `thumbnail_url`        | `text`         | Class image                                                                                 |
| `registration_start`   | `datetime`     | Enrollment opens                                                                            |
| `registration_end`     | `datetime`     | Enrollment closes                                                                           |
| `max_students`         | `int`          | Capacity (NULL = unlimited)                                                                 |
| `class_start`          | `datetime`     | First class date                                                                            |
| `class_end`            | `datetime`     | Last class date                                                                             |
| `schedule_description` | `varchar`      | (e.g., "Tu-Th 7-9pm, 8 weeks")                                                              |
| `price`                | `decimal(8,2)` | Course fee                                                                                  |
| `currency`             | `enum`         | `VND` or `USD` (default: VND)                                                               |
| `status`               | `enum`         | `upcoming`, `registration_open`, `registration_closed`, `ongoing`, `completed`, `cancelled` |
| `created_at`           | `datetime`     | Creation time                                                                               |
| `updated_at`           | `datetime`     | Last update                                                                                 |

**Indexes**:

- Index on `teacher_id`
- Index on `status`

### Table: `live_class_enrollment`

Student enrollment in live class

| Column          | Type       | Notes                                |
| --------------- | ---------- | ------------------------------------ |
| `id`            | `uuid`     | Primary key                          |
| `live_class_id` | `uuid`     | FK → live_class.id                   |
| `student_id`    | `uuid`     | Soft ref → users.id                  |
| `status`        | `enum`     | `pending`, `confirmed`, `cancelled`  |
| `enrolled_at`   | `datetime` | Enrollment time                      |
| `confirmed_at`  | `datetime` | Confirmation time (NULL if pending)  |
| `payment_id`    | `uuid`     | Soft ref → payment.id (cross-module) |

**Indexes**:

- Unique index: `(live_class_id, student_id)`
- Index on `live_class_id`
- Index on `student_id`

### Table: `classroom`

Classroom workspace (created from live_class or standalone)

| Column          | Type       | Notes                                                    |
| --------------- | ---------- | -------------------------------------------------------- |
| `id`            | `uuid`     | Primary key                                              |
| `teacher_id`    | `uuid`     | Soft ref → users.id                                      |
| `live_class_id` | `uuid`     | Soft ref → live_class.id (NULL if created independently) |
| `name`          | `varchar`  | Classroom name                                           |
| `description`   | `text`     | Description                                              |
| `is_active`     | `boolean`  | Active flag (default: true)                              |
| `created_at`    | `datetime` | Creation time                                            |

**Indexes**:

- Index on `teacher_id`

### Table: `classroom_student`

Student membership in classroom

| Column         | Type       | Notes                                   |
| -------------- | ---------- | --------------------------------------- |
| `id`           | `uuid`     | Primary key                             |
| `classroom_id` | `uuid`     | FK → classroom.id                       |
| `student_id`   | `uuid`     | Soft ref → users.id                     |
| `added_by`     | `uuid`     | Soft ref → users.id (teacher who added) |
| `status`       | `enum`     | `active`, `removed`                     |
| `added_at`     | `datetime` | Addition time                           |

**Indexes**:

- Unique index: `(classroom_id, student_id)`
- Index on `classroom_id`
- Index on `student_id`

### Table: `assignment`

Assignment (homework/project)

| Column            | Type       | Notes                                                      |
| ----------------- | ---------- | ---------------------------------------------------------- |
| `id`              | `uuid`     | Primary key                                                |
| `classroom_id`    | `uuid`     | FK → classroom.id                                          |
| `teacher_id`      | `uuid`     | Soft ref → users.id                                        |
| `title`           | `varchar`  | Assignment name                                            |
| `description`     | `text`     | Instructions                                               |
| `skill_focus`     | `enum`     | `listen`, `read`, `write`, `mixed`                         |
| `assignment_type` | `enum`     | `class` (all students) or `individual` (specific students) |
| `due_date`        | `datetime` | Deadline (nullable)                                        |
| `is_published`    | `boolean`  | Publication status (default: false)                        |
| `created_at`      | `datetime` | Creation time                                              |
| `updated_at`      | `datetime` | Last update                                                |

**Indexes**:

- Index on `classroom_id`
- Index on `teacher_id`

### Table: `assignment_exercise`

Exercises included in an assignment

| Column          | Type   | Notes                                 |
| --------------- | ------ | ------------------------------------- |
| `assignment_id` | `uuid` | FK → assignment.id (PK component)     |
| `exercise_id`   | `uuid` | Soft ref → exercise.id (PK component) |
| `order_index`   | `int`  | Question order                        |

**Indexes**:

- Primary Key: `(assignment_id, exercise_id)`
- Index on `assignment_id`

### Table: `assignment_recipient`

Specific students assigned (for `assignment_type = "individual"`)

| Column          | Type   | Notes                              |
| --------------- | ------ | ---------------------------------- |
| `assignment_id` | `uuid` | FK → assignment.id (PK component)  |
| `student_id`    | `uuid` | Soft ref → users.id (PK component) |

**Indexes**:

- Primary Key: `(assignment_id, student_id)`

### Table: `assignment_submission`

Student's submission for an assignment

| Column             | Type       | Notes                                                      |
| ------------------ | ---------- | ---------------------------------------------------------- |
| `id`               | `uuid`     | Primary key                                                |
| `assignment_id`    | `uuid`     | Soft ref → assignment.id (different Aggregate Root, no FK) |
| `student_id`       | `uuid`     | Soft ref → users.id                                        |
| `status`           | `enum`     | `not_started`, `in_progress`, `submitted`, `graded`        |
| `submitted_at`     | `datetime` | Submission time (nullable)                                 |
| `total_score`      | `float`    | Total score (nullable)                                     |
| `teacher_feedback` | `text`     | Grader's comments                                          |
| `graded_at`        | `datetime` | Grading time (nullable)                                    |
| `created_at`       | `datetime` | Creation time                                              |
| `updated_at`       | `datetime` | Last update                                                |

**Indexes**:

- Unique index: `(assignment_id, student_id)`
- Index on `assignment_id`
- Index on `student_id`

### Table: `submission_answer`

Individual answer within a submission

| Column          | Type       | Notes                                |
| --------------- | ---------- | ------------------------------------ |
| `id`            | `uuid`     | Primary key                          |
| `submission_id` | `uuid`     | FK → assignment_submission.id        |
| `exercise_id`   | `uuid`     | Soft ref → exercise.id               |
| `answer`        | `text`     | Student's answer                     |
| `is_correct`    | `boolean`  | Correctness flag (nullable)          |
| `score`         | `float`    | Points earned (nullable)             |
| `ai_feedback`   | `text`     | AI-generated feedback (writing only) |
| `created_at`    | `datetime` | Submission time                      |

**Indexes**:

- Index on `submission_id`
- Index on `(submission_id, exercise_id)`

---

## Module: Flashcard

**Purpose**: Spaced Repetition System (SRS) for vocabulary practice

### Design Notes

- Flashcards auto-created when topic completed or user selects review
- Requires subscription for SRS access
- Supports characters, phrases, and sentences
- Includes stroke order data for hanzi writing practice

### Table: `flashcard`

Vocabulary flashcard entry

| Column              | Type       | Notes                                                     |
| ------------------- | ---------- | --------------------------------------------------------- |
| `id`                | `uuid`     | Primary key                                               |
| `text_cn`           | `text`     | Chinese text (e.g., "你好" or "你好，我叫小明")           |
| `pinyin`            | `text`     | Romanization (e.g., "nǐ hǎo")                             |
| `meaning_vi`        | `text`     | Vietnamese meaning                                        |
| `meaning_en`        | `text`     | English meaning (optional)                                |
| `audio_url`         | `varchar`  | Audio pronunciation                                       |
| `hsk_level`         | `int`      | 1-6 or NULL                                               |
| `is_hsk_core`       | `boolean`  | Part of official HSK vocabulary (default: false)          |
| `phrase_type`       | `enum`     | `word`, `phrase`, `sentence`                              |
| `stroke_order_json` | `json`     | SVG path data (Hanzi Writer format, word only)            |
| `radical`           | `varchar`  | Base radical component (word only)                        |
| `stroke_count`      | `int`      | Number of strokes (word only)                             |
| `is_traditional`    | `boolean`  | Simplified (false) or Traditional (true) (default: false) |
| `course_id`         | `uuid`     | Soft ref → course.id                                      |
| `topic_id`          | `uuid`     | Soft ref → topic.id (optional, specific topic)            |
| `created_at`        | `datetime` | Creation time                                             |
| `updated_at`        | `datetime` | Last update                                               |

**Indexes**:

- Index on `(course_id, topic_id)`

---

## Module: Payment

**Purpose**: Payment processing, subscriptions, and course access control

### Design Notes

- **Pricing Model**:
  - Course: One-time purchase (1 year access)
  - Subscription: Monthly recurring
- **Payment Methods**: Momo, VNPay, Stripe, Bank Transfer
- **Access Expiration**: Triggers progress reset via Outbox event

### Table: `subscription_plan`

Subscription pricing tier

| Column                 | Type       | Notes                                                |
| ---------------------- | ---------- | ---------------------------------------------------- |
| `id`                   | `uuid`     | Primary key                                          |
| `currency`             | `enum`     | `VND` or `USD`                                       |
| `name`                 | `text`     | Plan name (e.g., "Basic Monthly", "Premium Monthly") |
| `price`                | `decimal`  | Monthly/plan price                                   |
| `duration_days`        | `int`      | Plan duration (e.g., 30 for 1 month)                 |
| `ai_messages_per_day`  | `int`      | Daily AI message quota                               |
| `max_hsk_level_access` | `int`      | Max HSK level (1-6) available in this plan           |
| `is_active`            | `boolean`  | Active plan flag (default: true)                     |
| `created_at`           | `datetime` | Creation time                                        |

### Table: `user_subscription`

Active subscription for a user

| Column         | Type       | Notes                                       |
| -------------- | ---------- | ------------------------------------------- |
| `id`           | `uuid`     | Primary key                                 |
| `plan_id`      | `uuid`     | FK → subscription_plan.id                   |
| `user_id`      | `uuid`     | Soft ref → users.id                         |
| `status`       | `enum`     | `active`, `expired`, `cancelled`, `pending` |
| `started_at`   | `datetime` | Subscription start                          |
| `expires_at`   | `datetime` | Subscription end                            |
| `cancelled_at` | `datetime` | Cancellation time (nullable)                |
| `auto_renew`   | `boolean`  | Auto-renewal flag (default: false)          |
| `created_at`   | `datetime` | Creation time                               |
| `updated_at`   | `datetime` | Last update                                 |

### Table: `payment`

Payment transaction record

| Column                  | Type           | Notes                                      |
| ----------------------- | -------------- | ------------------------------------------ |
| `id`                    | `uuid`         | Primary key                                |
| `user_id`               | `uuid`         | Soft ref → users.id                        |
| `amount`                | `decimal(8,2)` | Transaction amount                         |
| `currency`              | `enum`         | `VND` or `USD`                             |
| `status`                | `enum`         | `pending`, `success`, `failed`             |
| `payment_method`        | `enum`         | `momo`, `vnpay`, `stripe`, `bank_transfer` |
| `transaction_id`        | `varchar`      | Unique gateway transaction ID              |
| `gateway_response_json` | `json`         | Gateway response payload                   |
| `paid_at`               | `datetime`     | Payment completion time (nullable)         |
| `created_at`            | `datetime`     | Creation time                              |

**Indexes**:

- Index on `user_id`
- Unique index on `transaction_id`

### Table: `payment_item`

Polymorphic item in a payment (what was purchased)

| Column       | Type           | Notes                                  |
| ------------ | -------------- | -------------------------------------- |
| `id`         | `uuid`         | Primary key                            |
| `payment_id` | `uuid`         | FK → payment.id                        |
| `item_type`  | `enum`         | `course`, `subscription`, `live_class` |
| `item_id`    | `uuid`         | Soft ref to the purchased item         |
| `amount`     | `decimal(8,2)` | Amount for this item                   |
| `created_at` | `datetime`     | Time                                   |

**Indexes**:

- Index on `payment_id`
- Index on `(item_type, item_id)`

### Table: `course_access`

User's access permission for a course

| Column            | Type       | Notes                                                 |
| ----------------- | ---------- | ----------------------------------------------------- |
| `id`              | `uuid`     | Primary key                                           |
| `user_id`         | `uuid`     | Soft ref → users.id                                   |
| `course_id`       | `uuid`     | Soft ref → course.id                                  |
| `access_type`     | `enum`     | `purchased` (1-year) or `subscription` (monthly)      |
| `payment_id`      | `uuid`     | FK → payment.id (nullable for subscription)           |
| `subscription_id` | `uuid`     | FK → user_subscription.id (nullable for purchase)     |
| `expires_at`      | `datetime` | Access expiration time                                |
| `is_expired`      | `boolean`  | Expiry flag; triggers progress reset (default: false) |
| `granted_at`      | `datetime` | Access grant time                                     |

**Indexes**:

- Unique index: `(user_id, course_id)`
- Index on `user_id`
- Index on `expires_at` (for expiration checks)

### Table: `payment_webhook_log`

Webhook event logging for idempotency and debugging

| Column          | Type       | Notes                                |
| --------------- | ---------- | ------------------------------------ |
| `id`            | `uuid`     | Primary key                          |
| `provider`      | `enum`     | `momo`, `vnpay`, `stripe`            |
| `event_type`    | `varchar`  | Event name from provider             |
| `payload_json`  | `json`     | Webhook payload                      |
| `is_processed`  | `boolean`  | Processing flag (default: false)     |
| `is_duplicate`  | `boolean`  | Duplicate detection (default: false) |
| `payment_id`    | `uuid`     | FK → payment.id (nullable)           |
| `error_message` | `text`     | Error details if processing failed   |
| `received_at`   | `datetime` | Receipt timestamp                    |
| `processed_at`  | `datetime` | Processing timestamp (nullable)      |

---

## Module: AI

**Purpose**: Asynchronous AI jobs and usage quota tracking

### Design Notes

- **Job Types**: ASR, TTS, Pronunciation feedback, Writing feedback, Content generation
- **Idempotency**: `idempotency_key` prevents duplicate processing
- **Retry Strategy**: Automatic with exponential backoff

### Table: `ai_job`

Async AI task

| Column            | Type       | Notes                                                                   |
| ----------------- | ---------- | ----------------------------------------------------------------------- |
| `id`              | `uuid`     | Primary key                                                             |
| `user_id`         | `uuid`     | Soft ref → users.id                                                     |
| `job_type`        | `enum`     | `ASR`, `TTS`, `PRONUNCIATION`, `WRITING_FEEDBACK`, `CONTENT_GENERATION` |
| `model_provider`  | `varchar`  | AI provider (e.g., "OpenAI", "Azure")                                   |
| `model_version`   | `varchar`  | Model version (e.g., "gpt-4", "whisper-1")                              |
| `idempotency_key` | `varchar`  | Unique request ID for idempotency                                       |
| `input_payload`   | `json`     | Job input parameters                                                    |
| `status`          | `enum`     | `pending`, `processing`, `done`, `failed`                               |
| `finished_at`     | `datetime` | Completion time (nullable)                                              |
| `result_payload`  | `json`     | Job result (nullable)                                                   |
| `error_message`   | `text`     | Error details (nullable)                                                |
| `retry_count`     | `int`      | Number of retries (default: 0)                                          |
| `max_retries`     | `int`      | Maximum retry attempts (default: 3)                                     |
| `priority`        | `int`      | Priority level (default: 0)                                             |
| `created_at`      | `datetime` | Creation time                                                           |
| `updated_at`      | `datetime` | Last update                                                             |

**Indexes**:

- Unique index on `idempotency_key`

### Table: `ai_usage_log`

Usage quota and cost tracking

| Column                | Type            | Notes                                                                   |
| --------------------- | --------------- | ----------------------------------------------------------------------- |
| `id`                  | `uuid`          | Primary key                                                             |
| `user_id`             | `uuid`          | Soft ref → users.id                                                     |
| `job_id`              | `uuid`          | FK → ai_job.id                                                          |
| `model_name`          | `varchar`       | Model used                                                              |
| `usage_type`          | `enum`          | `tts`, `pronunciation`, `asr`, `writing_feedback`, `content_generation` |
| `related_entity_type` | `varchar`       | Context (e.g., "Exercise", "Assignment")                                |
| `related_entity_id`   | `uuid`          | Entity ID                                                               |
| `tokens_used`         | `int`           | Token consumption                                                       |
| `cost_usd`            | `decimal(10,6)` | Cost in USD                                                             |
| `created_at`          | `datetime`      | Log time                                                                |

---

## Module: Notification

**Purpose**: Announcements and user notifications

### Design Notes

- **Announcements**: Broadcast messages to users/roles
- **Notifications**: Individual user inbox items
- **Deduplication**: `dedupe_key` prevents duplicate notifications

### Table: `announcement`

Broadcast announcement

| Column         | Type       | Notes                                                                                                                                                  |
| -------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `id`           | `uuid`     | Primary key                                                                                                                                            |
| `created_by`   | `uuid`     | Soft ref → users.id                                                                                                                                    |
| `title`        | `varchar`  | Announcement title                                                                                                                                     |
| `body`         | `text`     | Content                                                                                                                                                |
| `type`         | `enum`     | `assignment_due`, `assignment_graded`, `live_class_reminder`, `live_class_start`, `course_expiring`, `flashcard_due`, `enrollment_confirmed`, `system` |
| `target_type`  | `enum`     | `all_users`, `role`, `manual` (default: all_users)                                                                                                     |
| `target_roles` | `json`     | Roles filter (nullable, e.g., ["student", "teacher"])                                                                                                  |
| `status`       | `enum`     | `draft`, `scheduled`, `published`, `cancelled` (default: draft)                                                                                        |
| `scheduled_at` | `datetime` | Scheduled publish time (nullable)                                                                                                                      |
| `published_at` | `datetime` | Actual publish time (nullable)                                                                                                                         |
| `expires_at`   | `datetime` | Expiration time (nullable)                                                                                                                             |
| `created_at`   | `datetime` | Creation time                                                                                                                                          |
| `updated_at`   | `datetime` | Last update                                                                                                                                            |

**Indexes**:

- Index on `status`
- Index on `created_by`
- Index on `(status, scheduled_at)`

### Table: `announcement_recipient`

Tracks delivery status for each recipient

| Column            | Type       | Notes                                                       |
| ----------------- | ---------- | ----------------------------------------------------------- |
| `id`              | `uuid`     | Primary key                                                 |
| `announcement_id` | `uuid`     | FK → announcement.id                                        |
| `user_id`         | `uuid`     | Soft ref → users.id                                         |
| `status`          | `enum`     | `pending`, `delivered`, `read`, `failed` (default: pending) |
| `delivered_at`    | `datetime` | Delivery time (nullable)                                    |
| `read_at`         | `datetime` | Read time (nullable)                                        |
| `error_message`   | `text`     | Error details if failed                                     |
| `created_at`      | `datetime` | Creation time                                               |

**Indexes**:

- Unique index: `(announcement_id, user_id)`
- Index on `user_id`
- Index on `(user_id, status)`

### Table: `notification`

Individual user notification (from announcements or system events)

| Column            | Type       | Notes                                                                                                                                                  |
| ----------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `id`              | `uuid`     | Primary key                                                                                                                                            |
| `user_id`         | `uuid`     | Soft ref → users.id                                                                                                                                    |
| `announcement_id` | `uuid`     | FK → announcement.id (nullable for broadcast notifications)                                                                                            |
| `type`            | `enum`     | `assignment_due`, `assignment_graded`, `live_class_reminder`, `live_class_start`, `course_expiring`, `flashcard_due`, `enrollment_confirmed`, `system` |
| `title`           | `varchar`  | Notification title                                                                                                                                     |
| `body`            | `text`     | Content                                                                                                                                                |
| `is_read`         | `boolean`  | Read flag (default: false)                                                                                                                             |
| `read_at`         | `datetime` | Read timestamp (nullable)                                                                                                                              |
| `expires_at`      | `datetime` | Expiration time (nullable)                                                                                                                             |
| `dedupe_key`      | `varchar`  | Idempotency key (e.g., outbox event ID)                                                                                                                |
| `payload_json`    | `json`     | Extra data for deep-linking (nullable)                                                                                                                 |
| `ref_type`        | `varchar`  | Related entity type (e.g., "assignment", "live_class")                                                                                                 |
| `ref_id`          | `uuid`     | Related entity ID                                                                                                                                      |
| `created_at`      | `datetime` | Creation time                                                                                                                                          |

**Indexes**:

- Unique index on `dedupe_key`
- Index on `announcement_id`
- Index on `user_id`
- Index on `(user_id, is_read)`
- Index on `(user_id, created_at)` (for timeline queries)

---

## Module: Search

**Purpose**: Full-text search capability

### Implementation

- **Storage**: Elasticsearch (not relational database)
- **Indexing**: Async indexing via Outbox events
- **Searchable Entities**: Courses, Topics, Exercises, Flashcards, Announcements

### Note

No relational tables; all search data stored in Elasticsearch indices managed by the Search module.

---

## Cross-Module References

### Soft References (No FK Constraint)

These references span module boundaries and are maintained via the **Outbox Pattern**:

| Source Table           | Column        | Target          | Notes                              |
| ---------------------- | ------------- | --------------- | ---------------------------------- |
| refresh_tokens         | user_id       | users.id        | Same module (Auth) but FK exists   |
| user_ai_usage          | user_id       | users.id        | Cross-module (User→Auth)           |
| live_class             | teacher_id    | users.id        | Cross-module (Classroom→Auth)      |
| live_class_enrollment  | student_id    | users.id        | Cross-module (Classroom→Auth)      |
| live_class_enrollment  | payment_id    | payment.id      | Cross-module (Classroom→Payment)   |
| classroom              | teacher_id    | users.id        | Cross-module (Classroom→Auth)      |
| classroom              | live_class_id | live_class.id   | Cross-module (Classroom→Classroom) |
| classroom_student      | student_id    | users.id        | Cross-module (Classroom→Auth)      |
| classroom_student      | added_by      | users.id        | Cross-module (Classroom→Auth)      |
| assignment             | teacher_id    | users.id        | Cross-module (Classroom→Auth)      |
| assignment_exercise    | exercise_id   | exercise.id     | Cross-module (Classroom→Lesson)    |
| assignment_recipient   | student_id    | users.id        | Cross-module (Classroom→Auth)      |
| assignment_submission  | student_id    | users.id        | Cross-module (Classroom→Auth)      |
| submission_answer      | exercise_id   | exercise.id     | Cross-module (Classroom→Lesson)    |
| course                 | created_by    | users.id        | Cross-module (Lesson→Auth)         |
| exercise               | created_by    | users.id        | Cross-module (Lesson→Auth)         |
| exercise_attempt       | user_id       | users.id        | Cross-module (Lesson→Auth)         |
| exercise_attempt       | exercise_id   | exercise.id     | Cross-module (Lesson→Lesson)       |
| user_topic_progress    | user_id       | users.id        | Cross-module (Lesson→Auth)         |
| flashcard              | course_id     | course.id       | Cross-module (Flashcard→Lesson)    |
| flashcard              | topic_id      | topic.id        | Cross-module (Flashcard→Lesson)    |
| payment                | user_id       | users.id        | Cross-module (Payment→Auth)        |
| payment_item           | item_id       | _(polymorphic)_ | Cross-module (Payment→various)     |
| course_access          | user_id       | users.id        | Cross-module (Payment→Auth)        |
| course_access          | course_id     | course.id       | Cross-module (Payment→Lesson)      |
| ai_job                 | user_id       | users.id        | Cross-module (AI→Auth)             |
| ai_usage_log           | user_id       | users.id        | Cross-module (AI→Auth)             |
| announcement           | created_by    | users.id        | Cross-module (Notification→Auth)   |
| announcement_recipient | user_id       | users.id        | Cross-module (Notification→Auth)   |
| notification           | user_id       | users.id        | Cross-module (Notification→Auth)   |

### Internal ForeignKeys (FK Constraint Maintained)

| Source Table           | Column          | Target                   | Module       |
| ---------------------- | --------------- | ------------------------ | ------------ |
| user_roles             | user_id         | users.id                 | Auth         |
| user_roles             | role_id         | roles.id                 | Auth         |
| refresh_tokens         | user_id         | users.id                 | Auth         |
| topic                  | course_id       | course.id                | Lesson       |
| exercise               | topic_id        | topic.id                 | Lesson       |
| user_topic_progress    | topic_id        | topic.id                 | Lesson       |
| live_class_enrollment  | live_class_id   | live_class.id            | Classroom    |
| classroom_student      | classroom_id    | classroom.id             | Classroom    |
| assignment             | classroom_id    | classroom.id             | Classroom    |
| assignment_exercise    | assignment_id   | assignment.id            | Classroom    |
| assignment_recipient   | assignment_id   | assignment.id            | Classroom    |
| submission_answer      | submission_id   | assignment_submission.id | Classroom    |
| user_subscription      | plan_id         | subscription_plan.id     | Payment      |
| payment_item           | payment_id      | payment.id               | Payment      |
| course_access          | payment_id      | payment.id               | Payment      |
| course_access          | subscription_id | user_subscription.id     | Payment      |
| payment_webhook_log    | payment_id      | payment.id               | Payment      |
| ai_usage_log           | job_id          | ai_job.id                | AI           |
| announcement_recipient | announcement_id | announcement.id          | Notification |
| notification           | announcement_id | announcement.id          | Notification |

---

## Data Flow Patterns

### User Course Learning Flow

1. User selects topic from course
2. Flashcard vocabulary preview (~30sec review)
3. Exercise attempts in sequence
4. Automatic feedback inline
5. Flashcard auto-created for SRS
6. Progress aggregated in `user_topic_progress`

### Live Class Registration Flow

1. Admin/Teacher creates `live_class` with registration window
2. Student enrolls → `live_class_enrollment` (status: pending)
3. Student completes payment → `payment` + `payment_item`
4. Payment confirmed → `live_class_enrollment` (status: confirmed)
5. Teacher creates `classroom` and adds confirmed students
6. Teacher assigns work via `assignment`

### Assignment Submission Flow

1. Teacher creates `assignment` with exercises
2. Teacher publishes for class or individuals
3. Student completes `assignment_submission`
4. Answers recorded in `submission_answer` per exercise
5. Auto-scoring (except `write_sentence`) → scores populated
6. AI feedback generated for writing (async via `ai_job`)
7. Teacher reviews and grades

### Subscription & Access Control Flow

1. User purchases subscription → `user_subscription` (active)
2. `course_access` created with `subscription_id`
3. Course content unlocked based on HSK level
4. Subscription expires → `course_access` (is_expired: true)
5. Outbox event triggers progress reset

### AI Job Processing

1. User action triggers AI need (e.g., write sentence feedback)
2. `ai_job` created (status: pending)
3. Background worker picks job, executes, updates status
4. `ai_usage_log` records consumption and cost
5. Quota checked against `user_ai_usage` (daily limit)

---

## Indexing Strategy

### High-Frequency Queries

| Query Pattern                        | Key Indexes                                          |
| ------------------------------------ | ---------------------------------------------------- |
| User dashboard: Latest notifications | `notification(user_id, created_at DESC)`             |
| Topic progress lookup                | `user_topic_progress(user_id, topic_id)`             |
| Exercise review                      | `exercise(topic_id, order_index)`                    |
| Student roster in classroom          | `classroom_student(classroom_id, status)`            |
| Unprocessed outbox events            | `outbox_messages(processed_on_utc, occurred_on_utc)` |
| Payment history                      | `payment(user_id, created_at DESC)`                  |
| Pending AI jobs                      | `ai_job(status, priority, created_at)`               |

### Unique Constraints

- `users(email)` — Email uniqueness
- `refresh_tokens(token)` — Token uniqueness
- `payment(transaction_id)` — Idempotency
- `user_topic_progress(user_id, topic_id)` — One record per user-topic pair
- `live_class_enrollment(live_class_id, student_id)` — One enrollment per user-class
- `classroom_student(classroom_id, student_id)` — One membership per user-classroom
- `assignment_submission(assignment_id, student_id)` — One submission per user-assignment
- `course_access(user_id, course_id)` — One access record per user-course pair
- `notification(dedupe_key)` — Prevents duplicate notifications
- `announcement_recipient(announcement_id, user_id)` — One delivery record per user-announcement

---

## Notes on Eventual Consistency

The system uses the **Outbox Pattern** for cross-module consistency:

1. Domain event occurs in one module
2. Event AND primary operation written in single transaction
3. Background processor polls `outbox_messages`
4. Event published to subscribers (other modules)
5. Subscriber updates its state based on event
6. Processed flag set when complete

This ensures:

- No distributed transactions
- No saga complexity
- Eventual consistency within SLA
- Simple rollback via retry

**Examples**:

- User enrolls in course → `user_subscription` created → Outbox event → `course_access` created in Payment module
- Assignment deadline reached → Outbox event → Notification module creates deadline reminder
- Course access expires → Outbox event → Lesson module resets progress

---

## Future Considerations

- **Audit Trail**: Consider adding `created_by` / `modified_by` / `modified_at` to all aggregate roots
- **Soft Deletes**: Add `deleted_at` timestamp for audit compliance
- **Data Archival**: Archive old `exercise_attempt` and `submission_answer` records quarterly
- **Caching Layer**: Redis cache for `user_topic_progress`, `flashcard` lookups
- **Search Enhancements**: Elasticsearch synonym expansion for exercise search

---

**End of Schema Documentation**
